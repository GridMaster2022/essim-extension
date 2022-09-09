using Newtonsoft.Json;

namespace essim_extension_core.Domain
{
    public class TransportObject
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "networkHTMLDiag")]
        public string NetworkHtmlDiagnostics { get; set; }
    }
}
