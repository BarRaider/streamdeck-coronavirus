using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.Coronavirus.Backend
{
    internal static class GraphicUtils
    {
        internal static string FormatNumber(long num)
        {
            if (num >= 100000000)
            {
                return (num / 1000000D).ToString("0.#M");
            }
            if (num >= 1000000)
            {
                return (num / 1000000D).ToString("0.##M");
            }
            if (num >= 100000)
            {
                return (num / 1000D).ToString("0.#k");
            }
            if (num >= 10000)
            {
                return (num / 1000D).ToString("0.##k");
            }

            return num.ToString("#,0");
        }

        internal static float CenterText(string text, int width, Font font, Graphics graphics, int minIndentation = 0)
        {
            SizeF stringSize = graphics.MeasureString(text, font);
            float stringWidth = minIndentation;
            if (stringSize.Width < width)
            {
                stringWidth = Math.Abs((width - stringSize.Width)) / 2;
            }
            return stringWidth;
        }

        internal static float DrawStringOnGraphics(Graphics graphics, string text, Font font, Brush brush, PointF position)
        {
            SizeF stringSize = graphics.MeasureString(text, font);
            graphics.DrawString(text, font, brush, position);

            return position.Y + stringSize.Height;
        }

        /// <param name="image">image to set opacity on</param>  
        /// <param name="opacity">percentage of opacity</param>  
        /// <returns></returns>  
        internal static Image SetImageOpacity(Image image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity  
                    matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"SetImageOpacity exception {ex}");
                return null;
            }
        }
    }
}
