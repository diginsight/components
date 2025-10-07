using Diginsight.Options;

namespace Diginsight.Components;

public sealed class ParallelServiceOptions : IParallelServiceOptions, IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public int MaxConcurrency { get; set; }
    public int LowConcurrency { get; set; }
    public int MediumConcurrency { get; set; }
    public int HighConcurrency { get; set; }
}