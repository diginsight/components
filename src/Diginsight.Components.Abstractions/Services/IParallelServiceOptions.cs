namespace Diginsight.Components;

public interface IParallelServiceOptions
{
    int MaxConcurrency { get; set; }
    int LowConcurrency { get; set; }
    int MediumConcurrency { get; set; }
    int HighConcurrency { get; set; }
}