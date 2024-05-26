#region using
using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
#endregion

namespace Common.Core.Blazor
{
    public class LayoutComponentBaseCore : LayoutComponentBase, INotifyPropertyChanged
    {
        #region PropertyChanged
        public IDictionary Properties { get; } = new Dictionary<string, object>();
        #region SetValue
        protected void SetValue<T>(Expression<Func<T>> propertySelector, T value, PropertyChangedEventHandler propertyChanged = null)
        {
            string propertyName = GetPropertyName(propertySelector);
            var oldValue = Properties[propertyName];
            Properties[propertyName] = value;
            if (!object.Equals(oldValue, value))
            {
                NotifyPropertyChanged(propertySelector);
                if (propertyChanged != null)
                {
                    var arg = new PropertyChangedEventArgs(propertyName);
                    propertyChanged?.Invoke(this, arg);
                }
            }
        }
        #endregion
        #region SetValue
        protected void SetValue<T>(string propertyName, T value, PropertyChangedEventHandler propertyChanged = null)
        {
            var oldValue = Properties[propertyName];
            Properties[propertyName] = value;
            if (!object.Equals(oldValue, value))
            {
                NotifyPropertyChanged(propertyName);
                if (propertyChanged != null)
                {
                    var arg = new PropertyChangedEventArgs(propertyName);
                    propertyChanged?.Invoke(this, arg);
                }
            }
        }
        #endregion
        #region GetValue
        protected T GetValue<T>(Expression<Func<T>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);
            return GetValue<T>(propertyName);
        }
        #endregion
        #region GetValue
        protected T GetValue<T>(string propertyName)
        {
            object value;
            if (Properties.Contains(propertyName))
            {
                value = Properties[propertyName];
                return (T)value;
            }
            return default(T);
        }
        #endregion
        #region NotifyPropertyChanged
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                string propertyName = GetPropertyName(propertySelector);
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        #region INotifyPropertyChanged Members
        /// <summary>Raised when a property on this object has a new value.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>Raises this object's PropertyChanged event.</summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        #endregion // INotifyPropertyChanged Members
        #region GetPropertyName
        private string GetPropertyName(LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) { throw new InvalidOperationException(); }
            return memberExpression.Member.Name;
        }
        #endregion

        #region Debugging
        /// <summary>Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.</summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real, public, instance property on this object.
            if (this.GetType().GetProperty(propertyName) == null)
            {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Assert(false, msg);
            }
        }
        /// <summary>Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.</summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
        #endregion
        #endregion
    }
}
