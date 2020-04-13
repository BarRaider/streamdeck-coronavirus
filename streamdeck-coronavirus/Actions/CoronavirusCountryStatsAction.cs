using BarRaider.Coronavirus.Backend;
using BarRaider.Coronavirus.Wrappers;
using BarRaider.SdTools;
using FontAwesome.Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.Coronavirus
{
    //---------------------------------------------------
    //          BarRaider's Hall Of Fame
    // 25 Bits: nubby_ninja
    // Ninja - Tip: 3.73
    // Subscriber: stupidN00b
    // 10 Bits: TyeMonkey
    // Subscriber: TyeMonkey
    //---------------------------------------------------
    [PluginActionId("com.barraider.coronavirus.countrystats")]
    public class CoronavirusCountryStatsAction : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Country = String.Empty,
                    Countries = null
                };
                return instance;
            }

            [JsonProperty(PropertyName = "country")]
            public string Country { get; set; }

            [JsonProperty(PropertyName = "countries")]
            public List<Country> Countries { get; set; }
        }

        #region Private Members

        private const int TOTAL_STAGES = 3;
        private const int STAGE_CHANGE_SECONDS = 5;
        private const string KEYPRESS_WEBSITE_URL = "https://www.worldometers.info/coronavirus/country/";

        private readonly PluginSettings settings;
        private int currentStage = 0;
        private DateTime lastStageChange = DateTime.MinValue;

        #endregion
        public CoronavirusCountryStatsAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            _ = InitializeSettings();
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
            var country = settings.Country.Replace("USA", "US");
            System.Diagnostics.Process.Start($"{KEYPRESS_WEBSITE_URL}{country}/");
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override async void OnTick()
        {
            if (String.IsNullOrEmpty(settings.Country))
            {
                return;
            }

            var countries = await CovidDataManager.Instance.GetCountriesStats();
            if (countries != null)
            {
                var country = countries.Where(c => c.Name == settings.Country).FirstOrDefault();
                DrawKey(country);
            }
        }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            await InitializeSettings();
            await SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private async Task InitializeSettings()
        {
            settings.Countries = new List<Country>();
            var countriesStats = await CovidDataManager.Instance.GetCountriesStats();
            if (countriesStats != null)
            {
                settings.Countries = countriesStats.OrderBy(c => c.Name).Select(c => new Country(c.Name)).ToList();
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Loaded ${settings.Countries.Count} countries");
            await SaveSettings();
        }

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private async void DrawKey(CovidCountryStats stats)
        {
            const int ICON_STARTING_X = 3;
            const int TEXT_PADDING_Y = 3;
            const int TEXT_PADDING_X = 40;
            const int ICON_SIZE_PIXELS = 35;
            const int COUNTRY_NAME_PADDING_Y = 10;

            if (stats == null)
            {
                return;
            }

            if ((DateTime.Now - lastStageChange).TotalSeconds >= STAGE_CHANGE_SECONDS)
            {
                lastStageChange = DateTime.Now;
                currentStage = (currentStage + 1) % TOTAL_STAGES;
            }


            if (!long.TryParse(stats.Recovered, out long recovered) || 
                !long.TryParse(stats.Deaths, out long deaths) ||
                !long.TryParse(stats.Cases, out long allCases))
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"CoronavirusCountryStatsAction > Could not convert deaths/all cases to integer Deaths: {stats.Deaths} Case: {stats.Cases}");
                return;
            }

            // Get the recovery rate as a percentage
            double recoveryRate = (double)recovered / (double)allCases * 100;
            using (Bitmap img = Tools.GenerateGenericKeyImage(out Graphics graphics))
            {
                graphics.PageUnit = GraphicsUnit.Pixel;
                int height = img.Height;
                int width = img.Width;
                float heightPosition = 10;
                string text;

                var font = new Font("Verdana", 23, FontStyle.Bold, GraphicsUnit.Pixel);
                var fontRecoveryTitle = new Font("Verdana", 20, FontStyle.Bold, GraphicsUnit.Pixel);
                var fontRecovery = new Font("Verdana", 30, FontStyle.Bold, GraphicsUnit.Pixel);

                Bitmap icon;
                float stringWidth = GraphicUtils.CenterText(stats.Name, width, font, graphics, 0);
                heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, stats.Name, font, Brushes.White, new PointF(stringWidth, heightPosition));
                heightPosition += COUNTRY_NAME_PADDING_Y;
                float widthPosition = 0;
                switch (currentStage)
                {
                    case 0: // All Cases
                        /*
                        using (icon = IconChar.Ambulance.ToBitmap(height, Color.Gray))
                        {  
                            text = $"{Utils.FormatNumber(allCases)}\n({stats.TodayCases})";
                            widthPosition = Utils.CenterText(text, width, font, graphics);
                            using (Image opacityIcon = Utils.SetImageOpacity(icon, 0.5f))
                            {
                                graphics.DrawImage(opacityIcon, new PointF(0, COUNTRY_NAME_PADDING_Y));
                            }
                            heightPosition = Utils.DrawStringOnGraphics(graphics, text, font, Brushes.Orange, new PointF(widthPosition, heightPosition + TEXT_PADDING_Y));
                        }
                        */
                         using (icon = IconChar.Ambulance.ToBitmap(ICON_SIZE_PIXELS, Color.Orange))
                        {  
                            text = $"{GraphicUtils.FormatNumber(allCases)}";
                            widthPosition = GraphicUtils.CenterText(text, width, font, graphics);
                            widthPosition -= ((ICON_SIZE_PIXELS + ICON_STARTING_X) / 2); // Add room for the icon
                            graphics.DrawImage(icon, new PointF(widthPosition, (int)heightPosition));
                            heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, text, font, Brushes.Orange, new PointF(widthPosition + ICON_SIZE_PIXELS + ICON_STARTING_X, heightPosition + TEXT_PADDING_Y));
                            text = $"({stats.TodayCases})";
                            widthPosition = GraphicUtils.CenterText(text, width, font, graphics);
                            heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, text, font, Brushes.Orange, new PointF(widthPosition, heightPosition));
                        }

                        break;
                    case 1: // Deaths
                        using (icon = IconChar.SkullCrossbones.ToBitmap(ICON_SIZE_PIXELS, Color.Red))
                        {
                            text = $"{GraphicUtils.FormatNumber(deaths)}";
                            widthPosition = GraphicUtils.CenterText(text, width, font, graphics);
                            widthPosition -= ((ICON_SIZE_PIXELS + ICON_STARTING_X) / 2); // Add room for the icon
                            graphics.DrawImage(icon, new PointF(widthPosition, (int)heightPosition));
                            heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, text, font, Brushes.Red, new PointF(widthPosition + ICON_SIZE_PIXELS + ICON_STARTING_X, heightPosition + TEXT_PADDING_Y));
                            text = $"({ stats.TodayDeaths})";
                            widthPosition = GraphicUtils.CenterText(text, width, font, graphics);
                            heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, text, font, Brushes.Red, new PointF(widthPosition, heightPosition));
                        }
                        break;
                    case 2: // Recovery
                        text = "Recovered:";
                        widthPosition = GraphicUtils.CenterText(text, width, font, graphics, 3);
                        heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, text, fontRecoveryTitle, Brushes.Green, new PointF(widthPosition, heightPosition));
                        // Put percentage exactly in middle
                        text = $"{(int)recoveryRate}%";
                        widthPosition = GraphicUtils.CenterText(text, width, fontRecovery, graphics, ICON_STARTING_X);
                        GraphicUtils.DrawStringOnGraphics(graphics, text, fontRecovery, Brushes.Green, new PointF(widthPosition, heightPosition));
                        break;
                }

                await Connection.SetImageAsync(img);
                graphics.Dispose();
            }
        }

        #endregion
    }
}
 