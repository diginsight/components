using Diginsight.Diagnostics;

namespace Diginsight.Components.Azure.Metrics;

public interface IQueryCostMetricRecorderOptions : IMetricRecordingOptions
{
    bool AddNormalizedQueryTag { get; }
    int NormalizedQueryMaxLen { get; }
    int AddQueryCallers { get; }
    IEnumerable<string> IgnoreQueryCallers { get; }
    string MetricUnit { get; }
}
