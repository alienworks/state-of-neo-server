using Newtonsoft.Json;

namespace StateOfNeo.Common.Http
{
    public class HeightResponseObject
    {
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }
    }
}
