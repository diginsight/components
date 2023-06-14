#region using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data; 
#endregion

namespace Common
{
    #region OperationEnum
    public enum OperationEnum
    {
        AND,
        NAND,
        OR,
        NOR,
        XOR,
        XNOR
    }
    #endregion

    #region NotConverter
    ///<summary>negates a boolean value.</summary>
    ///<example>Example use:
    ///     <Button HorizontalAlignment="Right" Margin="0,132,101,0" Name="button2" Width="75" IsCancel="True" Height="23" VerticalAlignment="Top" IsEnabled="{Binding Path=IsSevereException, Converter={StaticResource notConverter}}" >Annulla</Button>
    ///     In this case, the button is disabled, when IsSevereException is true
    ///</example>
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(bool), typeof(bool))]
    public sealed class NotConverter : IValueConverter
    {
        ///<summary>Esegue la conversione del booleano con la sua versione negata.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) { return false; }

            bool ret = false;
            if (value != null && value != DependencyProperty.UnsetValue)
            {
                ret = !System.Convert.ToBoolean(value);
            }
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
    #region BinaryOperatorConverter
    public sealed class BinaryOperatorConverter : IMultiValueConverter
    {
        #region constants
        #endregion

        #region Operation
        public OperationEnum Operation { get; set; }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool? result = null;
            if (values == null || values == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;

            foreach (object value in values)
            {
                object item = value;
                if (item == null || item == DependencyProperty.UnsetValue)
                {
                    continue;
                }
                else
                {
                    if (!(item is bool))
                    {
                        bool res = false; bool success = bool.TryParse(Convert2.ToString(item), out res);
                        if (!success) { return DependencyProperty.UnsetValue; } // throw new ArgumentException("item", E_VALUENULL); 
                        item = res;
                    }

                    bool valore = (bool)item;
                    if (result == null)
                    {
                        result = valore;
                    }
                    else
                    {
                        switch (Operation)
                        {
                            case OperationEnum.AND:
                                result = (bool)result && valore;
                                break;
                            case OperationEnum.NAND:
                                result = !((bool)result && valore);
                                break;
                            case OperationEnum.OR:
                                result = (bool)result || valore;
                                break;
                            case OperationEnum.NOR:
                                result = !((bool)result || valore);
                                break;
                            case OperationEnum.XOR:
                                result = (bool)result ? !valore : valore;
                                break;
                            case OperationEnum.XNOR:
                                result = (bool)result ? valore : !valore;
                                break;
                        }
                    }
                }
            }
            return result != null ? (bool)result : DependencyProperty.UnsetValue;
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

    #region HasFlagConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class HasFlagConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value != null ? (int)value : 0;
            var par = value != null ? (int)value : 0;

            bool hasFlag = (val | par) != 0 ? true : false;

            return hasFlag;
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
            var val = Collections.TryGetItem<int>(values, i++);
            var par = Collections.TryGetItem<int>(values, i++);

            bool hasFlag = (val | par) != 0 ? true : false;
            return hasFlag;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[2] { Binding.DoNothing, Binding.DoNothing };
        }
        #endregion
    }
    #endregion

    #region BooleanToObjectConverter
    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanToObjectConverter : IValueConverter, IMultiValueConverter
    {
        public BooleanToObjectConverter() { }

        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

        #region Convert
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object trueValue = parameter ?? this.TrueValue;
            object falseValue = this.FalseValue ?? DependencyProperty.UnsetValue;
            if (value == null) { return falseValue; }

            bool condition;
            if (value is bool)
            {
                condition = (bool)value;
            }
            else
            {
                string svalue = Convert2.ToString(value);
                bool ok = bool.TryParse(svalue, out condition);
                if (!ok) { return falseValue; }
            }

            return condition ? trueValue : falseValue;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object trueValue = parameter ?? this.TrueValue;
            object falseValue = this.FalseValue ?? DependencyProperty.UnsetValue;
            if (value == null) { return DependencyProperty.UnsetValue; }

            return value.Equals(trueValue);
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object trueValue = this.TrueValue ?? DependencyProperty.UnsetValue;
            object falseValue = this.FalseValue ?? DependencyProperty.UnsetValue;
            if (values == null) { return falseValue; }
            if (values.Length > 1) { trueValue = values[1]; }
            if (values.Length > 2) { falseValue = values[2]; }
            if (values.Length == 0) { return falseValue; }

            object value = values[0];
            if (value == null || value == DependencyProperty.UnsetValue) { value = false; }

            bool condition;
            if (value is bool)
            {
                condition = (bool)value;
            }
            else
            {
                string svalue = Convert2.ToString(value);
                bool ok = bool.TryParse(svalue, out condition);
                if (!ok) { return falseValue; }
            }

            object ret = condition ? trueValue : falseValue;
            return ret;
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
}
