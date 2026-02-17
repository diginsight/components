using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

public sealed class OpenTelemetryOptions : IOpenTelemetryOptions
{
    public string AzureMonitorConnectionString { get; set; }
    public bool EnableTraces { get; set; }
    public bool EnableMetrics { get; set; }
    public double TracingSamplingRatio { get; set; }
    public bool EnablePayloadTagging { get; set; }

    public ICollection<string> ActivitySources { get; } = new List<string>();
    IEnumerable<string> IOpenTelemetryOptions.ActivitySources => ActivitySources;

    public ICollection<string> Meters { get; } = new List<string>();
    IEnumerable<string> IOpenTelemetryOptions.Meters => Meters;

    public ICollection<string> ExcludedHttpHosts { get; } = new List<string>();
    IEnumerable<string> IOpenTelemetryOptions.ExcludedHttpHosts => ExcludedHttpHosts;

    public ICollection<string> DurationMetricTags { get; } = new List<string>();
    IEnumerable<string> IOpenTelemetryOptions.DurationMetricTags => DurationMetricTags;
}
