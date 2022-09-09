using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using essim_extension_core;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Json.Serialization;
using essim_extension_core.Domain;
using essim_extension_core.Helpers;
using Microsoft.Extensions.Logging;

namespace essim_engine_smo_nl_extended
{
    public class Startup
    {
        private static ILogger loggerModule;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
#if DEBUG
            SetDebugEnvironmentVariables();
#endif

            Configuration = configuration;
            loggerModule = logger;

            InitializeComponents();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);

            app.UseStaticFiles(); // For the wwwroot folder if you need it

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void InitializeComponents()
        {
            //Forward logger to components
            AwsSqsClient.SetLogger(loggerModule);
            AwsS3Client.SetLogger(loggerModule);
            SimulationProcessor.SetLogger(loggerModule);
            CompressionHelper.SetLogger(loggerModule);
            EssimManager.SetLogger(loggerModule);

            //Start Essim application
            EssimManager.StartEssim();
            //Listen to SQS queue and link simulation
            AwsSqsClient.ReadMessageFromSqs(SimulationProcessor.ProcessEsdlContent, SimulationRunFinished, ApplicationFinished);
        }

        private void SimulationRunFinished(object queueObject)
        {
            if (!(queueObject is QueueObject sqsObject)) return;

            //Compress result
            string outputDirectory = StorageHelper.GetPathToCsvStorage(sqsObject);
            if (Directory.Exists(outputDirectory) &&
                CompressionHelper.TryCompressFolder(outputDirectory, out string archivePath))
            {
                //Write output archive to S3
                string pathOnS3 = AwsHelper.GetStoragePathOnS3(archivePath);
                AwsS3Client.UploadFile(sqsObject.BucketName, pathOnS3, archivePath);

                //Clean files on disk
                StorageHelper.CleanUpFiles(sqsObject);

                //Update SQS object
                sqsObject.EssimResultLocation = pathOnS3;
            }

            //Notify SQS
            AwsSqsClient.WriteMessageToSqs(sqsObject);

            //Start listening to SQS queue again
            AwsSqsClient.ReadMessageFromSqs(SimulationProcessor.ProcessEsdlContent, SimulationRunFinished, ApplicationFinished);
        }

        private void ApplicationFinished()
        {
            loggerModule.LogInformation("Application finished. Terminating sub-modules");
            //Stop listening to SQS queue
            AwsSqsClient.Stop();
            //Stop any simulations
            SimulationProcessor.Stop();
            //Stop Essim application
            EssimManager.Stop();
            //Terminate application
            Environment.Exit(0);
        }

#if DEBUG
        private void SetDebugEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("HTTP_SERVER_SCHEME", "http");
            Environment.SetEnvironmentVariable("HTTP_SERVER_PORT", "8112");
            Environment.SetEnvironmentVariable("HTTP_SERVER_PATH", "essim");
            Environment.SetEnvironmentVariable("INFLUXDB_INTERNAL_URL", "http://influxdb-stripped:8086");
            Environment.SetEnvironmentVariable("INFLUXDB_EXTERNAL_URL", "http://influxdb-stripped:8086");
            

            Environment.SetEnvironmentVariable("AWS_ESSIM_QUEUE_URL", "https://sqs.eu-central-1.amazonaws.com/{AWS_ACCOUNT_NR}/gridmaster_essim_queue");
            Environment.SetEnvironmentVariable("AWS_ESSIM_EXPORT_QUEUE_URL", "https://sqs.eu-central-1.amazonaws.com/{AWS_ACCOUNT_NR}/gridmaster_essim_export_queue");
            Environment.SetEnvironmentVariable("AWS_ESSIM_QUEUE_TIMEOUT", "30");
            Environment.SetEnvironmentVariable("SIMULATION_START_DATE", "2018-12-31T23:00:00+0100");
            Environment.SetEnvironmentVariable("SIMULATION_END_DATE", "2019-12-31T22:00:00+0100");
        }
#endif
    }
}
