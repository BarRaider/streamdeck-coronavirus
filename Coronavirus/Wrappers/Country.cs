using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.Coronavirus.Wrappers
{
    class Country
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        public Country(string name)
        {
            Name = name;
        }
    }
}
