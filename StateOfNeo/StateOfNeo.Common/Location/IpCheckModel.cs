using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Common
{
    public class IpCheckModel
    {
        [JsonProperty(PropertyName = "ip")]
        public string Ip { get; set; }
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
        [JsonProperty(PropertyName = "country_name")]
        public string CountryName { get; set; }
        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }
        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }
        [JsonProperty(PropertyName = "location")]
        public LocationModel Location { get; set; }
    }
}
