using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common
{
    #region IsInConverter
    [ValueConversion(typeof(IList), typeof(bool))]
    public class IsInConverter : IValueConverter, IMultiValueConverter
    {
        #region List
        public IList List { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList list = parameter as IList ?? this.List as IList;
            if (list == null) { return DependencyProperty.UnsetValue; }

            bool ret = false;
            if (list.Contains(value)) { ret = true; }
            return ret;
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
            IList list = values != null && values.Length > i ? values[i] as IList : null; i++;
            if (list == null) { list = parameter as IList; }
            if (list == null) { list = this.List as IList; }
            if (list == null) { return DependencyProperty.UnsetValue; }

            bool ret = false;
            if (list.Contains(value)) { ret = true; }
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
    #region ContainsConverter
    [ValueConversion(typeof(IList), typeof(bool))]
    public class ContainsConverter : IValueConverter, IMultiValueConverter
    {
        #region Negate
        public bool Negate { get; set; }
        #endregion
        #region Element
        public object Element { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object element = parameter as object ?? this.Element as object;
            if (element == null) { return DependencyProperty.UnsetValue; }

            bool ret = false;
            if (!(value is IList list)) { return false; }
            if (list.Contains(element)) { ret = true; }
            return ret;
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
            IList list = values != null && values.Length > i ? values[i] as IList : null; i++;
            object element = values != null && values.Length > i ? values[i] : null; i++;
            if (list == null) { return false; }
            if (element == null) { element = this.Element; }
            if (element == null) { element = parameter as object; }
            if (element == null) { return DependencyProperty.UnsetValue; }

            bool ret = false;
            if (list.Contains(element)) { ret = true; }
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

    #region ChooseItemConverter
    [ValueConversion(typeof(IList), typeof(object))]
    public sealed class ChooseItemConverter : IValueConverter, IMultiValueConverter
    {
        #region Item
        int _item;
        public int Item
        {
            get { return _item; }
            set { _item = value; }
        }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? item = parameter is int ? (int?)parameter : null;
            if (item == null) { item = this.Item; }
            if (item == null) { return DependencyProperty.UnsetValue; }

            object ret = DependencyProperty.UnsetValue;
            if (value is IList)
            {
                if (value is IList aValue)
                {
                    if (item >= 0 && aValue.Count > item) { ret = aValue[Item]; }
                    if (item == -1 && aValue.Count > 0) { ret = aValue[aValue.Count - 1]; }
                }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int? item = parameter is int ? (int?)parameter : null;
            if (item == null) { item = this.Item; }
            if (item == null) { return DependencyProperty.UnsetValue; }

            object ret = DependencyProperty.UnsetValue;
            if (values is IList)
            {
                if (values is IList aValue)
                {
                    if (item >= 0 && aValue.Count > item) { ret = aValue[Item]; }
                    if (item == -1 && aValue.Count > 0) { ret = aValue[aValue.Count - 1]; }
                }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion
    #region SelectItemConverter
    [ValueConversion(typeof(IList), typeof(object))]
    public sealed class SelectItemConverter : IValueConverter, IMultiValueConverter
    {
        #region .ctor
        public SelectItemConverter() { Item = 1; }
        #endregion

        #region Item
        int _item;
        public int Item
        {
            get { return _item; }
            set { _item = value; }
        }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? item = parameter is int ? (int?)parameter : null;
            if (item == null) { item = this.Item; }
            if (item == null) { return DependencyProperty.UnsetValue; }

            object ret = DependencyProperty.UnsetValue;
            if (value is IList)
            {
                if (value is IList aValue)
                {
                    if (item >= 0 && aValue.Count > item) { ret = aValue[Item]; }
                    if (item == -1 && aValue.Count > 0) { ret = aValue[aValue.Count - 1]; }
                }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int? item = parameter is int ? (int?)parameter : null;
            if (item == null) { item = this.Item; }
            if (item == null) { return DependencyProperty.UnsetValue; }

            object ret = DependencyProperty.UnsetValue;
            if (values is IList)
            {
                if (values is IList aValue)
                {
                    if (item >= 0 && aValue.Count > item) { ret = aValue[Item]; }
                    if (item == -1 && aValue.Count > 0) { ret = aValue[aValue.Count - 1]; }
                }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion
    #region FindItemConverter
    [ValueConversion(typeof(object[]), typeof(bool))]
    public sealed class FindItemConverter : IValueConverter
    {
        #region .ctor
        public FindItemConverter() { }
        #endregion

        #region Item
        object Item { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object item = Item;
            if (parameter != null) { item = parameter; }

            bool ret = false;
            if (value is IList)
            {
                IList list = value as IList;
                ret = list.Contains(item);
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion
    #region FirstOfConverter
    [ValueConversion(typeof(object[]), typeof(object))]
    public sealed class FirstOfConverter : IMultiValueConverter
    {
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = null;
            if (values != null && values.Length > 0)
            {
                ret = values[0];
                for (int i = 0; (ret == null || ret == DependencyProperty.UnsetValue) && i < values.Length; i++) { ret = values[i]; }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion

    #region CollectionConverter
    [ValueConversion(typeof(object), typeof(object))]
    public class CollectionConverter<C> : IValueConverter, IMultiValueConverter where C : IList, new()
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable sourcelist = value is IEnumerable ? (IEnumerable)value : value != null ? new object[] { value } : null;
            object ret = CollectionHelper.Clone<C>(sourcelist, true);
            if (ret == null) return DependencyProperty.UnsetValue;
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = CollectionHelper.Clone<C>(values, true);
            if (ret == null) return DependencyProperty.UnsetValue;
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
        #endregion
    }

    public class CollectionConverter : CollectionConverter<ObservableCollection<object>> { }
    public class CollectionConverterString : CollectionConverter<ObservableCollection<string>> { }
    public class CollectionConverterInt16 : CollectionConverter<ObservableCollection<Int16>> { }
    public class CollectionConverterInt32 : CollectionConverter<ObservableCollection<Int32>> { }
    public class CollectionConverterInt64 : CollectionConverter<ObservableCollection<Int64>> { }
    public class CollectionConverterDecimal : CollectionConverter<ObservableCollection<Decimal>> { }
    public class CollectionConverterSingle : CollectionConverter<ObservableCollection<Single>> { }
    public class CollectionConverterDouble : CollectionConverter<ObservableCollection<Double>> { }
    #endregion

    [ValueConversion(typeof(object), typeof(object))]
    public class ItemToCollectionConverter<C> : IValueConverter where C : IList, new()
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var array = value != null ? new object[] { value } : null;
            object ret = CollectionHelper.Clone<C>(array, true);
            if (ret == null) return DependencyProperty.UnsetValue;
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }
    public class ItemToCollectionConverter : ItemToCollectionConverter<ObservableCollection<object>> { }

    #region class IsCollectionNullOrEmpty
    public class IsCollectionNullOrEmpty : IValueConverter
    {
        public Boolean Negate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !(value is ICollection))
            {
                return DependencyProperty.UnsetValue;
            }
            bool ret = value == null || ((ICollection)value).Count == 0;

            return this.Negate ? !ret : ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    #endregion
    #region class IsNotCollectionNullOrEmpty
    public class IsNotCollectionNullOrEmpty : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            if (value as ICollection == null) return false;
            int c = ((ICollection)value).Count;
            return !(c == 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
    #region SelectDictionaryEntry
    [ValueConversion(typeof(object), typeof(object))]
    public class SelectDictionaryEntry : IValueConverter, IMultiValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dic = value is IDictionary ? (IDictionary)value : null;
            var key = parameter;
            if (dic == null) { return DependencyProperty.UnsetValue; }
            if (key == null || key == "") { return DependencyProperty.UnsetValue; }

            object ret = dic.Contains(key) ? dic[key] : DependencyProperty.UnsetValue;
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            var dic = values != null && values.Length > i && values[i] is IDictionary ? (IDictionary)values[i] : null; i++;
            var key = values != null && values.Length > i && values[i] is object ? values[i] : null; i++;
            if (key == null || key == "") { key = parameter; }
            if (dic == null) { return DependencyProperty.UnsetValue; }
            if (key == null || key == "") { return DependencyProperty.UnsetValue; }

            object ret = dic.Contains(key) ? dic[key] : DependencyProperty.UnsetValue;
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
        #endregion
    }
    #endregion

    #region ForEachConverter
    public delegate object SelectFromEnumeration(IEnumerable values, Type targetType, object parameter, CultureInfo culture);
    public delegate void SetAsSelectedItem(IEnumerable values, object value, Type targetType, object parameter, CultureInfo culture);

    [ValueConversion(typeof(IEnumerable), typeof(object))]
    public class ForEachConverter : IValueConverter
    {
        #region events
        public event SelectFromEnumeration ConvertEvent;
        public event SetAsSelectedItem ConvertBackEvent;
        #endregion

        private IEnumerable Enumeration;

        #region Convert
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = null;
            if (!(values is IEnumerable)) return ret;
            this.Enumeration = values as IEnumerable;

            if (this.ConvertEvent != null)
            {
                try
                {
                    ret = this.ConvertEvent(this.Enumeration, targetType, parameter, culture);
                }
                catch (Exception ex)
                {
                    // Logger.Exception(ex); ExceptionHelper.OnException(DesktopManager.VistaCorrente.QuadroCorrente, ex);
                }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (this.ConvertBackEvent != null)
                this.ConvertBackEvent(this.Enumeration, value, targetType, parameter, culture);
            return this.Enumeration;
        }
        #endregion
    }
    #endregion
}
