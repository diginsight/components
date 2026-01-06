using Diginsight.Diagnostics;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Diginsight.Components.Configuration;

internal sealed class MetricRecordingDurationMetricTagsEnricherMarker;
internal sealed class MetricRecordingDurationMetricTagsEnricher : IMetricRecordingEnricher
{
    private readonly IOpenTelemetryOptions openTelemetryOptions;
    private readonly IMetricRecordingEnricher metricRecordingEnricher;

    public MetricRecordingDurationMetricTagsEnricher(
        IMetricRecordingEnricher decoratee,
        IOptions<OpenTelemetryOptions> openTelemetryOptions
    )
    {
        this.metricRecordingEnricher = decoratee;
        this.openTelemetryOptions = openTelemetryOptions.Value;
    }

    public IEnumerable<KeyValuePair<string, object?>> ExtractTags(Activity activity, Instrument instrument)
    {
        return openTelemetryOptions.DurationMetricTags
            .Select(k => (Key: k, Value: activity.GetAncestors(true).Select(a => a.GetTagItem(k)).FirstOrDefault(static v => v is not null)))
            .Where(static x => x.Value is not null)
            .Select(static x => KeyValuePair.Create(x.Key, x.Value))
            .Concat(metricRecordingEnricher.ExtractTags(activity, instrument));
    }
}
