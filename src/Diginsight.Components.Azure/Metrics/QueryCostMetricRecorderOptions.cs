using Diginsight.Diagnostics;
using Diginsight.Options;

namespace Diginsight.Components.Azure.Metrics;

public sealed class QueryCostMetricRecorderOptions
    : IQueryCostMetricRecorderOptions,
        IDynamicallyConfigurable,
        IVolatilelyConfigurable
{
    public bool AddNormalizedQueryTag { get; set; }

    public int? NormalizedQueryMaxLen { get; set; }

    int IQueryCostMetricRecorderOptions.NormalizedQueryMaxLen => NormalizedQueryMaxLen ?? 500;

    public int? AddQueryCallers { get; set; }

    int IQueryCostMetricRecorderOptions.AddQueryCallers => AddQueryCallers ?? 0;

    public ICollection<string> IgnoreQueryCallers { get; } = [ ];

    IEnumerable<string> IQueryCostMetricRecorderOptions.IgnoreQueryCallers => IgnoreQueryCallers;

    public bool Record { get; set; }

    public string? MeterName { get; set; }

    string IMetricRecordingOptions.MeterName => MeterName ?? throw new InvalidOperationException($"{nameof(MeterName)} is unset");

    public string? MetricName { get; set; }

    string IMetricRecordingOptions.MetricName => MetricName ?? "diginsight.query_cost";

    public string? MetricUnit { get; set; }

    string IQueryCostMetricRecorderOptions.MetricUnit => MetricUnit ?? throw new InvalidOperationException($"{nameof(MetricUnit)} is unset");

    public string? MetricDescription { get; set; }
}
