using Diginsight.Options;

namespace Diginsight.Components.Configuration;

public class ConcurrencyOptions : IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public int? MaxConcurrency { get; set; }
}

