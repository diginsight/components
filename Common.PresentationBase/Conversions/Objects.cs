#region using
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;
#endregion

namespace Common
{
    #region FormatConverter
    [ValueConversion(typeof(object), typeof(string))]
    public sealed class FormatConverter : IValueConverter, IMultiValueConverter
    {
        #region Format
        public string Format { get; set; }
        #endregion

        #region NullFormat
        public string NullFormat { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string format = Format;
            if (string.IsNullOrEmpty(format)) { format = Convert2.ToString(parameter); }
            string nullFormat = NullFormat;
            if (value == null && NullFormat != null) { format = nullFormat; }
            if (format == null) { return DependencyProperty.UnsetValue; }

            string ret = culture != null ? string.Format(culture, format, value) : string.Format(format, value);
            return ret;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) throw new ArgumentException("values", "Il parametro values ('{values}') non è un valido; FormatConverter.Convert() richiede values contenga almeno un elemento.".Replace("{values}", Convert2.ToString(values)));
            string format = Format;
            if (string.IsNullOrEmpty(format) && parameter != null) { format = Convert2.ToString(parameter); }
            string nullFormat = NullFormat;
            if (values == null && NullFormat != null) { format = nullFormat; }
            object[] formatValues = values;
            if (format == null && values.Length > 0)
            {
                format = values[0] as String;
                formatValues = new object[values.Length - 1];
                Array.Copy(values, 1, formatValues, 0, formatValues.Length);
            }
            if (format == null) { return DependencyProperty.UnsetValue; }

            string ret = culture != null ? string.Format(culture, format, formatValues) : string.Format(format, formatValues);
            return ret;
        }
        #endregion

        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object objRet = value;
            IConvertible convertible = value as IConvertible;

            if (convertible != null && targetType != null)
            {
                objRet = convertible.ToType(targetType, CultureInfo.CurrentUICulture);
            }

            return objRet;
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
    #region ToStringConverter
    [ValueConversion(typeof(object), typeof(string))]
    public class ToStringConverter : TransformConverter<object, object>
    {
        public ToStringConverter()
        {
            this.Trasformation = delegate (object o)
            {
                return o != null ? o.ToString() : DependencyProperty.UnsetValue;
            };
            this.Trasformation2 = delegate (object[] oa)
            {
                var o = oa != null && oa.Length > 0 ? oa[0] : null;
                return o != null ? o.ToString() : DependencyProperty.UnsetValue;
            };
        }
    }
    #endregion

    #region ObjectToBooleanConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class ObjectToBooleanConverter : IValueConverter
    {
        #region internal state
        private bool _nullValue;
        private bool _nnullValue = true;
        #endregion

        #region NullValue
        public bool NullValue
        {
            get { return _nullValue; }
            set { _nullValue = value; }
        }
        #endregion

        #region NNullValue
        public bool NNullValue
        {
            get { return _nnullValue; }
            set { _nnullValue = value; }
        }
        #endregion

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? NNullValue : NullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion

    #region ReplaceObjectConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(object)), Obsolete]
    public sealed class ReplaceObjectConverter : IValueConverter
    {
        public object StartValue { get; set; }
        public object ReplaceValue { get; set; }
        public BindingMode Mode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;

            if (Mode == BindingMode.TwoWay || Mode == BindingMode.OneWay || Mode == BindingMode.OneTime)
            {
                if (value == null) { return StartValue == null ? ReplaceValue : value; }
                if (value.Equals(StartValue)) { ret = ReplaceValue; }
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;

            if (Mode == BindingMode.TwoWay || Mode == BindingMode.OneWayToSource || Mode == BindingMode.OneTime)
            {
                if (value == null) { return ReplaceValue == null ? StartValue : value; }
                if (value.Equals(ReplaceValue)) { ret = StartValue; }
            }

            return ret;
        }
    }
    #endregion
    #region OneWayReplaceConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class OneWayReplaceConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var startValue = parameter;
            object ret = value;
            if (value == null) { return startValue == null ? DependencyProperty.UnsetValue : value; }
            if (value.Equals(startValue)) { ret = DependencyProperty.UnsetValue; }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;
            return ret;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0];
            var startValue = parameter;
            object ret = value;
            if (value == null) { return startValue == null ? DependencyProperty.UnsetValue : value; }
            if (value.Equals(startValue)) { ret = DependencyProperty.UnsetValue; }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object ret = value;
            return new object[] { ret };
        }
        #endregion
    }
    #endregion
    #region NullReplacerObjectConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class NullReplacerObjectConverter : IValueConverter, IMultiValueConverter
    {
        public object ReplaceValue { get; set; }
        ///<summary>Se il primo parametro è nullo lo sostituisce col secondo (Eg. se importo è nullo viene sostituito da "ND")
        ///         La priorità dei valori è:
        ///         1)il secondo parametro passato al convertitore (permette di usare un binding, Eg. se codice tipo garanzia non è presente usa codice)
        ///         2)il convert parameter (Eg. importo non presente)
        ///         3)il Replace value specificato in fase di dichiarazione del convertitore (Eg. dato non presente)
        ///</summary>
        /// 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null && parameter != null) ? parameter :
                   (value == null && ReplaceValue != null) ? ReplaceValue : value;
        }
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object retVal = DependencyProperty.UnsetValue;
            if (values.Length == 2)
            {
                retVal = ((values[0] == null || values[0] == DependencyProperty.UnsetValue) && (values[1] != null && values[1] != DependencyProperty.UnsetValue)) ? values[1] :
                         ((values[0] == null || values[0] == DependencyProperty.UnsetValue) && parameter != null) ? parameter :
                         ((values[0] == null || values[0] == DependencyProperty.UnsetValue) && ReplaceValue != null) ? ReplaceValue : values[0];
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion
    #region ReplaceOnError
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class ReplaceOnError : IValueConverter
    {
        public object ReplaceValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value == null) { return StartValue == null ? ReplaceValue : value; }
            DependencyObject dObj = parameter as DependencyObject;

            if (dObj == null) { return DependencyProperty.UnsetValue; }

            bool hasError = (bool)dObj.GetValue(Validation.HasErrorProperty);

            return hasError ? ReplaceValue : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value == null) { return ReplaceValue == null ? StartValue : value; }
            return value;
        }
    }
    #endregion

    #region ApplyConverterConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class ApplyConverterConverter : IValueConverter, IMultiValueConverter
    {
        #region LeaveUnmachedValues
        public bool LeaveUnmachedValues { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;
            var converter = parameter as IValueConverter;
            if (converter == null && this.LeaveUnmachedValues) { return value; }
            if (converter == null && this.LeaveUnmachedValues == false) { return DependencyProperty.UnsetValue; }

            if (value == null) { return converter.Convert(value, targetType, parameter, culture); }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;
            return ret;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            var converter = Collections.TryGetItem<object>(values, i++);
            var converterValues = values.SkipChecked(1).ToArrayChecked();
            if (converter == null) { return DependencyProperty.UnsetValue; }

            if (converterValues != null && converterValues.Count() == 1)
            {
                var iValueConverter = converter as IValueConverter;
                if (iValueConverter == null) { return DependencyProperty.UnsetValue; }
                return iValueConverter.Convert(converterValues[0], targetType, parameter, culture);
            }

            if (converterValues != null && converterValues.Count() > 1)
            {
                var iMultiValueConverter = converter as IMultiValueConverter;
                if (iMultiValueConverter == null) { return DependencyProperty.UnsetValue; }
                return iMultiValueConverter.Convert(converterValues, targetType, parameter, culture);
            }

            return DependencyProperty.UnsetValue;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object ret = value;
            return new object[] { ret };
        }
        #endregion
    }
    #endregion


    #region GetHashCodeConverter
    [ValueConversion(typeof(object), typeof(int))]
    public sealed class GetHashCodeConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int ret = value != null ? value.GetHashCode() : 0;
            return ret;
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

    #region GetTypeConverter
    [ValueConversion(typeof(object), typeof(Type))]
    public class GetTypeConverter : TransformConverter<object, Type>
    {
        public GetTypeConverter()
        {
            this.Trasformation = delegate (object o)
            {
                Type t = o != null ? o.GetType() : null;
                return t;
            };
            this.Trasformation2 = delegate (object[] oa)
            {
                var o = oa != null && oa.Length > 0 ? oa[0] : null;
                Type t = o != null ? o.GetType() : null;
                return t;
            };
        }
    }
    #endregion
    #region GetTypeNameConverter
    [ValueConversion(typeof(object), typeof(string))]
    public class GetTypeNameConverter : TransformConverter<object, string>
    {
        public GetTypeNameConverter()
        {
            this.Trasformation = delegate (object o)
            {
                Type t = o != null ? o.GetType() : null;
                string ret = t != null ? t.Name : null;
                return ret;
            };
            this.Trasformation2 = delegate (object[] oa)
            {
                var o = oa != null && oa.Length > 0 ? oa[0] : null;
                Type t = o != null ? o.GetType() : null;
                string ret = t != null ? t.Name : null;
                return ret;
            };
        }
    }
    #endregion
    #region GetTypeNamespaceConverter
    [ValueConversion(typeof(object), typeof(string))]
    public class GetTypeNamespaceConverter : TransformConverter<object, string>
    {
        public GetTypeNamespaceConverter()
        {
            this.Trasformation = delegate (object o)
            {
                Type t = o != null ? o.GetType() : null;
                string ret = t != null ? t.Namespace : null;
                return ret;
            };
            this.Trasformation2 = delegate (object[] oa)
            {
                var o = oa != null && oa.Length > 0 ? oa[0] : null;
                Type t = o != null ? o.GetType() : null;
                string ret = t != null ? t.Namespace : null;
                return ret;
            };
        }
    }
    #endregion
    #region GetAssemblyNameConverter
    [ValueConversion(typeof(object), typeof(string))]
    public class GetAssemblyNameConverter : TransformConverter<object, string>
    {
        public GetAssemblyNameConverter()
        {
            this.Trasformation = delegate (object o)
            {
                Type t = o != null ? o.GetType() : null;
                Assembly a = t != null ? t.Assembly : null;
                string ret = a != null ? a.GetName().Name : null;
                return ret;
            };
            this.Trasformation2 = delegate (object[] oa)
            {
                var o = oa != null && oa.Length > 0 ? oa[0] : null;
                Type t = o != null ? o.GetType() : null;
                Assembly a = t != null ? t.Assembly : null;
                string ret = a != null ? a.GetName().Name : null;
                return ret;
            };
        }
    }
    #endregion

    #region IsConverter
    [ValueConversion(typeof(Type), typeof(bool))]
    public sealed class IsConverter : IValueConverter
    {
        public Type Type { get; set; }

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type baseType = parameter as Type;

            if (baseType == null && parameter != null) { baseType = parameter.GetType(); }
            if (baseType == null) { baseType = Type; }
            if (baseType == null) { return DependencyProperty.UnsetValue; }

            Type type = value != null ? value.GetType() : null;
            if (type == null) { return DependencyProperty.UnsetValue; }

            return baseType.IsInstanceOfType(value);
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
            Type baseType = values != null && values.Length > i ? values[i] as Type : null; i++;
            if (baseType == null && parameter != null) { baseType = parameter.GetType(); }
            if (baseType == null) { baseType = Type; }
            if (baseType == null) { return DependencyProperty.UnsetValue; }

            Type type = value != null ? value.GetType() : null;
            if (type == null) { return DependencyProperty.UnsetValue; }

            return baseType.IsInstanceOfType(value);
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
    #region IsNotConverter
    [ValueConversion(typeof(Type), typeof(bool))]
    public sealed class IsNotConverter : IValueConverter
    {
        public Type Type { get; set; }

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type baseType = parameter as Type;

            if (baseType == null && parameter != null) { baseType = parameter.GetType(); }
            if (baseType == null) { baseType = Type; }
            if (baseType == null) { return DependencyProperty.UnsetValue; }

            Type type = value != null ? value.GetType() : null;
            if (type == null) { return DependencyProperty.UnsetValue; }

            return !baseType.IsInstanceOfType(value);
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
            Type baseType = values != null && values.Length > i ? values[i] as Type : null; i++;

            if (baseType == null && parameter != null) { baseType = parameter.GetType(); }
            if (baseType == null) { baseType = Type; }
            if (baseType == null) { return DependencyProperty.UnsetValue; }

            Type type = value != null ? value.GetType() : null;
            if (type == null) { return DependencyProperty.UnsetValue; }

            return !baseType.IsInstanceOfType(value);
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

    #region IdentityConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class IdentityConverter : IValueConverter, IMultiValueConverter
    {
        private static readonly Type T = typeof(IdentityConverter);
        
        ///<summary>ritorna l'oggetto sorgente.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //using var scope = TraceLogger.BeginMethodScope(T, new { value, targetType, parameter, culture });
            return value;
        }

        ///<summary>ritorna l'oggetto target.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //using var scope = TraceLogger.BeginMethodScope(T, new { values, targetType, parameter, culture });
            return values;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
    }
    #endregion
    #region ParameterConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class ParameterConverter : IValueConverter, IMultiValueConverter
    {
        ///<summary>ritorna l'oggetto sorgente.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }

        ///<summary>ritorna l'oggetto target.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { parameter };
        }
    }
    #endregion

    #region ExceptionConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class ExceptionConverter : IValueConverter, IMultiValueConverter
    {
        public Exception Exception { get; set; }

        ///<summary>ritorna l'oggetto sorgente.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string message = parameter != null ? Convert2.ToString(parameter) : Convert2.ToString(value);
            Exception exception = new ClientException(message) { ExceptionLevel = ExceptionLevel.Information };
            return exception;
        }

        ///<summary>ritorna l'oggetto target.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string message = parameter != null ? Convert2.ToString(parameter) : Convert2.ToString(values);
            Exception exception = new ClientException(message) { ExceptionLevel = ExceptionLevel.Information };
            return exception;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion

    #region ObjectCompareConverter
    [ValueConversion(typeof(decimal), typeof(bool))]
    public sealed class ObjectCompareConverter : IValueConverter
    {
        public object Limit { get; set; }
        public string Op { get; set; }

        ///<summary>Esegue il confronto di un numerico con un parametro dato, ritorna un booleano.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IComparable iComparable = value as IComparable;
            if (iComparable == null) { return DependencyProperty.UnsetValue; }

            object limit = Limit;
            if (parameter != null) { limit = parameter; }

            bool ret = false;
            int compareTo = iComparable.CompareTo(limit);
            if (Op == "==" && compareTo == 0) { ret = true; }
            if (Op == "!=" && compareTo != 0) { ret = true; }
            if (Op == "<" && compareTo == -1) { ret = true; }
            if (Op == "<=" && (compareTo == -1 || compareTo == 0)) { ret = true; }
            if (Op == ">=" && (compareTo == 1 || compareTo == 0)) { ret = true; }
            if (Op == ">" && compareTo == 1) { ret = true; }
            return ret;
        }

        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Limit;
        }
    }
    #endregion

    //#region ChiaveDescrizioneConverter
    //// [Localizability(LocalizationCategory.NeverLocalize)]
    ////[ValueConversion(typeof(ChiaveDescrizione), typeof(string))]
    //public sealed class ChiaveDescrizioneConverter : IValueConverter
    //{
    //    public BindingMode Mode { get; set; }

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        object ret = value;

    //        if (Mode == BindingMode.TwoWay || Mode == BindingMode.OneWay || Mode == BindingMode.OneTime)
    //        {
    //            string chiave = Convert2.ToString(value);
    //            ret = new ChiaveDescrizione(chiave, "");
    //        }

    //        return ret;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        object ret = value;

    //        if (Mode == BindingMode.TwoWay || Mode == BindingMode.OneWayToSource || Mode == BindingMode.OneTime)
    //        {
    //            ChiaveDescrizione keyDesc = value as ChiaveDescrizione;

    //            if (keyDesc != null)
    //            {
    //                ret = keyDesc.Chiave;
    //            }
    //        }

    //        return ret;
    //    }
    //}
    //#endregion

    #region GetValidationErrorConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class GetValidationErrorConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        ///<summary>ritorna l'oggetto sorgente.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList<ValidationError> errors = value as IList<ValidationError>;
            ValidationError validationError = errors != null ? errors.Where(e => !IsIgnorableValidation(e)).LastOrDefault() : null;
            return validationError;
        }
        #endregion
        #region ConvertBack
        ///<summary>ritorna l'oggetto target.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            IList<ValidationError> errors = values != null && values.Length > i ? values[i] as IList<ValidationError> : null;
            ValidationError validationError = errors != null ? errors.Where(e => !IsIgnorableValidation(e)).LastOrDefault() : null;
            return validationError;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion

        #region IsIgnorableValidation
        public static bool IsIgnorableValidation(ValidationError e)
        {
            bool isIgnorable = false;
            if (e.RuleInError is ExceptionValidationRule)
            { // ExceptionValidationRule
                BindingExpressionBase expr = e.BindingInError as BindingExpressionBase;
                Binding binding = expr.ParentBindingBase as Binding;
                if (binding.ValidationRules == null || !binding.ValidationRules.Contains(e.RuleInError)) { isIgnorable = true; }
                //if (binding.ValidatesOnExceptions == false && (binding.ValidationRules == null || !binding.ValidationRules.Contains(e.RuleInError))) { isIgnorable = true; }
            }

            return isIgnorable;
        }
        #endregion
    }
    #endregion
    #region GetValidationErrorLevelConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class GetValidationErrorLevelConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        ///<summary>ritorna l'oggetto sorgente.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList<ValidationError> errors = value as IList<ValidationError>;
            ValidationError validationError = errors != null ? errors.Where(e => !IsIgnorableValidation(e)).LastOrDefault() : null;

            var ex = validationError.Exception;
            if (ex is ClientException clientException) { return (int)clientException.ExceptionLevel; }
            return (int)ExceptionLevel.Critical;
        }
        #endregion
        #region ConvertBack
        ///<summary>ritorna l'oggetto target.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            IList<ValidationError> errors = values != null && values.Length > i ? values[i] as IList<ValidationError> : null;
            ValidationError validationError = errors != null ? errors.Where(e => !IsIgnorableValidation(e)).LastOrDefault() : null;

            var ex = validationError?.Exception;
            if (ex is ClientException clientException) { return (int)clientException.ExceptionLevel; }

            var rule = validationError?.RuleInError;
            if (rule is ExceptionRule exceptionRule) {
                if (exceptionRule.Exception is ClientException clientException1) { return (int)clientException1.ExceptionLevel; }
            }
            return (int)ExceptionLevel.Critical;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion

        #region IsIgnorableValidation
        public static bool IsIgnorableValidation(ValidationError e)
        {
            bool isIgnorable = false;
            if (e.RuleInError is ExceptionValidationRule)
            { // ExceptionValidationRule
                BindingExpressionBase expr = e.BindingInError as BindingExpressionBase;
                Binding binding = expr.ParentBindingBase as Binding;
                if (binding.ValidationRules == null || !binding.ValidationRules.Contains(e.RuleInError)) { isIgnorable = true; }
                //if (binding.ValidatesOnExceptions == false && (binding.ValidationRules == null || !binding.ValidationRules.Contains(e.RuleInError))) { isIgnorable = true; }
            }

            return isIgnorable;
        }
        #endregion
    }
    #endregion
    #region GetValidationResultLevelConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class GetValidationResultLevelConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        ///<summary>ritorna l'oggetto sorgente.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value as ValidationResult;
            if (result is ExceptionValidationResult exceptionResult)
            {
                var ex = exceptionResult.Exception;
                if (ex is ClientException clientException) { return (int)clientException.ExceptionLevel; }
            }
            return (int)ExceptionLevel.Critical;
        }
        #endregion
        #region ConvertBack
        ///<summary>ritorna l'oggetto target.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            var result = values != null && values.Length > i ? values[i] as ValidationResult : null;

            if (result is ExceptionValidationResult exceptionResult)
            {
                var ex = exceptionResult.Exception;
                if (ex is ClientException clientException) { return (int)clientException.ExceptionLevel; }
            }
            return (int)ExceptionLevel.Critical;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion

        #region IsIgnorableValidation
        public static bool IsIgnorableValidation(ValidationError e)
        {
            bool isIgnorable = false;
            if (e.RuleInError is ExceptionValidationRule)
            { // ExceptionValidationRule
                BindingExpressionBase expr = e.BindingInError as BindingExpressionBase;
                Binding binding = expr.ParentBindingBase as Binding;
                if (binding.ValidationRules == null || !binding.ValidationRules.Contains(e.RuleInError)) { isIgnorable = true; }
                //if (binding.ValidatesOnExceptions == false && (binding.ValidationRules == null || !binding.ValidationRules.Contains(e.RuleInError))) { isIgnorable = true; }
            }

            return isIgnorable;
        }
        #endregion
    }
    #endregion



    #region GetErrorContentConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class GetErrorContentConverter : IValueConverter, IMultiValueConverter
    {
        ///<summary>ritorna l'oggetto sorgente.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList<ValidationError> errors = value as IList<ValidationError>;
            ValidationError validationError = errors != null ? errors.Where(e => !GetValidationErrorConverter.IsIgnorableValidation(e)).LastOrDefault() : null;
            return validationError != null ? validationError.ErrorContent : string.Empty;
        }

        ///<summary>ritorna l'oggetto target.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            IList<ValidationError> errors = values != null && values.Length > i ? values[i] as IList<ValidationError> : null;
            ValidationError validationError = errors != null ? errors.Where(e => !GetValidationErrorConverter.IsIgnorableValidation(e)).LastOrDefault() : null;
            return validationError != null ? validationError.ErrorContent : string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion
}
