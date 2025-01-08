using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diginsight.Components.Configuration;

public interface IOpenTelemetryOptions
{
    string AzureMonitorConnectionString { get; }

    bool EnableTraces { get; }
    bool EnableMetrics { get; }

    double TracingSamplingRatio { get; }

    IEnumerable<string> ActivitySources { get; }
    IEnumerable<string> Meters { get; }

    IEnumerable<string> ExcludedHttpHosts { get; }

    IEnumerable<string> DurationMetricTags { get; }
}
