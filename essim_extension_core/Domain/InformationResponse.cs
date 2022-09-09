using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace essim_extension_core.Domain
{
    public class InformationResponse
    {
        [JsonProperty(PropertyName = "esdlContents")]
        public string EsdlContents { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "scenarioID")]
        public string ScenarioId { get; set; }

        [JsonProperty(PropertyName = "simulationDescription")]
        public string SimulationDescription { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty(PropertyName = "status")]
        public StateObject Status { get; set; }

        [JsonProperty(PropertyName = "influxURL")]
        public string InfluxUrl { get; set; }

        [JsonProperty(PropertyName = "simRunDate")]
        public DateTime? SimulationRunDate { get; set; }

        [JsonProperty(PropertyName = "transport")]
        public List<TransportObject> Transport { get; set; }

        [JsonProperty(PropertyName = "dashboardURL")]
        public string DashboardUrl { get; set; }
    }
}
