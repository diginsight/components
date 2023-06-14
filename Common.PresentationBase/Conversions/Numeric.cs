using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Common
{
    #region MathOperatorConverter
    public class MathOperatorConverter<T> : IMultiValueConverter, IValueConverter where T : IComparable
    {
        public char Operation { get; set; }
        public T DefaultParameter { get; set; }

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IMath<T> math = Math<T>.Default; T result = default(T);
            if (values == null || values == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;

            bool firstIteration = true;
            foreach (object value in values)
            {
                object item = value;
                if (item == null || item == DependencyProperty.UnsetValue)
                {
                    return DependencyProperty.UnsetValue;
                }
                else
                {
                    if (!(item is T))
                    {
                        T res = default(T); bool success = math.TryParse(Convert2.ToString(item), NumberStyles.Any, culture, out res);
                        if (!success) { return DependencyProperty.UnsetValue; }
                        item = res;
                    }

                    T valore = (T)item;
                    if (firstIteration) { firstIteration = false; result = valore; continue; }
                    switch (Operation)
                    {
                        case '+':
                            //math.MaxValue - valore < result
                            //math.MinValue - valore < result
                            if (valore.CompareTo(default(T)) > 0 && math.Subtract(math.MaxValue, valore).CompareTo(result) < 0) { return DependencyProperty.UnsetValue; }
                            if (valore.CompareTo(default(T)) < 0 && math.Subtract(math.MinValue, valore).CompareTo(result) < 0) { return DependencyProperty.UnsetValue; }
                            result = math.Add(result, valore);
                            break;
                        case '-':
                            //math.MinValue + valore > result
                            //math.MaxValue + valore > result
                            if (valore.CompareTo(default(T)) > 0 && math.Add(math.MinValue, valore).CompareTo(result) > 0) { return DependencyProperty.UnsetValue; }
                            if (valore.CompareTo(default(T)) < 0 && math.Add(math.MaxValue, valore).CompareTo(result) > 0) { return DependencyProperty.UnsetValue; }
                            result = math.Subtract(result, valore);
                            break;
                        case '*':
                            if (math.Abs(valore).CompareTo(default(T)) > 1 && math.Divide(math.MaxValue, math.Abs(valore)).CompareTo(math.Abs(result)) < 0) { return DependencyProperty.UnsetValue; }
                            result = math.Multiply(result, valore);
                            break;
                        case ':':
                            if (math.Abs(valore).CompareTo(default(T)) < 1 && math.Multiply(math.MaxValue, math.Abs(valore)).CompareTo(math.Abs(result)) < 0) { return DependencyProperty.UnsetValue; }
                            result = math.Divide(result, valore);
                            break;
                    }
                }
            }

            return result;
        }
        #endregion
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IMath<T> math = Math<T>.Default; object left = value;
            if (left == null || left == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }
            if (!(left is T))
            {
                T res = default(T); bool success = math.TryParse(Convert2.ToString(left), NumberStyles.Any, culture, out res);
                if (!success) { return DependencyProperty.UnsetValue; }
                left = res;
            }
            T leftDecimal = (T)left;
            object right = parameter ?? this.DefaultParameter;
            if (right == null || right == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }
            if (!(right is T))
            {
                T res = default(T); bool success = math.TryParse(Convert2.ToString(right), NumberStyles.Any, culture, out res);
                if (!success) { return DependencyProperty.UnsetValue; }
                right = res;
            }
            T rightDecimal = (T)right;

            T result = default(T);
            switch (Operation)
            {
                case '+':
                    if (math.Subtract(math.MaxValue, rightDecimal).CompareTo(leftDecimal) < 0) { return DependencyProperty.UnsetValue; }
                    result = math.Add(leftDecimal, rightDecimal);
                    break;
                case '-':
                    if (math.Add(math.MinValue, rightDecimal).CompareTo(result) > 0) { return DependencyProperty.UnsetValue; }
                    result = math.Subtract(leftDecimal, rightDecimal);
                    break;
                case '*':
                    //if (decimal.MaxValue / rightDecimal < result) { return DependencyProperty.UnsetValue; }
                    result = math.Multiply(leftDecimal, rightDecimal);
                    break;
                case ':':
                    //if (decimal.MinValue * rightDecimal > result) { return DependencyProperty.UnsetValue; }
                    result = math.Divide(leftDecimal, rightDecimal);
                    break;
            }
            return result;
        }
        #endregion

        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IMath<T> math = Math<T>.Default; object left = value;
            if (left == null || left == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }
            if (!(left is T))
            {
                T res = default(T); bool success = math.TryParse(Convert2.ToString(left), NumberStyles.Any, culture, out res);
                if (!success) { return DependencyProperty.UnsetValue; }
                left = res;
            }
            T leftDecimal = (T)left;
            object right = parameter ?? this.DefaultParameter;
            if (right == null || right == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }
            if (!(right is T))
            {
                T res = default(T); bool success = math.TryParse(Convert2.ToString(right), NumberStyles.Any, culture, out res);
                if (!success) { return DependencyProperty.UnsetValue; }
                right = res;
            }
            T rightDecimal = (T)right;

            T result = default(T);
            switch (Operation)
            {
                case '+':
                    //if (math.Subtract(math.MaxValue, rightDecimal).CompareTo(leftDecimal) < 0) { return DependencyProperty.UnsetValue; }
                    //result = math.Add(leftDecimal, rightDecimal);
                    break;
                case '-':
                    //if (math.Add(math.MinValue, rightDecimal).CompareTo(result) > 0) { return DependencyProperty.UnsetValue; }
                    //result = math.Subtract(leftDecimal, rightDecimal);
                    break;
                case '*':
                    //if (decimal.MaxValue / rightDecimal < result) { return DependencyProperty.UnsetValue; }
                    result = math.Divide(leftDecimal, rightDecimal);
                    break;
                case ':':
                    //if (decimal.MinValue * rightDecimal > result) { return DependencyProperty.UnsetValue; }
                    result = math.Multiply(leftDecimal, rightDecimal);
                    break;
            }
            return result;
        }
        #endregion
    }
    #endregion
    #region MathOperatorConverters
    public sealed class MathOperatorConverterInt32 : MathOperatorConverter<int> { }
    public sealed class MathOperatorConverterInt64 : MathOperatorConverter<long> { }
    public sealed class MathOperatorConverterDecimal : MathOperatorConverter<decimal> { }
    public sealed class MathOperatorConverterSingle : MathOperatorConverter<Single> { }
    public sealed class MathOperatorConverterDouble : MathOperatorConverter<Double> { }
    #endregion

    #region RndConverter
    [ValueConversion(typeof(object), typeof(int))]
    public class RndConverter : TransformConverter<object, object>
    {
        static Random _rnd = new Random();
        private int _max = 100;
        private int _min = 1;

        public int Min { get => _min; set => _min = value; }
        public int Max { get => _max; set => _max = value; }

        public RndConverter()
        {
            this.Trasformation = delegate (object o)
            {
                return _rnd.Next(this.Min, this.Max);
            };
            this.Trasformation2 = delegate (object[] oa)
            {
                return _rnd.Next(this.Min, this.Max);
            };
        }
    }
    #endregion
    #region RndDoubleConverter
    [ValueConversion(typeof(object), typeof(float))]
    public class RndDoubleConverter : TransformConverter<object, object>
    {
        static Random _rnd = new Random();
        private double _min = 0;
        private double _max = 1;

        public double Min { get => _min; set => _min = value; }
        public double Max { get => _max; set => _max = value; }

        public RndDoubleConverter()
        {
            this.Trasformation = delegate (object o)
            {
                return _rnd.NextDouble() * (this.Max- this.Min) + this.Min;
            };
            this.Trasformation2 = delegate (object[] oa)
            {
                return _rnd.NextDouble() * (this.Max - this.Min) + this.Min;
            };
        }
    }
    #endregion

    #region NumberCompareConverter
    [ValueConversion(typeof(decimal), typeof(bool))]
    public sealed class NumberCompareConverter : IValueConverter
    {
        public decimal Limit { get; set; }
        public string Op { get; set; }

        ///<summary>Esegue il confronto di un numerico con un parametro dato, ritorna un booleano.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = true;
            decimal number = System.Convert.ToDecimal(value);

            decimal limit = this.Limit;
            if (parameter != null) { limit = System.Convert.ToDecimal(parameter, culture); }

            if (Op == "==" && number != limit) { ret = false; }
            if (Op == "!=" && number == limit) { ret = false; }
            if (Op == "<" && number >= limit) { ret = false; }
            if (Op == "<=" && number > limit) { ret = false; }
            if (Op == ">=" && number < limit) { ret = false; }
            if (Op == ">" && number <= limit) { ret = false; }
            return ret;
        }
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Limit;
        }
    }
    #endregion

    #region ChooseStringConverter
    [ValueConversion(typeof(object), typeof(string))]
    public sealed class ChooseStringConverter : IValueConverter
    {
        public string[] Strings { get; set; }

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = 0;
            if (value != DependencyProperty.UnsetValue)
            {
                index = System.Convert.ToInt32(value);
            }

            string ret = "";
            if (Strings != null && index < Strings.Length) { ret = Strings[index]; }
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

    #region NullableZeroStringValueConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [Obsolete] // usare replaceobject converter
    [ValueConversion(typeof(decimal), typeof(string))]
    public class NullableZeroStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            string stringa = value.ToString();
            return (stringa == "0") ? string.Empty : stringa;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringa = value as string;
            if (StringHelper.IsEmpty(stringa))
            {
                return 0;
            }
            else
            {
                return System.Convert.ChangeType(stringa, targetType, null);
            }
        }
    }
    #endregion

    #region ChooseObjectConverter
    [ValueConversion(typeof(object[]), typeof(object))]
    public sealed class ChooseObjectConverter : IMultiValueConverter
    {
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values == DependencyProperty.UnsetValue || values.Length <= 1) { return DependencyProperty.UnsetValue; }
            if (values[0] == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }

            int index = System.Convert.ToInt32(values[0]);

            object ret = DependencyProperty.UnsetValue;
            if (index < values.Length) { ret = values[index]; }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion

    #region SumConverter
    [ValueConversion(typeof(decimal[]), typeof(decimal))]
    public class SumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            decimal? totalNullable = null;
            foreach (object val in values)
            {
                if (val != DependencyProperty.UnsetValue)
                {
                    if (totalNullable.HasValue)
                        totalNullable += System.Convert.ToDecimal(val);
                    else
                        totalNullable = System.Convert.ToDecimal(val);
                }
            }
            if (!totalNullable.HasValue)
                return DependencyProperty.UnsetValue;
            decimal total = totalNullable.Value;
            if (targetType == typeof(string))
                return total.ToString();
            if (targetType == typeof(byte))
                return (byte)total;
            if (targetType == typeof(short))
                return (short)total;
            if (targetType == typeof(int))
                return (int)total;
            if (targetType == typeof(long))
                return (long)total;
            if (targetType == typeof(float))
                return (float)total;
            if (targetType == typeof(double))
                return (double)total;
            return total;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
    #region SumItemsConverter
    [ValueConversion(typeof(ReadOnlyObservableCollection<object>), typeof(object))]
    public sealed class SumItemsConverter : IValueConverter
    {
        #region Operator
        public string Operator { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ReadOnlyObservableCollection<object> groupItems = value as ReadOnlyObservableCollection<object>;
            if (groupItems == null) { return null; }
            string fieldNames = Convert2.ToString(parameter);
            if (StringHelper.IsEmpty(fieldNames)) { return Binding.DoNothing; }
            string[] aFieldNames = fieldNames.Split('.'); // fieldNames può avere la seguente forma parentField.childField....

            PropertyInfo[] pInfo = new PropertyInfo[aFieldNames.Length]; // contiene i property info per il calcolo delle proprietà innestate
            decimal sum = 0;
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
                    if (itemValue != null) { sum += System.Convert.ToDecimal(itemValue); }
                }
                catch (Exception) { } // ignora le eccezioni nel loop di calcolo del raggruppamento
            }

            return sum;
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
    #region SumItemsConverter2
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class SumItemsConverter2 : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable groupItems = value as IEnumerable;
            if (groupItems == null) { return DependencyProperty.UnsetValue; }
            string fieldNames = Convert2.ToString(parameter);
            if (StringHelper.IsEmpty(fieldNames)) { return DependencyProperty.UnsetValue; }
            string[] aFieldNames = fieldNames.Split('.'); // fieldNames può avere la seguente forma parentField.childField....

            PropertyInfo[] pInfo = new PropertyInfo[aFieldNames.Length]; // contiene i property info per il calcolo delle proprietà innestate
            decimal sum = 0;
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
                    if (itemValue != null) { sum += System.Convert.ToDecimal(itemValue); }
                }
                catch (Exception) { } // ignora le eccezioni nel loop di calcolo del raggruppamento
            }

            return sum;
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

    #region SubtractionConverter
    [ValueConversion(typeof(decimal[]), typeof(decimal))]
    public class SubtractionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            decimal? totalNullable = null;
            foreach (object val in values)
            {
                if (val != DependencyProperty.UnsetValue)
                {
                    if (totalNullable.HasValue)
                        totalNullable -= System.Convert.ToDecimal(val);
                    else
                        totalNullable = System.Convert.ToDecimal(val);
                }
            }
            if (!totalNullable.HasValue)
                return DependencyProperty.UnsetValue;
            else
                return (int)totalNullable.Value;
            //decimal total = totalNullable.Value;
            //if (targetType == typeof(string))
            //    return total.ToString();
            //if (targetType == typeof(byte))
            //    return (byte)total;
            //if (targetType == typeof(short))
            //    return (short)total;
            //if (targetType == typeof(int))
            //    return (int)total;
            //if (targetType == typeof(long))
            //    return (long)total;
            //if (targetType == typeof(float))
            //    return (float)total;
            //if (targetType == typeof(double))
            //    return (double)total;
            //return total;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
