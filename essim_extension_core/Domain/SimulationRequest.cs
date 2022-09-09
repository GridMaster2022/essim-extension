using System;
using Newtonsoft.Json;

namespace essim_extension_core.Domain
{
    public class SimulationRequest
    {
        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "scenarioID")]
        public string ScenarioId { get; set; }

        [JsonProperty(PropertyName = "simulationDescription")]
        public string SimulationDescription { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public string EndDate { get; set; }

        [JsonProperty(PropertyName = "influxURL")]
        public string InfluxUrl { get; set; }

        [JsonProperty(PropertyName = "csvFilesLocation")]
        public string CsvFilesLocation { get; set; }

        [JsonProperty(PropertyName = "esdlContents")]
        public string EsdlContents { get; set; }

        public string GetOutputType()
        {
            if (!string.IsNullOrEmpty(InfluxUrl) && string.IsNullOrEmpty(CsvFilesLocation))
                return "Influx";
            if (string.IsNullOrEmpty(InfluxUrl) && !string.IsNullOrEmpty(CsvFilesLocation))
                return "Csv";

            return "Unknown";
        }
    }
}
