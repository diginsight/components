namespace Diginsight.Components.Configuration;

internal sealed class MetricRecordingFilterConfiguration
{
    public string? MetricName { get; set; }
    public IDictionary<string, bool> ActivityNames { get; set; } = new Dictionary<string, bool>();
}

internal sealed class MetricRecordingEnricherConfiguration
{
    public string? MetricName { get; set; }
    public string[] MetricTags { get; set; } = [];
}