using System.ComponentModel;

namespace Diginsight.Components;

public static class INotifyPropertyChangedExtensions
{
    public static NotifyPropertyBinding SetPRopertyBinding(this INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, object[] values, params string[] properties)
    {
        return new NotifyPropertyBinding(source, del, values, properties);
    }

    public static NotifyPropertyTask RunTaskOnProperties(this INotifyPropertyChanged source, INotifyPropertyChangedDelegate2 del, params string[] properties)
    {
        return new NotifyPropertyTask(source, del, properties);
    }
}
