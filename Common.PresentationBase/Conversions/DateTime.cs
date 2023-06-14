using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common
{
    #region AddDaysConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class AddDaysConverter : IValueConverter, IMultiValueConverter
    {
        public double? Days { get; set; }
        public bool UseWorkingDays { get; set; }

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object date = value; if (date == null || date == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }
            object oDays = parameter ?? this.Days;
            if (oDays == null || oDays == DependencyProperty.UnsetValue) { oDays = 0; }

            float days = 0;
            if (oDays is float || oDays is float?)
            {
                days = (float)oDays;
            }
            else
            {
                float tryDays = float.NaN;
                bool res = float.TryParse(oDays.ToString(), out tryDays);
                if (res == true) { days = tryDays; }
            }

            DateTime result = DateTime.Today;
            if (date is DateTime || date is DateTime?)
            {
                result = (DateTime)date;
            }
            else
            {
                bool res = DateTime.TryParse(date.ToString(), out result);
                if (res == false) { return DependencyProperty.UnsetValue; }
            }

            //if (this.UseWorkingDays)
            //{
            //    short sdays = short.MinValue <= days && days <= short.MaxValue ? (short)days : (short)0;
            //    result = UtilityCalendario.TraslazioneGiorniLavorativi(result, sdays, 1);
            //}
            //else
            //{
                result = result.AddDays(days);
            //}
            return result;
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
            object date = values != null && values.Length > i ? values[i] : null; i++;
            if (date == null || date == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }
            object oDays = values != null && values.Length > i ? values[i] : null; i++;
            if (oDays == null || oDays == DependencyProperty.UnsetValue) { oDays = 0; }

            float days = 0;
            if (oDays is float || oDays is float?)
            {
                days = (float)oDays;
            }
            else
            {
                float tryDays = float.NaN;
                bool res = float.TryParse(oDays.ToString(), out tryDays);
                if (res == true) { days = tryDays; }
            }

            DateTime result = DateTime.Today;
            if (date is DateTime || date is DateTime?)
            {
                result = (DateTime)date;
            }
            else
            {
                bool res = DateTime.TryParse(date.ToString(), out result);
                if (res == false) { return DependencyProperty.UnsetValue; }
            }

            //if (this.UseWorkingDays)
            //{
                //short sdays = short.MinValue <= days && days <= short.MaxValue ? (short)days : (short)0;
                //result = UtilityCalendario.TraslazioneGiorniLavorativi(result, sdays, 1);
            //}
            //else
            //{
                result = result.AddDays(days);
            //}
            return result;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[3] { Binding.DoNothing, Binding.DoNothing, Binding.DoNothing };
        }
        #endregion

    }
    #endregion

    #region LastTickConverter
    [ValueConversion(typeof(DateTime?), typeof(DateTime?))]
    public class LastTickConverter : TransformConverter<DateTime?>
    {
        public LastTickConverter()
        {
            //this.Trasformation = delegate(DateTime? d) { return d != null ? (DateTime?)d.Value.Date.AddDays(1).AddMilliseconds(-2) : null; };
            this.Trasformation = delegate (DateTime? d) { return d != null ? (DateTime?)d.Value.Date.AddMilliseconds(-2).AddDays(1) : null; };
            this.BackTrasformation = delegate (DateTime? d) { return d != null ? (DateTime?)d.Value.Date.AddMilliseconds(-2).AddDays(1) : null; };
        }
    }
    #endregion

    #region DateConverter
    [ValueConversion(typeof(DateTime?), typeof(DateTime?))]
    public class DateConverter : TransformConverter<DateTime?>
    {
        public DateConverter()
        {
            this.Trasformation = delegate (DateTime? d) { return d != null ? (DateTime?)d.Value.Date : null; };
            this.BackTrasformation = delegate (DateTime? d) { return d != null ? (DateTime?)d.Value.Date : null; };
        }
    }
    #endregion

    #region SecondsToDelayConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(int), typeof(string))]
    public sealed class SecondsToDelayConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            long secondi = 0;
            if (value != null && value != DependencyProperty.UnsetValue && Int64.TryParse(value.ToString(), out secondi))
            {
                TimeSpan ts = new TimeSpan(secondi * TimeSpan.TicksPerSecond);

                if (ts.TotalMinutes < 1)
                {
                    result = string.Format("{0} secondi", ts.Seconds);
                }
                else if (ts.TotalHours < 1)
                {
                    result = string.Format("{0} min {1} sec", ts.Minutes, ts.Seconds);
                }
                else if (ts.TotalDays < 1)
                {
                    result = string.Format("{0}h {1}m {2}s", ts.Hours, ts.Minutes, ts.Seconds);
                }
                else
                {
                    result = string.Format("{0}g {1}h {2}m {3}s", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
                }
            }
            return result;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
    #endregion

    #region TimeSpanPartConverter
    [ValueConversion(typeof(string), typeof(TimeSpan))]
    public sealed class TimeSpanPartConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new TimeSpan();
            string val = value as string;
            string format = parameter as string;
            if (format == null)
                return TimeSpan.Parse(val);
            return TimeSpan.ParseExact(val, format, null);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            TimeSpan val = (TimeSpan)value;
            string format = parameter as string;
            if (format == null)
                return val.ToString();
            return val.ToString(format, null);
        }
        #endregion
    }
    #endregion

    #region DatesAreEqualsConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(DateTime), typeof(bool))]
    public sealed class DatesAreEqualsConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value != null && value != DependencyProperty.UnsetValue && parameter != null && parameter != DependencyProperty.UnsetValue)
            {
                DateTime dateStart = (DateTime)value;
                DateTime dateCompare = (DateTime)parameter;

                if (dateStart.ToShortDateString().Equals(dateCompare.ToShortDateString()))
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;

            }
            return result;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
    #endregion
}
