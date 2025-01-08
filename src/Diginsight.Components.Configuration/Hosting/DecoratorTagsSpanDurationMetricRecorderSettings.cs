using Diginsight.Diagnostics;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

internal sealed class DecoratedSpanDurationMetricRecorderSettingsMarker;
internal sealed class DecoratorTagsSpanDurationMetricRecorderSettings : ISpanDurationMetricRecorderSettings
{
    private readonly ISpanDurationMetricRecorderSettings decoratee;
    private readonly IOpenTelemetryOptions openTelemetryOptions;

    public DecoratorTagsSpanDurationMetricRecorderSettings(
        ISpanDurationMetricRecorderSettings decoratee,
        IOptions<OpenTelemetryOptions> openTelemetryOptions
    )
    {
        this.decoratee = decoratee;
        this.openTelemetryOptions = openTelemetryOptions.Value;
    }

    public bool? ShouldRecord(Activity activity)
    {
        return decoratee.ShouldRecord(activity);
    }

    public IEnumerable<KeyValuePair<string, object?>> ExtractTags(Activity activity)
    {
        return openTelemetryOptions.DurationMetricTags
            .Select(k => (Key: k, Value: activity.GetAncestors(true).Select(a => a.GetTagItem(k)).FirstOrDefault(static v => v is not null)))
            .Where(static x => x.Value is not null)
            .Select(static x => KeyValuePair.Create(x.Key, x.Value))
            .Concat(decoratee.ExtractTags(activity));
    }
}
