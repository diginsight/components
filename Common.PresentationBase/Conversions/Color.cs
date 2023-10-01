#region using
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Binding = System.Windows.Data.Binding;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;
#endregion

namespace Common
{
    [ValueConversion(typeof(object), typeof(float))]
    public class Color2BrushConverter : TransformConverter<object, object>
    {
        public Color2BrushConverter()
        {
            Trasformation = delegate (object c)
            {
                if (c is Color) { return new SolidColorBrush((Color)c); }
                return DependencyProperty.UnsetValue;
            };
            Trasformation2 = delegate (object[] oa)
            {
                object c = oa.FirstOrDefault();
                if (c is Color) { return new SolidColorBrush((Color)c); }
                return DependencyProperty.UnsetValue;
            };
        }
    }

    #region BrightnessConverter
    [ValueConversion(typeof(Color), typeof(Color))]
    public sealed class BrightnessConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object val = value != null ? (Color)value : DependencyProperty.UnsetValue;
            float par = parameter != null ? (float)value : 0;

            if (val is Color col)
            {
                byte hue = (byte)(System.Drawing.Color.FromArgb(col.R, col.G, col.B).GetHue() * 255);
                byte saturation = (byte)(System.Drawing.Color.FromArgb(col.R, col.G, col.B).GetSaturation() * 255);
                byte brightness = (byte)(System.Drawing.Color.FromArgb(col.R, col.G, col.B).GetBrightness() * 255);
                brightness = (byte)(brightness * par);
                return HSBtoColor(hue, saturation, brightness);
            }

            return val;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            Color val = Collections.TryGetItem<Color>(values, i++);
            double par = Collections.TryGetItem<double>(values, i++);

            //if (val is Color)
            //{
            Color col = val;
            HsbColor hsbColor = RgbToHsb(col);
            hsbColor.B *= par;
            Color rgbColorOut = HsbToRgb(hsbColor);
            return rgbColorOut;
            //System.Drawing.Color drawColor = System.Drawing.Color.FromArgb(col.R, col.G, col.B);
            //var hue = drawColor.GetHue(); 
            //var saturation = drawColor.GetSaturation();
            //var luminosity = drawColor.GetBrightness();
            //luminosity = luminosity * (float)par;

            //var c = new System.Drawing.Color(){ }
            //System.Drawing.Color.
            ////return HSBtoColor(hue, saturation, luminosity);
            //return FromAhsv(255, hue, saturation, luminosity);

            //}
            return val;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[2] { Binding.DoNothing, Binding.DoNothing };
        }
        #endregion

        #region RGB-HSB Conversion
        public struct HsbColor
        {
            public double A;
            public double H;
            public double S;
            public double B;
        }

        /// <summary>
        /// Converts an RGB color to an HSB color.
        /// </summary>
        /// <param name="rgbColor">The RGB color to convert.</param>
        /// <returns>The HSB color equivalent of the RGBA color passed in.</returns>
        /// <remarks>Source: http://msdn.microsoft.com/en-us/library/ms771620.aspx</remarks>
        public static HsbColor RgbToHsb(Color rgbColor)
        {
            /* Hue values range between 0 and 360. All 
             * other values range between 0 and 1. */

            // Create HSB color object
            HsbColor hsbColor = new HsbColor();

            // Get RGB color component values
            int r = rgbColor.R;
            int g = rgbColor.G;
            int b = rgbColor.B;
            int a = rgbColor.A;

            // Get min, max, and delta values
            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);
            double delta = max - min;

            /* Black (max = 0) is a special case. We 
             * simply set HSB values to zero and exit. */

            // Black: Set HSB and return
            if (max == 0.0)
            {
                hsbColor.H = 0.0;
                hsbColor.S = 0.0;
                hsbColor.B = 0.0;
                hsbColor.A = a;
                return hsbColor;
            }

            /* Now we process the normal case. */

            // Set HSB Alpha value
            double alpha = a;
            hsbColor.A = alpha / 255;

            // Set HSB Hue value
            if (r == max) hsbColor.H = (g - b) / delta;
            else if (g == max) hsbColor.H = 2 + (b - r) / delta;
            else if (b == max) hsbColor.H = 4 + (r - g) / delta;
            hsbColor.H *= 60;
            if (hsbColor.H < 0.0) hsbColor.H += 360;

            // Set other HSB values
            hsbColor.S = delta / max;
            hsbColor.B = max / 255;

            // Set return value
            return hsbColor;
        }

        /// <summary>
        /// Converts an HSB color to an RGB color.
        /// </summary>
        /// <param name="hsbColor">The HSB color to convert.</param>
        /// <returns>The RGB color equivalent of the HSB color passed in.</returns>
        /// Source: http://msdn.microsoft.com/en-us/library/ms771620.aspx
        public static Color HsbToRgb(HsbColor hsbColor)
        {
            // Initialize
            Color rgbColor = new Color();

            /* Gray (zero saturation) is a special case.We simply
             * set RGB values to HSB Brightness value and exit. */

            // Gray: Set RGB and return
            if (hsbColor.S == 0.0)
            {
                rgbColor.A = (byte)(hsbColor.A * 255);
                rgbColor.R = (byte)(hsbColor.B * 255);
                rgbColor.G = (byte)(hsbColor.B * 255);
                rgbColor.B = (byte)(hsbColor.B * 255);
                return rgbColor;
            }

            /* Now we process the normal case. */

            double h = (hsbColor.H == 360) ? 0 : hsbColor.H / 60;
            int i = (int)(Math.Truncate(h));
            double f = h - i;

            double p = hsbColor.B * (1.0 - hsbColor.S);
            double q = hsbColor.B * (1.0 - (hsbColor.S * f));
            double t = hsbColor.B * (1.0 - (hsbColor.S * (1.0 - f)));

            double r, g, b;
            switch (i)
            {
                case 0:
                    r = hsbColor.B;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = hsbColor.B;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = hsbColor.B;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = hsbColor.B;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = hsbColor.B;
                    break;

                default:
                    r = hsbColor.B;
                    g = p;
                    b = q;
                    break;
            }

            // Set WPF Color object
            rgbColor.A = (byte)(hsbColor.A * 255);
            rgbColor.R = (byte)(r * 255);
            rgbColor.G = (byte)(g * 255);
            rgbColor.B = (byte)(b * 255);

            // Set return value
            return rgbColor;
        }

        #endregion

        #region helpers
        public static Color HSBtoColor(float hue, float saturation, float brightness)
        {
            int r = 0, g = 0, b = 0;
            if (saturation == 0)
            {
                r = g = b = (int)(brightness * 255.0f + 0.5f);
            }
            else
            {
                float h = (hue - (float)Math.Floor(hue)) * 6.0f;
                float f = h - (float)Math.Floor(h);
                float p = brightness * (1.0f - saturation);
                float q = brightness * (1.0f - saturation * f);
                float t = brightness * (1.0f - (saturation * (1.0f - f)));
                switch ((int)h)
                {
                    case 0:
                        r = (int)(brightness * 255.0f + 0.5f);
                        g = (int)(t * 255.0f + 0.5f);
                        b = (int)(p * 255.0f + 0.5f);
                        break;
                    case 1:
                        r = (int)(q * 255.0f + 0.5f);
                        g = (int)(brightness * 255.0f + 0.5f);
                        b = (int)(p * 255.0f + 0.5f);
                        break;
                    case 2:
                        r = (int)(p * 255.0f + 0.5f);
                        g = (int)(brightness * 255.0f + 0.5f);
                        b = (int)(t * 255.0f + 0.5f);
                        break;
                    case 3:
                        r = (int)(p * 255.0f + 0.5f);
                        g = (int)(q * 255.0f + 0.5f);
                        b = (int)(brightness * 255.0f + 0.5f);
                        break;
                    case 4:
                        r = (int)(t * 255.0f + 0.5f);
                        g = (int)(p * 255.0f + 0.5f);
                        b = (int)(brightness * 255.0f + 0.5f);
                        break;
                    case 5:
                        r = (int)(brightness * 255.0f + 0.5f);
                        g = (int)(p * 255.0f + 0.5f);
                        b = (int)(q * 255.0f + 0.5f);
                        break;
                }
            }
            return Color.FromArgb(System.Convert.ToByte(255), System.Convert.ToByte(r), System.Convert.ToByte(g), System.Convert.ToByte(b));
        }
        public static Color FromAhsv(byte alpha, float hue, float saturation, float value)
        {
            if (hue < 0f || hue > 360f) { throw new ArgumentOutOfRangeException(nameof(hue), hue, "Hue must be in the range [0,360]"); }
            if (saturation < 0f || saturation > 1f) { throw new ArgumentOutOfRangeException(nameof(saturation), saturation, "Saturation must be in the range [0,1]"); }
            if (value < 0f || value > 1f) { throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be in the range [0,1]"); }

            Func<int, byte> component = (int n) =>
            {
                float k = (n + hue / 60f) % 6;
                float c = value - value * saturation * Math.Max(Math.Min(Math.Min(k, 4 - k), 1), 0);
                int b = (int)Math.Round(c * 255);
                return (byte)(b < 0 ? 0 : b > 255 ? 255 : b);
            };

            return Color.FromArgb(alpha, component(5), component(3), component(1));
        }
        #endregion
    }
    #endregion

    //rndColorConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class RndColorConverter : IValueConverter
    {
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Random rnd = new Random();
            Rectangle grid = value as Rectangle;
            if (grid != null)
            {
                SolidColorBrush[] colorBrushes = parameter as SolidColorBrush[];

                return colorBrushes[rnd.Next(0, colorBrushes.Length - 1)];
            }
            return null;
        }
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
