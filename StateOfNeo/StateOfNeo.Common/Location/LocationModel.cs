using Newtonsoft.Json;
using System.Collections.Generic;

namespace StateOfNeo.Common
{
    public class LocationModel
    {
        [JsonProperty(PropertyName = "country_flag")]
        public string Flag { get; set; }
        [JsonProperty(PropertyName = "capital")]
        public string Capital { get; set; }
        [JsonProperty(PropertyName = "languages")]
        public ICollection<LanguageModel> Languages { get; set; }
    }
}
