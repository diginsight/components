using System.ComponentModel;

namespace Diginsight.Components;

public class NotifyPropertyBinding : IDisposable
{
    public NotifyPropertyBinding(INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, object[]? values, params string[]? properties)
    {
        Source = source;
        Delegate = del;
        Properties = properties;
        Values = values;
        Name = string.Empty;
        Description = string.Empty;
        source.PropertyChanged += source_PropertyChanged;
        source_PropertyChanged(this, new PropertyChangedEventArgs(""));
    }

    private INotifyPropertyChanged? Source { get; set; }
    private INotifyPropertyChangedDelegate2? Delegate { get; set; }
    private string Name { get; set; }
    private string Description { get; set; }
    private object[]? Values { get; set; }
    private string[]? Properties { get; set; }

    public void source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var propertyName = e.PropertyName;
        if (Delegate is null || (!string.IsNullOrEmpty(propertyName) && (!(Properties?.Contains(propertyName) ?? false))))
            return;

        var propertyValues = Values is not null ? new List<object>(Values) : new List<object>();
        var sourceType = Source?.GetType(); //.GetProperty(propertyName).GetValue(car, null); ;
        Properties?.ToList().ForEach(name =>
        {
            // var propertyValue = source[name]; // Reflection
            var propertyInfo = sourceType?.GetProperty(name);
            var propertyValue = propertyInfo?.GetValue(Source, null);
            if (propertyValue is not null) { propertyValues.Add(propertyValue); }
        });
        Delegate(this, propertyValues.ToArray(), null, null, null);
    }

    public void Dispose()
    {
        if (Source is not null)
        {
            Source.PropertyChanged -= source_PropertyChanged;
            Source = null;
        }
    }
}
