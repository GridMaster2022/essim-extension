using Newtonsoft.Json;

namespace essim_extension_core.Domain
{
    public class StateObject
    {
        [JsonProperty(PropertyName = "State")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }
    }
}
