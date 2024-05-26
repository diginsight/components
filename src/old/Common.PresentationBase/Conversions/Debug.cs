using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace Common
{
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class LogValueConverter : IValueConverter, IMultiValueConverter
    {
        #region Level
        private ExceptionLevel _level = ExceptionLevel.Verbose;
        public ExceptionLevel Level { get { return _level; } set { _level = value; } }
        #endregion
        #region Format
        public string Format { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string format = this.Format;
            if (parameter is string && !string.IsNullOrEmpty(parameter as string)) { format = parameter as string; }

            switch (this.Level)
            {
                case ExceptionLevel.Critical:
                case ExceptionLevel.Error:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Error(string.Format(format, value));
                    }
                    else {
                        //Logger.Error(value);
                    }
                    break;
                case ExceptionLevel.Warning:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Warning(string.Format(format, value));
                    }
                    else {
                        //Logger.Warning(value);
                    }
                    break;
                case ExceptionLevel.Information:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Information(string.Format(format, value));
                    }
                    else {
                        //Logger.Information(value);
                    }
                    break;
                case ExceptionLevel.Verbose:
                case ExceptionLevel.Unknown:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Verbose(string.Format(format, value));
                    }
                    else {
                        //Logger.Verbose(value);
                    }
                    break;
                default:
                    break;
            }

            return null;
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
            object value = values != null && values.Length > i ? values[i] : null; i++;
            string format = values != null && values.Length > i ? values[i] as string : null; i++;
            object oLevel = values != null && values.Length > i ? values[i] : null; i++;

            if (string.IsNullOrEmpty(format) && parameter is string && !string.IsNullOrEmpty(parameter as string)) { format = parameter as string; }
            var level = oLevel is ExceptionLevel ? (ExceptionLevel)oLevel : _level;

            switch (level)
            {
                case ExceptionLevel.Critical:
                case ExceptionLevel.Error:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Error(string.Format(format, value));
                    }
                    else {
                        //Logger.Error(value);
                    }
                    break;
                case ExceptionLevel.Warning:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Warning(string.Format(format, value));
                    }
                    else {
                        //Logger.Warning(value);
                    }
                    break;
                case ExceptionLevel.Information:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Information(string.Format(format, value));
                    }
                    else {
                        //Logger.Information(value);
                    }
                    break;
                case ExceptionLevel.Verbose:
                case ExceptionLevel.Unknown:
                    if (!string.IsNullOrEmpty(format))
                    {
                        //Logger.Verbose(string.Format(format, value));
                    }
                    else {
                        //Logger.Verbose(value);
                    }
                    break;
                default:
                    break;
            }

            return null;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[3] { Binding.DoNothing, Binding.DoNothing, Binding.DoNothing };
        }
        #endregion
    }
}
