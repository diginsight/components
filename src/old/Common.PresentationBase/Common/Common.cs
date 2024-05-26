#region using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
#endregion

namespace Common
{
    #region Reference<T>
    public class Reference<T> : INotifyPropertyChanged
    {
        #region .ctor
        public Reference() { }
        public Reference(T value) { Value = value; }
        #endregion

        #region Value
        T _value;
        public T Value { get { return _value; } set { UserControlHelper.SetAndNotifyChange(this, this.PropertyChanged, "Value", ref _value, value); } }
        #endregion

        // INotifyPropertyChanged
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void PropertyChangedDelegateImpl(object sender, PropertyChangedEventArgs e) { if (PropertyChanged != null) { PropertyChanged(sender, e); } }
        #endregion
    }
    #endregion
    #region Reference
    public class Reference : Reference<object> { }
    #endregion
    #region ResultReference<T>
    public class ResultReference<T> : INotifyPropertyChanged
    {
        #region .ctor
        public ResultReference() { }
        public ResultReference(T value) { Value = value; }
        #endregion

        #region Value
        T _value;
        public T Value
        {
            get { if (_exception != null) { throw new TargetInvocationException(_exception); } return _value; }
            set { UserControlHelper.SetAndNotifyChange(this, this.PropertyChanged, "Value", ref _value, value); }
        }
        #endregion
        #region Exception
        Exception _exception;
        public Exception Exception { get { return _exception; } set { _exception = value; } }
        #endregion

        // INotifyPropertyChanged
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void PropertyChangedDelegateImpl(object sender, PropertyChangedEventArgs e) { if (PropertyChanged != null) { PropertyChanged(sender, e); } }
        #endregion
    }
    #endregion
    #region ResultReference
    public class ResultReference : ResultReference<object> { }
    #endregion

    #region WeakReference<T> 
    public class ABCWeakReference<T> : WeakReference, INotifyPropertyChanged
    {
        #region .ctor
        public ABCWeakReference(T target) : base(target) { }
        #endregion

        #region Target
        public override object Target
        {
            get { return base.Target; }
            set { base.Target = value; if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("Target")); } }
        }
        #endregion

        #region Value
        public T Value
        {
            get { return base.Target is T ? (T)base.Target : default(T); }
        }
        #endregion

        // INotifyPropertyChanged
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    #endregion
    #region WeakEventHandlerHandler<T>
    public class WeakEventHandlerHandler<T>
        where T : EventArgs
    {
        #region internal state
        WeakReference _targetReference = null;
        MethodInfo _method = null;
        #endregion

        #region .ctor
        public WeakEventHandlerHandler(EventHandler<T> handler)
        {
            _targetReference = new WeakReference(handler.Target);
            _method = handler.Method;
        }
        #endregion

        #region WeakHandler
        public EventHandler<T> WeakHandler
        {
            get
            {
                return delegate (object sender, T e)
                {
                    if (_targetReference == null) { return; }
                    object target = _targetReference.Target;
                    if (target == null) { return; }

                    EventHandler<T> handler = Delegate.CreateDelegate(typeof(EventHandler<T>), target, _method) as EventHandler<T>;
                    if (handler != null) { handler(sender, e); }
                };
            }
        }
        #endregion
    }
    #endregion
    #region AddHandlerInfo<T>
    public class AddHandlerInfo<T>
        where T : EventArgs
    {
        public FrameworkElement FrameworkElement { get; set; }
        public EventHandler<T> WeakHandler { get; set; }
    }
    #endregion

    #region Pair<L, R>
    public class Pair<L, R>
    {
        public L Left { get; set; }
        public R Right { get; set; }
    }
    #endregion

    #region SwitchOnDispose
    public class SwitchOnDispose : IDisposable
    {
        #region internal state
        private Reference<bool> _flag;
        #endregion
        #region .ctor
        public SwitchOnDispose(Reference<bool> flag)
        {
            _flag = flag;
            _flag.Value = !_flag.Value;
        }
        public SwitchOnDispose(Reference<bool> flag, bool init)
        {
            _flag = flag;
            _flag.Value = init;
        }
        #endregion
        #region Detach
        public Reference<bool> Detach()
        {
            Reference<bool> flag = _flag;
            _flag = null;
            return flag;
        }
        #endregion
        #region Dispose
        public void Dispose()
        {
            if (_flag != null) { _flag.Value = !_flag.Value; }
        }
        #endregion
    }
    #endregion

    #region FindParent
    public static class Finder
    {
        public static T FindParent<T>(DependencyObject child, string parentName = "") where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;

            if (parent != null && (string.IsNullOrEmpty(parentName) || (parent as FrameworkElement).Name == parentName))
                return parent;
            else
                return FindParent<T>(parentObject, parentName);
        }

        public static T GetChild<T>(this DependencyObject depObj, string childName = "") where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChild<T>(child, childName);
                if (result != null && (string.IsNullOrEmpty(childName) || (result as FrameworkElement).Name == childName)) return result;
            }
            return null;
        }
    }
    #endregion

    public static class Convert2
    {
        #region ToString(object)
        public static string ToString(object value)
        {
            string ret = "null";
            if (value != null) { ret = Convert.ToString(value); }
            return ret;
        }
        #endregion
        #region ToString(string)
        public static string ToString(string value)
        {
            string ret = "null";
            if (value != null) { ret = Convert.ToString(value); }
            return ret;
        }
        #endregion

        #region ToString(object, IFormatProvider)
        public static string ToString(object value, IFormatProvider provider)
        {
            string ret = "null";
            if (value != null) { ret = Convert.ToString(value, provider); }
            return ret;
        }
        #endregion
        #region ToString(string, IFormatProvider)
        public static string ToString(string value, IFormatProvider provider)
        {
            string ret = "null";
            if (value != null) { ret = Convert.ToString(value, provider); }
            return ret;
        }
        #endregion

        public static short ToInt16(byte[] data)
        {
            var word = BitConverter.IsLittleEndian ? ArrayHelper.SwapBytesInWord(data.Clone() as byte[]) : data;
            return BitConverter.ToInt16(word, 0);
        }

        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null; // could also return string.Empty
        }
    }
}
