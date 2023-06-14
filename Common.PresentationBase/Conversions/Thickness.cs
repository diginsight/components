using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common
{
    [ValueConversion(typeof(double), typeof(Thickness))]
    public class ThicknessConverter : TransformConverter<double, Thickness>
    {
        public ThicknessConverter()
        {
            this.Trasformation = delegate (double length)
            {
                return new Thickness(length);
            };
            this.Trasformation2 = delegate (double[] oa)
            {
                if (oa == null) { return default(Thickness); }

                var length = oa.Length;
                if (length == 1) { return new Thickness(oa[0]); }
                if (length == 2) { return new Thickness(oa[0], oa[1], oa[0], oa[1]); }
                if (length == 4) { return new Thickness(oa[0], oa[1], oa[2], oa[3]); }
                return default(Thickness);
            };
        }
    }


    public class Double2GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new GridLength((double)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            GridLength gridLength = (GridLength)value;
            return gridLength.Value;
        }
    }

}
