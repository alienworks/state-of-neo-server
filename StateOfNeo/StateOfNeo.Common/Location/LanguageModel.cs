using Newtonsoft.Json;

namespace StateOfNeo.Common
{
    public class LanguageModel
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "native")]
        public string Native { get; set; }
    }
}
