using System;
using essim_extension_core.Domain;
using Microsoft.Extensions.Logging;

namespace essim_extension_core.Helpers
{
    static class SimulationRequestHelper
    {
        public static SimulationRequest GetSimulationRequest(QueueObject simulationQueueObject, ILogger logger)
        {
            if (simulationQueueObject == null) return null;

            string esdlContent = AwsS3Client.ReadFile(simulationQueueObject.BucketName, simulationQueueObject.UpdatedEsdlLocation);
            if (string.IsNullOrEmpty(esdlContent)) return null;

            if (Environment.GetEnvironmentVariable("DEBUG")?.ToUpper() == "TRUE")
            {
                //Replace URL to Influx DB for local testing
                esdlContent = esdlContent.Replace("CLOUD_INFLUX_DB_IP", "http://influxdb-stripped");
            }

            string outputType = Environment.GetEnvironmentVariable("SIMULATION_OUTPUT_TYPE") ?? "INFLUX"; //Use "CSV" or "INFLUX"

            return new SimulationRequest
            {
                User = "AWS",
                ScenarioId = simulationQueueObject.ScenarioId.ToString(),
                SimulationDescription = $"{simulationQueueObject.ScenarioUuid}_{simulationQueueObject.ScenarioYear}",
                StartDate = Environment.GetEnvironmentVariable("SIMULATION_START_DATE"),
                EndDate = Environment.GetEnvironmentVariable("SIMULATION_END_DATE"),
                InfluxUrl = outputType.ToUpper() == "INFLUX" ? Environment.GetEnvironmentVariable("INFLUXDB_INTERNAL_URL") : null,
                CsvFilesLocation = outputType.ToUpper() == "CSV" ? StorageHelper.GetPathToCsvStorage(simulationQueueObject) : null,
                EsdlContents = esdlContent.ToBase64()
            };
        }
    }
}
