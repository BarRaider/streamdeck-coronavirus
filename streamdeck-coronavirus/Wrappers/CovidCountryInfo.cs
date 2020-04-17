using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.Coronavirus.Wrappers
{
    class CovidCountryInfo
    {
        [JsonProperty(PropertyName = "iso2")]
        public string Iso2 { get; set; }

        [JsonProperty(PropertyName = "iso3")]
        public string Iso3 { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public string Latitude { get; set; }

        [JsonProperty(PropertyName = "long")]
        public string Longitude { get; set; }

        [JsonProperty(PropertyName = "flag")]
        public string FlagURL { get; set; }
    }
}
