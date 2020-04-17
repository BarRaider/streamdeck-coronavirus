using BarRaider.Coronavirus.Wrappers;
using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.Coronavirus.Backend
{
    class CovidDataManager
    {

        #region Private Members

        private static CovidDataManager instance = null;
        private static readonly object objLock = new object();

        private const string COVID_API_SITE = "https://corona.lmao.ninja/v2/";
        private const string API_WORLDWIDE = "all";
        private const string API_COUNTRIES = "countries";
        private const int REFRESH_RATE_SECONDS = 600;


        private DateTime lastRefreshTime = DateTime.MinValue;
        private CovidWorldwideStats worldwideStats = null;
        private List<CovidCountryStats> countriesStats = null;

        #endregion

        #region Constructors

        public static CovidDataManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (objLock)
                {
                    if (instance == null)
                    {
                        instance = new CovidDataManager();
                    }
                    return instance;
                }
            }
        }

        private CovidDataManager()
        {
        }

        #endregion

        #region Public Methods

        public async Task<CovidWorldwideStats> GetWorldwideStats()
        {
            await LoadCovidData();
            return worldwideStats;
        }

        public async Task<List<CovidCountryStats>> GetCountriesStats()
        {
            await LoadCovidData();
            return countriesStats;
        }

        #endregion

        #region Private Methods

        private async Task LoadCovidData()
        {
            // Check if we should refresh the data
            if ((DateTime.Now - lastRefreshTime).TotalSeconds >= REFRESH_RATE_SECONDS)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Refreshing Covid data");
                lastRefreshTime = DateTime.Now;

                // Get worldwide data
                string url = $"{COVID_API_SITE}{API_WORLDWIDE}";
                string response = await QueryAPI(url);

                // Parse response
                if (!String.IsNullOrEmpty(response))
                {
                    worldwideStats = JsonConvert.DeserializeObject<CovidWorldwideStats>(response);
                }

                // Get country data
                url = $"{COVID_API_SITE}{API_COUNTRIES}";
                response = await QueryAPI(url);

                // Parse response
                if (!String.IsNullOrEmpty(response) && TryParse(response, out JArray jArr))
                {
                    countriesStats = jArr.ToObject<List<CovidCountryStats>>();
                }
            }          
        }

        private bool TryParse(string json, out JArray arr)
        {
            arr = null;
            try
            {
                arr = JArray.Parse(json);
                return true;
            }
            catch { }
            return false;
        }

        private async Task<string> QueryAPI(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 30);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string body = await response.Content.ReadAsStringAsync();
                        return body;
                    }
                    else
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, $"LoadWorldwideCovidStats failed! Response: {response.ReasonPhrase} Status Code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"LoadWorldwideCovidStats exception {ex}");
            }
            return null;
        }

        #endregion
    }

}
