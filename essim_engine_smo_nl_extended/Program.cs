using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace essim_engine_smo_nl_extended
{
    public class Program
    {
        public static DateTime BuildDateTime;
        public static DateTime BootDateTime;
        
        public static void Main(string[] args)
        {
            BuildDateTime = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
            BootDateTime = DateTime.Now;
            
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
