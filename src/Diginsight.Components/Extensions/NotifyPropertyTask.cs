using System.ComponentModel;

namespace Diginsight.Components;

public class NotifyPropertyTask : IDisposable
{
    public NotifyPropertyTask(INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, params string[] properties)
    {
        Source = source;
        Delegate = del;
        Properties = properties;
        Name = string.Empty;
        Description = string.Empty;
        source.PropertyChanged += source_PropertyChanged;
    }

    private string Name { get; set; }
    private string Description { get; set; }
    private INotifyPropertyChanged? Source { get; set; }
    private INotifyPropertyChangedDelegate2? Delegate { get; set; }
    private string[] Properties { get; set; }

    public void source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var propertyName = e.PropertyName;
        if (Source is null || Delegate is null || (!(Properties?.Contains(propertyName) ?? false)))
            return;

        var propertyValues = new List<object>();
        var sourceType = Source.GetType(); //.GetProperty(propertyName).GetValue(car, null); ;
        Properties?.ToList().ForEach(name =>
        {
            var propertyInfo = sourceType.GetProperty(name);
            var propertyValue = propertyInfo?.GetValue(Source, null);
            if (propertyValue is not null) { propertyValues.Add(propertyValue); }
        });

        // if ()
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
