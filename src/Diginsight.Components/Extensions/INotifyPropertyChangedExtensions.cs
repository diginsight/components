#nullable enable

using System.ComponentModel;
using System.Globalization;

namespace Diginsight.Components
{
    public delegate object? INotifyPropertyChangedDelegate2(object source, object[] values, Type? targetType, object? parameter, CultureInfo? culture);

    public class NotifyPropertyBinding : IDisposable
    {
        #region .ctor
        public NotifyPropertyBinding(INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, object[]? values, params string[]? properties)
        {
            this.Source = source;
            this.Delegate = del;
            this.Properties = properties;
            this.Values = values;
            this.Name = string.Empty;
            this.Description = string.Empty;
            source.PropertyChanged += this.source_PropertyChanged;
            source_PropertyChanged(this, new PropertyChangedEventArgs(""));
        }
        #endregion

        INotifyPropertyChanged? Source { get; set; }
        INotifyPropertyChangedDelegate2? Delegate { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        object[]? Values { get; set; }
        string[]? Properties { get; set; }

        public void source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var propertyName = e.PropertyName;
            if (this.Delegate != null && (string.IsNullOrEmpty(propertyName) || (this.Properties?.Contains(propertyName) ?? false)))
            {
                var propertyValues = Values != null ? new List<object>(Values) : new List<object>();
                var sourceType = this.Source?.GetType(); //.GetProperty(propertyName).GetValue(car, null); ;
                this.Properties?.ToList().ForEach((name) =>
                {
                    // var propertyValue = source[name]; // Reflection
                    var propertyInfo = sourceType?.GetProperty(name);
                    var propertyValue = propertyInfo?.GetValue(this.Source, null);
                    if (propertyValue != null) { propertyValues.Add(propertyValue); }
                });
                this.Delegate(this, propertyValues.ToArray(), null, null, null);
            }
        }
        public void Dispose()
        {
            if (this.Source != null) { this.Source.PropertyChanged -= source_PropertyChanged; this.Source = null; };
        }
    }

    public class NotifyPropertyTask : IDisposable
    {
        #region .ctor
        public NotifyPropertyTask(INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, params string[] properties)
        {
            this.Source = source;
            this.Delegate = del;
            this.Properties = properties;
            this.Name = string.Empty;
            this.Description = string.Empty;
            source.PropertyChanged += this.source_PropertyChanged;
        }
        #endregion

        string Name { get; set; }
        string Description { get; set; }
        INotifyPropertyChanged? Source { get; set; }
        INotifyPropertyChangedDelegate2? Delegate { get; set; }
        string[] Properties { get; set; }

        public void source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var propertyName = e.PropertyName;
            if (this.Source != null && this.Delegate != null && (this.Properties?.Contains(propertyName) ?? false))
            {
                var propertyValues = new List<object>();
                var sourceType = this.Source.GetType(); //.GetProperty(propertyName).GetValue(car, null); ;
                this.Properties?.ToList().ForEach((name) =>
                {
                    var propertyInfo = sourceType.GetProperty(name);
                    var propertyValue = propertyInfo?.GetValue(this.Source, null);
                    if (propertyValue != null) { propertyValues.Add(propertyValue); }
                });

                // if ()
                this.Delegate(this, propertyValues.ToArray(), null, null, null);
            }
        }
        public void Dispose()
        {
            if (this.Source != null) { this.Source.PropertyChanged -= source_PropertyChanged; this.Source = null; };
        }
    }

    public static class INotifyPropertyChangedExtensions
    {
        public static NotifyPropertyBinding SetPRopertyBinding(this INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, object[] values, params string[] properties)
        {
            var binding = new NotifyPropertyBinding(source, del, values, properties);
            return binding;
        }

        public static NotifyPropertyTask RunTaskOnProperties(this INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, params string[] properties)
        {
            var task = new NotifyPropertyTask(source, del, properties);
            return task;
        }
    }
}
