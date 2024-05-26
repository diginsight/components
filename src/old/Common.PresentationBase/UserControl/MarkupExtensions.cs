#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
#endregion

namespace Common
{
    #region IntegralExtensionBase
    public class ExtensionBase<T> : MarkupExtension
    {
        T _value;
        public ExtensionBase(T value) { _value = value; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _value;
        }
    }
    #endregion

    #region BooleanExtension
    public class BooleanExtension : ExtensionBase<bool>
    {
        public BooleanExtension() : base(default(bool)) { }
        public BooleanExtension(bool value) : base(value) { }
    }
    #endregion
    #region Int32Extension
    public class Int32Extension : ExtensionBase<int>
    {
        public Int32Extension() : base(default(int)) { }
        public Int32Extension(int value) : base(value) { }
    }
    #endregion
    #region Int16Extension
    public class Int16Extension : ExtensionBase<short>
    {
        public Int16Extension() : base(default(short)) { }
        public Int16Extension(short value) : base(value) { }
    }
    #endregion
    #region Int64Extension
    public class Int64Extension : ExtensionBase<long>
    {
        public Int64Extension() : base(default(long)) { }
        public Int64Extension(long value) : base(value) { }
    }
    #endregion
    #region DecimalExtension
    public class DecimalExtension : ExtensionBase<decimal>
    {
        public DecimalExtension() : base(default(decimal)) { }
        public DecimalExtension(decimal value) : base(value) { }
    }
    #endregion
    #region SingleExtension
    public class SingleExtension : ExtensionBase<float>
    {
        public SingleExtension() : base(default(float)) { }
        public SingleExtension(float value) : base(value) { }
    }
    #endregion
    #region DoubleExtension
    public class DoubleExtension : ExtensionBase<double>
    {
        public DoubleExtension() : base(default(double)) { }
        public DoubleExtension(double value) : base(value) { }
    }
    #endregion

    #region ObjectExtension
    public class ObjectExtension : ExtensionBase<object>
    {
        public ObjectExtension() : base(null) { }
        public ObjectExtension(object value) : base(value) { }
    }
    #endregion

    #region DateTimeExtension
    public class DateTimeExtension : ExtensionBase<DateTime>
    {
        public DateTimeExtension() : base(default(DateTime)) { }
        public DateTimeExtension(DateTime value) : base(value) { }
    }
    #endregion

    public static class ABCVisualTreeHelper
    {
        public static Visual GetDescendantByName(Visual element, string name)
        {
            if (element == null) return null;
            if (element is FrameworkElement && (element as FrameworkElement).Name == name) return element;

            Visual result = null;
            if (element is FrameworkElement) (element as FrameworkElement).ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                result = GetDescendantByName(visual, name);
                if (result != null) break;
            }
            return result;
        }

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;

            Visual foundElement = null;
            if (element is FrameworkElement) (element as FrameworkElement).ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }


        public static DependencyObject GetAncestorByType(DependencyObject element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            return GetAncestorByType(VisualTreeHelper.GetParent(element), type);
        }
    }

}
