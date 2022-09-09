using Newtonsoft.Json;

namespace essim_extension_core.Domain
{
    public class StateResponse
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "State")] //Account for inconsistent naming
        public string State
        {
            get => Status;
            set => Status = value;
        }
        
        [JsonProperty(PropertyName = "Description")] //Account for inconsistent naming
        public string Description2
        {
            get => Description;
            set => Description = value;
        }
    }
}
