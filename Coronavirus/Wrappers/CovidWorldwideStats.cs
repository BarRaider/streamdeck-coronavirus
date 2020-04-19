using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.Coronavirus.Wrappers
{
    class CovidWorldwideStats
    {
        [JsonProperty(PropertyName = "cases")]
        public string AllCases { get; set; }

        [JsonProperty(PropertyName = "deaths")]
        public string Deaths { get; set; }

        [JsonProperty(PropertyName = "recovered")]
        public string Recovered { get; set; }
    }
}
