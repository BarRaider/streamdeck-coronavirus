using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.Coronavirus.Wrappers
{
    class CovidCountryStats
    {
        [JsonProperty(PropertyName = "country")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "cases")]
        public string Cases { get; set; }

        [JsonProperty(PropertyName = "todayCases")]
        public string TodayCases { get; set; }

        [JsonProperty(PropertyName = "deaths")]
        public string Deaths { get; set; }

        [JsonProperty(PropertyName = "todayDeaths")]
        public string TodayDeaths { get; set; }

        [JsonProperty(PropertyName = "recovered")]
        public string Recovered { get; set; }

        [JsonProperty(PropertyName = "critical")]
        public string Critical { get; set; }

        [JsonProperty(PropertyName = "countryInfo")]
        public CovidCountryInfo Info { get; set; }
    }
}
