using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common
{
    #region TruncateConverter
    ///<summary>funzione di conversione per troncare una stringa.</summary>
    ///<example>Esempio di utilizzo:
    ///     <TextBlock Text="{Binding Path=Description, Converter={StaticResource truncateConverter}, ConverterParameter=70}" />
    ///     in questo caso il textblock è associato alla proprietà description, troncata a 70 caratteri</example>
    ///<remarks>la conversione Truncate è una operazione applicabile in una sola direzione; si raccomanda che il binding abbia Mode=OneWay</remarks>
    [ValueConversion(typeof(string), typeof(string))]
    public class TruncateConverter : IValueConverter
    {
        #region .ctor
        ///<summary>inizializza una istanza dell'oggetto TruncateConverter.</summary>
        public TruncateConverter()
        {
            Suffix = "...";
        }
        #endregion
        #region IValueConverter Members
        #region Convert
        ///<summary>Esegue la conversione della stringa effettuando il troncamento.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) { return false; }
            if (value == null) throw new ArgumentException("value", "il parametro values ('{value}') non è un valido; TruncateConverter.Convert() richiede values contenga un array di oggetti.".Replace("{value}", Convert2.ToString(value)));

            int len = System.Convert.ToInt32(parameter);
            string sValue = Convert2.ToString(value);
            if (sValue != null && sValue.Length > len)
            {
                sValue = sValue.Substring(0, len) + Suffix;
            }
            return sValue;
        }
        #endregion
        #region ConvertBack
        ///<summary>ritorna la stessa stringa.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
        #endregion

        public string Suffix { get; set; }
    }
    #endregion
    #region JoinConverter
    [ValueConversion(typeof(object[]), typeof(string))]
    public sealed class JoinConverter : IValueConverter
    {
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentException("value", "il parametro values ('{value}') non è valido; JoinConverter.Convert() richiede values contenga un array di oggetti.".Replace("{value}", Convert2.ToString(value)));

            List<string> list = new List<string>();
            IEnumerable valueEnum = value as IEnumerable;
            if (valueEnum != null)
            {
                foreach (object v in valueEnum)
                {
                    list.Add(Convert2.ToString(v));
                }
            }
            else if (value != null)
            {
                list.Add(Convert2.ToString(value));
            }
            string[] values = list.ToArray();

            string separator = Convert2.ToString(parameter);
            string ret = "";

            if (values != null && values.Length > 0) ret = string.Join(separator, values);
            return ret;
        }
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;
            if (value != null && value != DependencyProperty.UnsetValue)
            {
                ret = !System.Convert.ToBoolean(value);
            }
            return ret;
        }
    }
    #endregion

    #region PadLeftConverter
    [ValueConversion(typeof(object), typeof(string))]
    public sealed class PadLeftConverter : IValueConverter
    {
        #region Format
        public int PadSize { get; set; }
        public char PadChar { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (PadChar == 0) return value;
            string ret = "";
            if (value == null) return "";
            if (value is string && string.IsNullOrEmpty((string)value)) return "";
            ret = PadChar != 0 ? value.ToString().PadLeft(PadSize, PadChar) : value.ToString();
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (PadChar == 0) return value;

            object objRet = value;
            if (value != null && value is string)
            {
                string str = (string)(value);
                if (str.Length > 1)
                {
                    // Se la stringa è composta da tutti i caratteri pad, lascio l'ultimo
                    string lastChar = str.Substring(str.Length - 1);
                    str = str.Substring(0, str.Length - 1).TrimStart(this.PadChar);
                    objRet = str + lastChar;
                }
            }
            return objRet;
        }
        #endregion
    }
    #endregion
    #region CutStringConverter
    /// <summary> taglia una stringa di un determinato numero di caratteri a destra o a sinistra
    /// esempio: </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public sealed class CutStringConverter : IValueConverter
    {
        #region Properties
        public int Length { get; set; }
        public string Start { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            string ret = value.ToString();
            if (Length > ret.Length) return ret;

            if (Start.ToLower() == "left")
            {
                ret = ret.Substring(Length, ret.Length - Length);
            }
            else if (Start.ToLower() == "right")
            {
                ret = ret.Substring(0, Length);
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion
    #region SubStringConverter
    [ValueConversion(typeof(object), typeof(string))]
    public sealed class SubStringConverter : IValueConverter
    {
        #region StartIndex
        public int StartIndex { get; set; }
        #endregion
        #region Lenght
        public int? Lenght { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = "";
            if (value == null) return ret;
            string szValue = value as String;
            if (szValue == null) return ret;
            if (StartIndex >= szValue.Length) return ret;

            int? length = Lenght;
            if (length != null && length.Value > szValue.Length - StartIndex) { length = null; }
            ret = length != null ? szValue.Substring(StartIndex, length.Value) : szValue.Substring(StartIndex);
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion

    #region TrimConverter
    [ValueConversion(typeof(string), typeof(string))]
    public class TrimConverter : TransformConverter<string>
    {
        public TrimConverter()
        {
            this.Trasformation = delegate (string s)
            {
                string ret = s != null ? s.Trim() : null;
                return !string.IsNullOrEmpty(ret) ? ret : null;
            };
        }
    }
    #endregion
    #region TrimEndConverter
    [ValueConversion(typeof(string), typeof(string))]
    public class TrimEndConverter : TransformConverter<string>
    {
        public TrimEndConverter()
        {
            this.Trasformation = delegate (string s)
            {
                string ret = s != null ? s.TrimEnd() : null;
                return !string.IsNullOrEmpty(ret) ? ret : null;
            };
        }
    }
    #endregion
    #region TrimStartConverter
    [ValueConversion(typeof(string), typeof(string))]
    public class TrimStartConverter : TransformConverter<string>
    {
        public TrimStartConverter()
        {
            this.Trasformation = delegate (string s)
            {
                string ret = s != null ? s.TrimStart() : null;
                return !string.IsNullOrEmpty(ret) ? ret : null;
            };
        }
    }
    #endregion
    #region IsNullOrEmptyConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public class IsNullOrEmptyConverter : TransformConverter<string, bool>
    {
        public IsNullOrEmptyConverter()
        {
            this.Trasformation = delegate (string s)
            {
                return string.IsNullOrEmpty(s);
            };
        }
    }
    #endregion
    #region IsNotNullOrEmptyConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public class IsNotNullOrEmptyConverter : TransformConverter<string, bool>
    {
        public IsNotNullOrEmptyConverter()
        {
            this.Trasformation = delegate (string s)
            {
                return !string.IsNullOrEmpty(s);
            };
        }
    }
    #endregion
    #region IsNullOrWhiteSpaceConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public class IsNullOrWhiteSpaceConverter : TransformConverter<string, bool>
    {
        public IsNullOrWhiteSpaceConverter()
        {
            this.Trasformation = delegate (string s)
            {
                return string.IsNullOrWhiteSpace(s);
            };
        }
    }
    #endregion
    #region IsNotNullOrWhiteSpaceConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public class IsNotNullOrWhiteSpaceConverter : TransformConverter<string, bool>
    {
        public IsNotNullOrWhiteSpaceConverter()
        {
            this.Trasformation = delegate (string s)
            {
                return !string.IsNullOrWhiteSpace(s);
            };
        }
    }
    #endregion
    #region WhiteSpaceToUnsetValueConverter
    [ValueConversion(typeof(string), typeof(object))]
    public class WhiteSpaceToUnsetValueConverter : TransformConverter<string, object>
    {
        public WhiteSpaceToUnsetValueConverter()
        {
            this.Trasformation = delegate (string s)
            {
                if (string.IsNullOrWhiteSpace(s)) { return DependencyProperty.UnsetValue; }
                return s;
            };
        }
    }
    #endregion

    #region NormalizeConverter
    [ValueConversion(typeof(string), typeof(string))]
    public class NormalizeConverter : TransformConverter<string>
    {
        public NormalizeConverter() { this.Trasformation = delegate (string s) { return s != null ? s.Normalize() : null; }; }
    }
    #endregion
    #region ToUpperConverter
    [ValueConversion(typeof(string), typeof(string))]
    public class ToUpperConverter : TransformConverter<string>
    {
        public ToUpperConverter() { this.Trasformation = delegate (string s) { return s != null ? s.ToUpper() : null; }; }
    }
    #endregion
    #region ToLowerConverter
    [ValueConversion(typeof(string), typeof(string))]
    public class ToLowerConverter : TransformConverter<string>
    {
        public ToLowerConverter() { this.Trasformation = delegate (string s) { return s != null ? s.ToLower() : null; }; }
    }
    #endregion    

    #region CheckStringValueConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(string), typeof(bool))]
    public class CheckStringValueConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return parameter == value; }
            return value.ToString().Equals(parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion
    #region CheckNotStringValueConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public class CheckNotStringValueConverter : CheckStringValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)base.Convert(value, targetType, parameter, culture));
        }
    }
    #endregion
    #region FlagToBoolValueConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public class FlagToBoolValueConverter : IValueConverter
    {
        public string Vero { get; set; }
        public string Falso { get; set; }

        public FlagToBoolValueConverter()
        {
            Vero = "True";
            Falso = "False";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.Equals(Falso))
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.Equals(false))
                return Falso;
            else
                return Vero;

        }
    }
    #endregion
    #region ConcatenateItemsConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class ConcatenateItemsConverter : IValueConverter
    {
        #region Separator
        public string Separator { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable groupItems = value as IEnumerable;
            if (groupItems == null) { return null; }
            string fieldNames = Convert2.ToString(parameter);
            if (StringHelper.IsEmpty(fieldNames)) { return DependencyProperty.UnsetValue; }
            string[] aFieldNames = fieldNames.Split('.'); // fieldNames può avere la seguente forma parentField.childField....

            PropertyInfo[] pInfo = new PropertyInfo[aFieldNames.Length]; // contiene i property info per il calcolo delle proprietà innestate
            string buffer = "";
            foreach (object item in groupItems)
            {
                try
                {
                    object itemValue = item; if (itemValue == null) { continue; } // salta i campi vuoti
                    for (int i = 0; i < aFieldNames.Length; i++)
                    { // calcola il valore dei campi innestati
                        string field = aFieldNames[i];
                        if (itemValue == null) { break; }
                        if (pInfo[i] == null) { pInfo[i] = itemValue.GetType().GetProperty(field); }
                        if (pInfo[i] != null) { itemValue = pInfo[i].GetValue(itemValue, null); }
                    }
                    if (itemValue != null)
                    {
                        if (buffer.Length > 0 && Separator != null) { buffer += Separator; }
                        buffer += Convert2.ToString(itemValue);
                    }
                }
                catch (Exception) { } // ignora le eccezioni nel loop di calcolo del raggruppamento
            }

            return buffer;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion

    #region EmptyStringToNullConvertBack
    [ValueConversion(typeof(string), typeof(string))]
    public class EmptyStringToNullConvertBack : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && value.ToString().Equals(string.Empty)) return null;
            return value;
        }
    }
    #endregion
    #region EmptyStringToNullConvert
    [ValueConversion(typeof(string), typeof(string))]
    public class EmptyStringToNullConvert : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && value.ToString().Equals(string.Empty)) return null;
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    #endregion
    #region StringToDateTimeConverter
    [ValueConversion(typeof(string), typeof(DateTime))]
    public sealed class StringToDateTimeConverter2 : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringFormat = parameter.ToString();
            string strValue = value as string;
            if (!string.IsNullOrWhiteSpace(strValue))
            {
                DateTime.TryParse(strValue, out var result);
                return result;
            }
            else
                return value;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringFormat = parameter.ToString();
            if (value is DateTime)
            {
                return ((DateTime)value).ToString(stringFormat);
            }
            else
                return null;
        }
        #endregion
    }
    #endregion
    #region StringToBooleanConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public sealed class StringToBooleanConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str)) return false;

            return System.Convert.ToBoolean(str);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            return System.Convert.ToString(b);
        }
        #endregion
    }
    #endregion
    #region StringArrayToStringConverter
    [ValueConversion(typeof(string[]), typeof(string))]
    public sealed class StringArrayToStringConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            string[] strArray = value as string[];
            if (strArray == null) return string.Empty;
            if (strArray.Length == 0) return string.Empty;
            string returnString = string.Empty;
            for (int i = 0; i < strArray.Length; i++)
            {
                returnString += ("\r\n" + strArray[i]);
            }

            return returnString.Substring(2);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Split(',');
        }
        #endregion
    }
    #endregion
    #region StringToDecimalConverter
    [ValueConversion(typeof(string), typeof(decimal))]
    public sealed class StringToDecimalConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0M;
            return System.Convert.ToDecimal(value);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;
            if (string.IsNullOrEmpty(s))
                return 0M;
            decimal ret;
            if (decimal.TryParse(s, out ret))
                return ret;
            return 0M;
        }
        #endregion
    }
    #endregion
    #region StringToIntegerConverter
    [ValueConversion(typeof(string), typeof(int))]
    public sealed class StringToIntegerConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0M;
            return System.Convert.ToInt32(value);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;
            if (string.IsNullOrEmpty(s))
                return 0;
            int ret;
            if (int.TryParse(s, out ret))
                return ret;
            return 0;
        }
        #endregion
    }
    #endregion

    #region StringToNullableDecimalConverter
    [ValueConversion(typeof(string), typeof(decimal?))]
    public sealed class StringToNullableDecimalConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == string.Empty)
                return null;
            return System.Convert.ToDecimal(value);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal? val = value as decimal?;
            if (val.HasValue)
                return val.Value.ToString();
            return null;
        }
        #endregion
    }
    #endregion
    #region StringToNullableIntegerConverter
    [ValueConversion(typeof(string), typeof(int?))]
    public sealed class StringToNullableIntegerConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == string.Empty)
                return null;
            return System.Convert.ToInt32(value);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? val = value as int?;
            if (val.HasValue)
                return val.Value.ToString();
            return null;
        }
        #endregion
    }
    #endregion

    #region BooleanToModifyConverter
    [ValueConversion(typeof(bool), typeof(string))]
    public sealed class BooleanToModifyConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if(parameter != null)
                {

                }

                if ((bool)value == true)
                    return "modify";
                else
                    return "";
            }
            return "";
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            switch (value.ToString().ToLower())
            {
                case "modify":
                    return true;
                case "no":
                    return false;
            }
            return false;
        }
        #endregion
    }
    #endregion

    #region ModifyToBooleanConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public sealed class ModifyToBooleanConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)value))
                return false;
            switch (value.ToString().ToLower())
            {
                case "modify":
                    return true;
                case "no":
                    return false;
            }
            return false;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return "modify";
                else
                    return "";
            }
            return "";


        }
        #endregion
    }
    #endregion

    //#region AccessTypeToVisibilityConverter
    //[ValueConversion(typeof(AccessType), typeof(Visibility))]
    //public sealed class AccessTypeToVisibilityConverter : IValueConverter
    //{
    //    #region Convert
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {

    //        AccessType strValue = (AccessType)value;
    //        string operation = parameter.ToString() ?? "base";
    //        switch (strValue)
    //        {
    //            case AccessType.ReadOnly:
    //                if (parameter.ToString().Equals("inverter"))
    //                    return Visibility.Collapsed;
    //                else
    //                    return Visibility.Visible;
    //            case AccessType.ReadWrite:
    //            case AccessType.WriteOnly:
    //                if (!parameter.ToString().Equals("inverter"))
    //                    return Visibility.Collapsed;
    //                else
    //                    return Visibility.Visible;
    //        }

    //        if (parameter.ToString().Equals("inverter"))
    //            return Visibility.Collapsed;
    //        else
    //            return Visibility.Visible;
    //    }
    //    #endregion
    //    #region ConvertBack
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //    #endregion
    //}
    //#endregion

    #region IntToBoolConverter
    [ValueConversion(typeof(int), typeof(bool))]
    public sealed class IntToBoolConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 0 ? false : true;
            }
            else
                return false;

        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is bool boolValue)
                return boolValue ? 1 : 0;
            else
                return 0;

        }
        #endregion
    }
    #endregion


}
