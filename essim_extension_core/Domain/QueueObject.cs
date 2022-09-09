using Newtonsoft.Json;

namespace essim_extension_core.Domain
{
    public class QueueObject
    {
        [JsonProperty(PropertyName = "scenarioId")]
        public int? ScenarioId { get; set; }
        
        [JsonProperty(PropertyName = "scenarioUuid")]
        public string ScenarioUuid { get; set; }
        
        [JsonProperty(PropertyName = "scenarioYear")]
        public int? ScenarioYear { get; set; }
        
        [JsonProperty(PropertyName = "calculationState")]
        public string CalculationState { get; set; }
        
        [JsonProperty(PropertyName = "scenarioMd5")]
        public string ScenarioMd5 { get; set; }
        
        [JsonProperty(PropertyName = "bucketName")]
        public string BucketName { get; set; }
        
        [JsonProperty(PropertyName = "bucketFolder")]
        public string BucketFolder { get; set; }
        
        [JsonProperty(PropertyName = "baseEsdlLocation")]
        public string BaseEsdlLocation { get; set; }
        
        [JsonProperty(PropertyName = "updatedEsdlLocation")]
        public string UpdatedEsdlLocation { get; set; }

        [JsonProperty(PropertyName = "scenarioJsonLocation")]
        public string ScenarioJsonLocation { get; set; }

        [JsonProperty(PropertyName = "contextScenarioLocation")]
        public string ContextScenarioLocation { get; set; }

        [JsonProperty(PropertyName = "essimExportGasunieLocation")]
        public string EssimExportGasunieLocation { get; set; }

        [JsonProperty(PropertyName = "essimExportTennetLocation")]
        public string EssimExportTennetLocation { get; set; }
        
        [JsonProperty(PropertyName = "essimSimulationId")]
        public string EssimSimulationId { get; set; }

        [JsonProperty(PropertyName = "essimResultLocation")]
        public string EssimResultLocation { get; set; }
        
        [JsonProperty(PropertyName = "etmResultLocation")]
        public string EtmResultLocation { get; set; }

        [JsonProperty(PropertyName = "etmScenarioId")]
        public int? EtmScenarioId { get; set; }

        [JsonProperty(PropertyName = "postProcessingGasunieLocation")]
        public string PostProcessingGasunieLocation { get; set; }

        [JsonProperty(PropertyName = "postProcessingGasunieAssignmentLocation")]
        public string PostProcessingGasunieAssignmentLocation { get; set; }

        [JsonProperty(PropertyName = "postProcessingTennetLocation")]
        public string PostProcessingTennetLocation { get; set; }
        
        [JsonProperty(PropertyName = "gasunieLoadFlowLocation")]
        public string GasunieLoadFlowLocation { get; set; }

        [JsonProperty(PropertyName = "tennetLoadFlowLocation")]
        public string TennetLoadFlowLocation { get; set; }

        public QueueObject() {}

        public QueueObject(QueueObject that)
        {
            this.ScenarioId = that.ScenarioId;
            this.ScenarioUuid = that.ScenarioUuid;
            this.ScenarioYear = that.ScenarioYear;
            this.CalculationState = that.CalculationState;
            this.ScenarioMd5 = that.ScenarioMd5;
            this.BucketName = that.BucketName;
            this.BucketFolder = that.BucketFolder;
            this.BaseEsdlLocation = that.BaseEsdlLocation;
            this.UpdatedEsdlLocation = that.UpdatedEsdlLocation;
            this.ScenarioJsonLocation = that.ScenarioJsonLocation;
            this.ContextScenarioLocation = that.ContextScenarioLocation;
            this.EssimExportGasunieLocation = that.EssimExportGasunieLocation;
            this.EssimExportTennetLocation = that.EssimExportTennetLocation;
            this.EssimSimulationId = that.EssimSimulationId;
            this.EssimResultLocation = that.EssimResultLocation;
            this.EtmResultLocation = that.EtmResultLocation;
            this.EtmScenarioId = that.EtmScenarioId;
            this.PostProcessingGasunieLocation = that.PostProcessingGasunieLocation;
            this.PostProcessingGasunieAssignmentLocation = that.PostProcessingGasunieAssignmentLocation;
            this.PostProcessingTennetLocation = that.PostProcessingTennetLocation;
            this.GasunieLoadFlowLocation = that.GasunieLoadFlowLocation;
            this.TennetLoadFlowLocation = that.TennetLoadFlowLocation;
        }
    }
}
