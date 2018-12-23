using Newtonsoft.Json;

namespace StateOfNeo.Common.Http
{
    public class NeoNotificationVersionResponse
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
        [JsonProperty(PropertyName = "current_height")]
        public int Height { get; set; }
    }
}
