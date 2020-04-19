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
    // Subscriber: Flewp Gifted Sub
    // Subscriber: nubby_ninja x10 Gifted Sub
    // Subscriber: RecliningGamer
    //---------------------------------------------------
    [PluginActionId("com.barraider.coronavirus.worldwidestats")]
    public class CoronavirusWorldwideStatsAction : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    InputString = String.Empty
                };
                return instance;
            }

            [JsonProperty(PropertyName = "inputString")]
            public string InputString { get; set; }
        }

        #region Private Members
        
        private const string KEYPRESS_WEBSITE_URL = "https://www.worldometers.info/coronavirus/";

        private readonly PluginSettings settings;

        #endregion
        public CoronavirusWorldwideStatsAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
            System.Diagnostics.Process.Start(KEYPRESS_WEBSITE_URL);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override async void OnTick() 
        {
            var worldwide = await CovidDataManager.Instance.GetWorldwideStats();
            if (worldwide != null)
            {
                DrawKey(worldwide);
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private async void DrawKey(CovidWorldwideStats stats)
        {
            const int ICON_STARTING_X = 3;
            const int ICON_PADDING_Y = 3;
            const int TEXT_PADDING_Y = 3;
            const int TEXT_PADDING_X = 40;
            const int ICON_SIZE_PIXELS = 35;

            if (stats == null)
            {
                return;
            }

            if (!long.TryParse(stats.Deaths, out long deaths) || !long.TryParse(stats.AllCases, out long allCases))
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"CoronavirusWorldwideStatsAction > Could not convert deaths/all cases to integer Deaths: {stats.Deaths} All Case: {stats.AllCases}");
                return;
            }
            
            // Get the recovery rate as a percentage
            double recoveryRate = (1 - ((double)deaths / (double)allCases)) * 100;
            using (Bitmap img = Tools.GenerateGenericKeyImage(out Graphics graphics))
            {
                int height = img.Height;
                int width = img.Width;
                float heightPosition = 10;
                string text;

                var font = new Font("Verdana", 22, FontStyle.Bold, GraphicsUnit.Pixel);
                var fontRecoveryTitle = new Font("Verdana", 20, FontStyle.Bold, GraphicsUnit.Pixel);
                var fontRecovery = new Font("Verdana", 30, FontStyle.Bold, GraphicsUnit.Pixel);

                Bitmap icon;
                using (icon = IconChar.Ambulance.ToBitmap(ICON_SIZE_PIXELS, Color.Orange))
                {
                    graphics.DrawImage(icon, new Point(ICON_STARTING_X, (int)heightPosition));
                    heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, GraphicUtils.FormatNumber(allCases), font, Brushes.Orange, new PointF(TEXT_PADDING_X, heightPosition + +TEXT_PADDING_Y));
                }
                heightPosition += ICON_PADDING_Y;
                using (icon = IconChar.SkullCrossbones.ToBitmap(ICON_SIZE_PIXELS, Color.Red))
                {
                    graphics.DrawImage(icon, new Point(ICON_STARTING_X, (int)heightPosition));
                    heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, GraphicUtils.FormatNumber(deaths), font, Brushes.Red, new PointF(TEXT_PADDING_X, heightPosition + TEXT_PADDING_Y));
                }
                heightPosition += ICON_PADDING_Y;

                heightPosition = GraphicUtils.DrawStringOnGraphics(graphics, "Recovered:", fontRecoveryTitle, Brushes.Green, new PointF(ICON_STARTING_X, heightPosition));
                // Put percentage exactly in middle
                text = $"{(int)recoveryRate}%";
                float stringWidth = GraphicUtils.CenterText(text, width, fontRecovery, graphics, ICON_STARTING_X);
                GraphicUtils.DrawStringOnGraphics(graphics, text, fontRecovery, Brushes.Green, new PointF(stringWidth, heightPosition));

                await Connection.SetImageAsync(img);
                graphics.Dispose();
            }
        }

        #endregion
    }
}