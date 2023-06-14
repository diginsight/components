using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common
{

    #region InvokeMethodConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class InvokeMethodConverter : IValueConverter
    {
        #region Method
        public MethodBase Method { get; set; }
        #endregion

        ///<summary>dato un oggetto ritorna il risultato della invocazione di un metodo.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) { return null; }

            object ret = null;
            if (value != null && value != DependencyProperty.UnsetValue)
            {
                if (Method.GetParameters().Length > 1)
                {
                    ret = Method.Invoke(value, parameter as object[]);
                }
                else if (Method.GetParameters().Length == 1)
                {
                    ret = Method.Invoke(value, new object[] { parameter });
                }
                else
                {
                    ret = Method.Invoke(value, null);
                }
            }
            return ret;
        }
        ///<summary>non implemetato.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion
    #region InvokeObjectMethodConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class InvokeObjectMethodConverter : IMultiValueConverter
    {
        #region Method
        public object ObjectInstance { get; set; }
        public MethodBase Method { get; set; }
        #endregion

        #region Convert
        ///<summary>invoca un metodo con l'insieme dei valori provenienti dal multibinding.</summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) { return null; }
            if (ObjectInstance == null) { throw new InvalidDataException("l'attributo 'ObjectInstance' ({ObjectInstance}) non è valido; InvokeObjectMethodConverter.Convert() richiede che l'attributo 'ObjectInstance' sia valorizzato con l'istanza dell'oggetto su cui invocare il metodo.".Replace("{ObjectInstance}", Convert2.ToString(ObjectInstance))); }

            object ret = DependencyProperty.UnsetValue;
            if (values == null || values == DependencyProperty.UnsetValue) { return ret; }
            foreach (var v in values) { if (v == DependencyProperty.UnsetValue) { return ret; } }

            int parCount = Method.GetParameters().Length;
            if (parCount != values.Length) { throw new ArgumentException("values", "il parametro values ('{value}') non è valido; InvokeObjectMethodConverter.Convert() richiede che il parametro values contenga un array di '{parCount}' elementi.".Replace("{parCount}", Convert2.ToString(parCount))); }
            ret = Method.Invoke(ObjectInstance, values);

            return ret;
        }
        #endregion

        #region ConvertBack
        ///<summary>non implemetato.</summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion

    #region PropertyAttributeConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class PropertyAttributeConverter : IValueConverter
    {
        public Type Type { get; set; }
        public Type AttributeType { get; set; }
        public string AttributeProperty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;
            if (this.Type == null) { return DependencyProperty.UnsetValue; }

            string property = Convert2.ToString(value);
            if (property == null) { return DependencyProperty.UnsetValue; }
            MemberInfo[] memInfos = this.Type.GetMember(property);
            if (memInfos == null || memInfos.Length == 0) { return DependencyProperty.UnsetValue; }

            if (this.AttributeType == null) { return DependencyProperty.UnsetValue; }
            if (this.AttributeProperty == null) { return DependencyProperty.UnsetValue; }

            object[] attrs = memInfos[0].GetCustomAttributes(this.AttributeType, true);
            if (attrs == null || attrs.Length == 0) { return DependencyProperty.UnsetValue; }

            PropertyInfo pInfo = this.AttributeType.GetProperty(this.AttributeProperty);
            if (pInfo == null) { return DependencyProperty.UnsetValue; }

            ret = pInfo.GetValue(attrs[0], null);
            return ret;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;
            return ret;
        }
    }
    #endregion
    #region EnumAttributeConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class EnumAttributeConverter : IValueConverter
    {
        public Type EnumType { get; set; }
        public Type AttributeType { get; set; }
        public string AttributeProperty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = null;
            if (this.EnumType == null) { return DependencyProperty.UnsetValue; }

            string property = Convert2.ToString(value);
            if (value == DependencyProperty.UnsetValue || string.IsNullOrEmpty(property)) { return DependencyProperty.UnsetValue; }
            MemberInfo[] memInfos = this.EnumType.GetMember(property);
            if (memInfos == null || memInfos.Length == 0) { return DependencyProperty.UnsetValue; }

            if (this.AttributeType == null) { return DependencyProperty.UnsetValue; }
            if (this.AttributeProperty == null) { return DependencyProperty.UnsetValue; }

            object[] attrs = memInfos[0].GetCustomAttributes(this.AttributeType, true);
            if (attrs == null || attrs.Length == 0) { return DependencyProperty.UnsetValue; }

            PropertyInfo pInfo = this.AttributeType.GetProperty(this.AttributeProperty);
            if (pInfo == null) { return DependencyProperty.UnsetValue; }

            ret = pInfo.GetValue(attrs[0], null);
            return ret;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value;
            return ret;
        }
    }
    #endregion
}
