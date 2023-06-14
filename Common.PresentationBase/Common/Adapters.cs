#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; 
#endregion

namespace Common
{
    public class ObjectItemAdapter
    {
        #region class declarations
        public class ItemsCollection : List<object> { }
        #endregion
        #region constants
        const string CONFIGVALUE_EXCLUDEDTYPESPATTERN = "ExcludedTypes"; const string DEFAULTVALUE_EXCLUDEDTYPESPATTERN = "String";
        #endregion
        #region internal state
        string _fieldName;
        Type _type;
        object _value;
        static string _excludedTypesPattern;
        static Regex _excludedTypesRegularExpression;
        #endregion

        #region .ctor
        static ObjectItemAdapter()
        {
            _excludedTypesPattern = ConfigurationHelper.GetClassSetting<ObjectItemAdapter, string>(CONFIGVALUE_EXCLUDEDTYPESPATTERN, DEFAULTVALUE_EXCLUDEDTYPESPATTERN); // , CultureInfo.InvariantCulture
            if (!StringHelper.IsEmpty(_excludedTypesPattern)) { _excludedTypesRegularExpression = new Regex(_excludedTypesPattern); } // try { } catch { }
        }
        public ObjectItemAdapter() { }
        public ObjectItemAdapter(string fieldName, Type type, object value)
        {
            if (fieldName == null) { throw new ArgumentNullException("fieldName", "fieldName non può essere null"); }
            if (type == null) { throw new ArgumentNullException("type", "type non può essere null"); }
            _fieldName = fieldName; _type = type;
            _value = value;
        }
        #endregion

        #region FieldName
        public string FieldName { get { return _fieldName; } }
        #endregion
        #region FieldType
        public Type FieldType { get { return _type; } }
        #endregion
        #region FieldValue
        public object FieldValue { get { return _value; } }
        #endregion

        #region TypeName
        public string TypeName { get { return _type.Name; } }
        #endregion
        #region Description
        public string Description
        {
            get
            {
                return Convert2.ToString(_value);
            }
        }
        #endregion

        #region Children
        public object[] Children
        {
            get
            {
                if (_value == null) { return null; }
                Type type = _value.GetType();
                List<object> children = new List<object>();

                if (!(_value is ItemsCollection))
                {
                    TypeDescriptionProvider provider = TypeDescriptor.GetProvider(_value);
                    ICustomTypeDescriptor td = provider.GetTypeDescriptor(_value);
                    PropertyDescriptorCollection pdc = td.GetProperties();

                    foreach (PropertyDescriptor pd in pdc)
                    {
                        Type t = pd.GetType();
                        object value = pd.GetValue(_value);

                        children.Add(new ObjectItemAdapter(pd.Name, t, value));
                        //if (t.IsSubclassOf(typeof(Exception))) {
                        //    children.Add(new ObjectItemAdapter(pd.Name, t, value));
                        //} else {
                        //    children.Add(value);
                        //}
                    }

                    if (_value is IEnumerable)
                    {
                        bool bExclude = false;
                        if (_excludedTypesRegularExpression != null) { if (_excludedTypesRegularExpression.IsMatch(type.Name)) { bExclude = true; } }

                        if (!bExclude)
                        {
                            IEnumerable oEnum = _value as IEnumerable;
                            ItemsCollection items = new ItemsCollection();
                            foreach (object item in oEnum) { items.Add(item); }

                            children.Add(new ObjectItemAdapter("Items", typeof(ItemsCollection), items));
                        }
                    }
                }
                else
                {
                    IEnumerable oEnum = _value as IEnumerable;
                    foreach (object item in oEnum)
                    {
                        if (item != null)
                        {
                            children.Add(new ObjectItemAdapter("item", item.GetType(), item));
                        }
                        else
                        {
                            children.Add(new ObjectItemAdapter("item", null, item));
                        }
                    }
                }

                return children.ToArray();
            }
        }
        #endregion
    }

    public class PropertyDescriptorItemAdapter
    {
        #region internal state
        PropertyDescriptor _pd;
        object _value;
        #endregion

        #region .ctor
        public PropertyDescriptorItemAdapter(PropertyDescriptor pd, object value)
        {
            if (pd == null) { throw new ArgumentNullException("pd", "pd parameter cannot be null"); }
            _pd = pd;
            _value = value;
        }
        #endregion

        #region FieldName
        public string FieldName { get { return _pd.Name; } }
        #endregion
        #region TypeName
        public string TypeName { get { return _pd.PropertyType.Name; } }
        #endregion
        #region Children
        public object[] Children
        {
            get
            {
                if (_value == null) { return null; }

                TypeDescriptionProvider provider = TypeDescriptor.GetProvider(_value);
                ICustomTypeDescriptor td = provider.GetTypeDescriptor(_value);
                PropertyDescriptorCollection pdc = td.GetProperties();
                List<object> children = new List<object>();

                foreach (PropertyDescriptor pd in pdc)
                {
                    Type t = pd.GetType();
                    object value = pd.GetValue(_value);

                    if (t.IsSubclassOf(typeof(Exception)))
                    {
                        children.Add(new PropertyDescriptorItemAdapter(_pd, value));
                    }
                    else
                    {
                        children.Add(value);
                    }
                }
                return children.ToArray();
            }
        }
        #endregion
    }

    public class ObjectItemAdapterTemplateSelector : DataTemplateSelector
    {
        public string ShortItemTemplateResource { get; set; }
        public string LongItemTemplateResource { get; set; }
        public object ShortItemTemplate { get; set; }
        public object LongItemTemplate { get; set; }
        public int Len { get; set; }
        DataTemplate _shortDataTemplate, _longDataTemplate;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is ObjectItemAdapter)
            {
                Window window = UserControlHelper.FindOwningWindow(container as FrameworkElement);
                if (_shortDataTemplate == null && window != null && ShortItemTemplateResource != null) { _shortDataTemplate = window.FindResource(ShortItemTemplateResource) as DataTemplate; }
                if (_longDataTemplate == null && window != null && LongItemTemplateResource != null) { _longDataTemplate = window.FindResource(LongItemTemplateResource) as DataTemplate; }

                if (_shortDataTemplate == null) { if (ShortItemTemplate != null) { _shortDataTemplate = ShortItemTemplate as DataTemplate; } }
                if (_longDataTemplate == null) { if (LongItemTemplate != null) { _longDataTemplate = LongItemTemplate as DataTemplate; } }

                ObjectItemAdapter adapter = item as ObjectItemAdapter;
                if (adapter.Description != null && adapter.Description.Length >= Len)
                {
                    return _longDataTemplate;
                }
                else
                {
                    return _shortDataTemplate;
                }
            }
            return null;
        }
    }

    public class ObjectItemAdapterListTemplateSelector : DataTemplateSelector
    {
        public string ShortItemTemplateResource { get; set; }
        public string LongItemTemplateResource { get; set; }
        public object ShortItemTemplate { get; set; }
        public object LongItemTemplate { get; set; }
        // public int Len { get; set; }
        DataTemplate _shortDataTemplate, _longDataTemplate;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is ObjectItemAdapter)
            {
                ObjectItemAdapter itemAdapter = item as ObjectItemAdapter;

                Window window = UserControlHelper.FindOwningWindow(container as FrameworkElement);
                if (_shortDataTemplate == null && window != null && ShortItemTemplateResource != null) { _shortDataTemplate = window.FindResource(ShortItemTemplateResource) as DataTemplate; }
                if (_longDataTemplate == null && window != null && LongItemTemplateResource != null) { _longDataTemplate = window.FindResource(LongItemTemplateResource) as DataTemplate; }

                if (_shortDataTemplate == null && ShortItemTemplate != null) { _shortDataTemplate = ShortItemTemplate as DataTemplate; }
                if (_longDataTemplate == null && LongItemTemplate != null) { _longDataTemplate = LongItemTemplate as DataTemplate; }

                ObjectItemAdapter adapter = item as ObjectItemAdapter;
                if (itemAdapter.FieldValue != null && itemAdapter.FieldValue is Exception && (itemAdapter.FieldValue as Exception).InnerException != null)
                {
                    return _longDataTemplate;
                }
                else
                {
                    return _shortDataTemplate;
                }
            }
            return null;
        }
    }

}
