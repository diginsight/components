#region using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Application = System.Windows.Application;
#endregion

namespace Common
{
    #region ObjectToVisibilityConverter
    ///<summary>Classe di conversione, converte una variabile booleana in una istanza dell'enumeration Visibility.
    ///può essere utilizzata per rendere visibile o meno un controllo sulla base di una variabile booleana</summary>
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class ObjectToVisibilityConverter : IValueConverter
    {
        ///<summary>Esegue la conversione del booleano verso o valori Visibility.Visible o Visibility.Collapsed.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) { return false; }
            bool flag = (value != null) ? true : false;
            return (flag ? Visibility.Visible : Visibility.Collapsed);
        }
        ///<summary>Esegue la conversione inversa.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
        }
    }
    #endregion
    #region ObjectToNotVisibilityConverter
    ///<summary>Classe di conversione, converte una variabile booleana in una istanza dell'enumeration Visibility.
    ///può essere utilizzata per rendere visibile o meno un controllo sulla base di una variabile booleana</summary>
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class ObjectToNotVisibilityConverter : IValueConverter
    {
        ///<summary>Esegue la conversione del booleano verso o valori Visibility.Visible o Visibility.Collapsed.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) { return true; }
            bool flag = (value != null) ? false : true;
            return (flag ? Visibility.Visible : Visibility.Collapsed);
        }
        ///<summary>Esegue la conversione inversa.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((value is Visibility) && (((Visibility)value) == Visibility.Visible));
        }
    }
    #endregion
    #region ObjectToArrayConverter
    [ValueConversion(typeof(object), typeof(object[]))]
    public sealed class ObjectToArrayConverter : IValueConverter
    {
        #region Length
        public int Length { get; set; }
        #endregion

        ///<summary>Esegue la conversione del object verso un array.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<object> list = new List<object>();
            for (int i = 0; i < Length; i++)
            {
                list.Add(value);
            }
            return list.ToArray();
        }
        ///<summary>Esegue la conversione inversa.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new object[1] { value };
        }
    }
    #endregion

    #region UnwindExceptionConverter
    [ValueConversion(typeof(Exception), typeof(Exception))]
    public sealed class UnwindExceptionConverter : IValueConverter
    {
        Regex _unwindExceptionRegularExpression;
        string _unwindExceptionPattern;

        public string UnwindExceptionPattern
        {
            get { return _unwindExceptionPattern; }
            set
            {
                if (!StringHelper.IsEmpty(_unwindExceptionPattern)) { _unwindExceptionRegularExpression = new Regex(_unwindExceptionPattern); } else { _unwindExceptionRegularExpression = null; }
                _unwindExceptionPattern = value;
            }
        }

        ///<summary>Esegue la unwind di una eccezione.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentException("value", "il parametro values ('{value}') non è valido; UnwindExceptionConverter.Convert() richiede values contenga un oggetto Exception.".Replace("{value}", Convert2.ToString(value)));
            var ex = value as Exception;
            var ret = ex;
            if (ex == null) throw new ArgumentException("value", "il parametro values ('{value}') non è un oggetto Exception; UnwindExceptionConverter.Convert() richiede values contenga un oggetto Exception.".Replace("{value}", Convert2.ToString(value)));

            if (ex.InnerException != null && _unwindExceptionRegularExpression != null && _unwindExceptionRegularExpression.IsMatch(ex.GetType().Name))
            {
                ret = ex.InnerException;
            }

            return ret;
        }
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Exception ret = null;
            if (value != null && value != DependencyProperty.UnsetValue && value is Exception)
            {
                ret = value as Exception;
            }
            return ret;
        }
    }
    #endregion

    #region ValueCompareConverter
    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class ValueCompareConverter : IValueConverter, IMultiValueConverter
    {
        private static readonly Type T = typeof(ValueCompareConverter);

        public object Value { get; set; }
        public string Op { get; set; }

        ///<summary>Esegue il confronto di un numerico con un parametro dato, ritorna un booleano.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            using var scope = TraceLogger.BeginMethodScope(T, new { this.Op, value, targetType, parameter, culture });
            bool ret = false;
            try
            {
                object compareValue = Value;
                if (Application.Current?.MainWindow != null && DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return DependencyProperty.UnsetValue; } // 
                if (compareValue == null) { compareValue = parameter; }

                if (value == null)
                {
                    if (Op == "==" && compareValue == null) { ret = true; return ret; }
                    if (Op == "!=" && compareValue != null) { ret = true; return ret; }
                    if (Op == "<=" && compareValue == null) { ret = true; return ret; }
                    if (Op == ">=" && compareValue == null) { ret = true; return ret; }
                    return ret;
                }

                IComparable iValue = value as IComparable;
                if (iValue != null)
                {
                    if (Op == "==" && iValue.CompareTo(compareValue) == 0) { ret = true; return ret; }
                    if (Op == "!=" && iValue.CompareTo(compareValue) != 0) { ret = true; return ret; }
                    if (Op == "<" && iValue.CompareTo(compareValue) < 0) { ret = true; return ret; }
                    if (Op == "<=" && (iValue.CompareTo(compareValue) < 0 || iValue.CompareTo(compareValue) == 0)) { ret = true; return ret; }
                    if (Op == ">=" && (iValue.CompareTo(compareValue) > 0 || iValue.CompareTo(compareValue) == 0)) { ret = true; return ret; }
                    if (Op == ">" && iValue.CompareTo(compareValue) > 0) { ret = true; return ret; }
                    return ret;
                }

                if (value != null)
                {
                    if (Op == "==" && value == compareValue) { ret = true; return ret; }
                    if (Op == "!=" && value != compareValue) { ret = true; return ret; }
                    return ret;
                }
            }
            finally
            {
                TraceLogger.LogDebug($"ret:{ret.GetLogString()}");
            }

            return ret;

        }
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Application.Current?.MainWindow == null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return DependencyProperty.UnsetValue; }
            return Value;
        }

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            using var scope = TraceLogger.BeginMethodScope(T, new { this.Op, values, targetType, parameter, culture });

            bool ret = false;
            try
            {
                object value = values != null && values.Length > 0 ? values[0] : null;
                object compareValue = values != null && values.Length > 1 ? values[1] : null;
                if (Application.Current?.MainWindow == null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return DependencyProperty.UnsetValue; }
                if (compareValue == null) { compareValue = parameter; }
                if (compareValue == null) { compareValue = Value; }
                if (value == null || compareValue == null) { return DependencyProperty.UnsetValue; }
                if (value == DependencyProperty.UnsetValue || compareValue == DependencyProperty.UnsetValue) { return DependencyProperty.UnsetValue; }

                if (value == null)
                {
                    if (Op == "==" && compareValue == null) { ret = true; return ret; }
                    if (Op == "!=" && compareValue != null) { ret = true; return ret; }
                    if (Op == "<=" && compareValue == null) { ret = true; return ret; }
                    if (Op == ">=" && compareValue == null) { ret = true; return ret; }
                    return ret;
                }

                IComparable iValue = value as IComparable;
                if (iValue != null)
                {
                    if (Op == "==" && iValue.CompareTo(compareValue) == 0) { ret = true; return ret; }
                    if (Op == "!=" && iValue.CompareTo(compareValue) != 0) { ret = true; return ret; }
                    if (Op == "<" && iValue.CompareTo(compareValue) < 0) { ret = true; return ret; }
                    if (Op == "<=" && (iValue.CompareTo(compareValue) < 0 || iValue.CompareTo(compareValue) == 0)) { ret = true; return ret; }
                    if (Op == ">=" && (iValue.CompareTo(compareValue) > 0 || iValue.CompareTo(compareValue) == 0)) { ret = true; return ret; }
                    if (Op == ">" && iValue.CompareTo(compareValue) > 0) { ret = true; return ret; }
                    return ret;
                }

                if (value != null)
                {
                    if (Op == "==" && value == compareValue) { ret = true; return ret; }
                    if (Op == "!=" && value != compareValue) { ret = true; return ret; }
                    return ret;
                }
            }
            finally
            {
                TraceLogger.LogDebug($"ret:{ret.GetLogString()}");
            }

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

    #region MapConverter
    /// <summary>
    /// An implementation of <see cref="IValueConverter"/> that converts from one set of values to another based on the contents of the
    /// <see cref="Mappings"/> collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>MapConverter</c> converts from one set of values to another. The source and destination values are stored in instances of
    /// <see cref="Mapping"/> inside the <see cref="Mappings"/> collection. 
    /// </para>
    /// <para>
    /// If this converter is asked to convert a value for which it has no knowledge, it will use the <see cref="FallbackBehavior"/> to determine
    /// how to deal with the situation.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example shows a <c>MapConverter</c> being used to control the visibility of a <c>Label</c> based on a
    /// <c>CheckBox</c>:
    /// <code lang="xml">
    /// <![CDATA[
    /// <CheckBox x:Name="_checkBox"/>
    /// <Label Content="Here is the label.">
    ///		<Label.Visibility>
    ///			<Binding Path="IsChecked" ElementName="_checkBox" FallbackValue="Collapsed">
    ///				<Binding.Converter>
    ///					<MapConverter>
    ///						<Mapping To="{x:Static Visibility.Visible}">
    ///							<Mapping.From>
    ///								<sys:Boolean>True</sys:Boolean>
    ///							</Mapping.From>
    ///						</Mapping>
    ///					</MapConverter>
    ///				</Binding.Converter>
    ///			</Binding>
    ///		</Label.Visibility>
    /// </Label>
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// The following example shows how a <c>MapConverter</c> can be used to convert between values of the <see cref="UriFormat"/>
    /// enumeration and human-readable strings. Notice how not all possible values are present in the mappings. The fallback behavior
    /// is set to <c>ReturnOriginalValue</c> to ensure that any conversion failures result in the original value being returned:
    /// <code lang="xml">
    /// <![CDATA[
    /// <Label>
    ///		<Label.Content>
    ///			<Binding Path="UriFormat">
    ///				<Binding.Converter>
    ///					<MapConverter FallbackBehavior="ReturnOriginalValue">
    ///						<Mapping From="{x:Static sys:UriFormat.SafeUnescaped}" To="Safe unescaped"/>
    ///						<Mapping From="{x:Static sys:UriFormat.UriEscaped}" To="URI escaped"/>
    ///					</MapConverter>
    ///				</Binding.Converter>
    ///			</Binding>
    ///		</Label.Content>
    ///	</Label>
    /// ]]>
    /// </code>
    /// </example>
    [ContentProperty("Mappings")]
    [ValueConversion(typeof(object), typeof(object))]
    public class MapConverter : DependencyObject, IValueConverter
    {

        #region Mappings
        private static readonly DependencyPropertyKey _mappingsPropertyKey = DependencyProperty.RegisterReadOnly("Mappings", typeof(Collection<Mapping>), typeof(MapConverter), new FrameworkPropertyMetadata());
        /// <summary>Identifies the <see cref="Mappings"/> dependency property. </summary>
        public static readonly DependencyProperty MappingsProperty = _mappingsPropertyKey.DependencyProperty;
        /// <summary> Gets the collection of <see cref="Mapping"/>s configured for this <c>MapConverter</c>. </summary>
        /// <remarks>
        /// <para>
        /// Each <see cref="Mapping"/> defines a relationship between a source object (see <see cref="Mapping.From"/>) and a destination (see
        /// <see cref="Mapping.To"/>). The <c>MapConverter</c> uses these mappings whilst attempting to convert values.
        /// </para>
        /// </remarks>
        public Collection<Mapping> Mappings
        {
            get { return GetValue(MappingsProperty) as Collection<Mapping>; }
            private set { SetValue(_mappingsPropertyKey, value); }
        }
        #endregion

        #region .ctor
        /// <summary> Constructs an instance of <c>MapConverter</c>. </summary>
        public MapConverter() { Mappings = new Collection<Mapping>(); }
        #endregion

        #region LeaveUnmachedValues
        public bool LeaveUnmachedValues { get; set; }
        #endregion

        #region Convert
        /// <summary> Attempts to convert the specified value. </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (Mapping mapping in Mappings)
            {
                if (object.Equals(value, mapping.From))
                {
                    return mapping.To;
                }
            }

            if (this.LeaveUnmachedValues)
                return value;
            if (this.DefaultValue != null)
                return this.DefaultValue;
            return DependencyProperty.UnsetValue;
        }
        #endregion

        #region ConvertBack
        /// <summary> Attempts to convert the specified value back. </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (Mapping mapping in Mappings)
            {
                if (object.Equals(value, mapping.To))
                {
                    return mapping.From;
                }
            }

            return this.LeaveUnmachedValues ? value : DependencyProperty.UnsetValue;
        }
        #endregion

        #region DefaultValue
        public object DefaultValue
        {
            get { return (object)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(object), typeof(MapConverter), new UIPropertyMetadata());
        #endregion
    }
    #endregion
    #region Mapping
    public class Mapping : DependencyObject
    {
        /// <summary>Identifies the <see cref="From"/> dependency property.</summary>
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(object), typeof(Mapping));

        /// <summary>Identifies the <see cref="To"/> dependency property.</summary>
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(object), typeof(Mapping));

        /// <summary>Gets or sets the source object for the mapping.</summary>
        [ConstructorArgument("from")]
        public object From
        {
            get { return GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        /// <summary>Gets or sets the destination object for the mapping.</summary>
        [ConstructorArgument("to")]
        public object To
        {
            get { return GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        #region .ctor
        /// <summary>Constructs a default instance of <c>Mapping</c>.</summary>
        public Mapping() { }
        #endregion

        #region Mapping
        /// <summary>Constructs an instance of <c>Mapping</c> with the specified <paramref name="from"/> and <paramref name="to"/> values.</summary>
        /// <param name="from">The value for the source in the mapping (see <see cref="From"/>).</param>
        /// <param name="to">The value for the destination in the mapping (see <see cref="To"/>).</param>
        public Mapping(object from, object to)
        {
            From = from;
            To = to;
        }
        #endregion
    }
    #endregion

    #region DoubleToGridLenght
    [ValueConversion(typeof(object), typeof(object[]))]
    public sealed class DoubleToGridLenght : IValueConverter
    {
        ///<summary>Esegue la conversione del object verso un array.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? v = value as double?;
            GridLength ret;
            if (v.HasValue)
                ret = new GridLength(v.Value);
            else
                ret = new GridLength();
            return ret;
        }
        ///<summary>Esegue la conversione inversa.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength? v = value as GridLength?;
            if (v.HasValue)
            {
                double? ret = v.Value.Value;
            }
            return null;
        }
    }
    #endregion


}
