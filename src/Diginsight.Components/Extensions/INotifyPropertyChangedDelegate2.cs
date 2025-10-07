using System.Globalization;

namespace Diginsight.Components;

public delegate object? INotifyPropertyChangedDelegate2(object source, object[] values, Type? targetType, object? parameter, CultureInfo? culture);