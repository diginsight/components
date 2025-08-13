using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Diginsight.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Diginsight.Components.Azure.Metrics;

public sealed class QueryCostMetricRecorder : IActivityListenerLogic
{
    private readonly ILogger logger;
    private readonly Histogram<double> queryCostMetric;
    private readonly IMetricRecordingFilter? metricFilter;
    private readonly IMetricRecordingEnricher? metricEnricher;

    private readonly Lazy<Histogram<double>> lazyMetric;
    private Histogram<double> Metric => lazyMetric.Value;


    public QueryCostMetricRecorder(
        IServiceProvider serviceProvider,
        ILogger<QueryCostMetricRecorder> logger,
        IClassAwareOptionsMonitor<IOpenTelemetryOptions> openTelemetryOptionsMonitor,
        IMeterFactory meterFactory
    )
    {
        this.logger = logger;
        this.queryCostMetric = QueryMetrics.QueryCost;

        IOpenTelemetryOptions openTelemetryOptions = openTelemetryOptionsMonitor.CurrentValue;
        var applicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
        var metricName = this.queryCostMetric.Name;

        this.lazyMetric = new Lazy<Histogram<double>>(() => {
            return meterFactory.Create(applicationName)
                               .CreateHistogram<double>(QueryMetrics.QueryCost.Name, "RU", $"{applicationName} metric");
        });

        var metricFilter = serviceProvider.GetNamedService<IMetricRecordingFilter>(metricName);
        this.metricFilter = metricFilter ?? serviceProvider.GetRequiredService<IMetricRecordingFilter>();

        var metricEnricher = serviceProvider.GetNamedService<IMetricRecordingEnricher>(metricName);
        this.metricEnricher = metricEnricher ?? serviceProvider.GetRequiredService<IMetricRecordingEnricher>();
    }

    void IActivityListenerLogic.ActivityStopped(Activity activity)
    {
        try
        {
            // Only record for CosmosDB operations that have a query cost
            if (activity.GetTagItem("query_cost") is object costObj &&
                double.TryParse(costObj.ToString(), out double cost) &&
                cost > 0)
            {
                var tags = new TagList
                {
                    //{ "trace_id", activity.TraceId.ToString() ?? "unknown" },
                    { "query", activity.GetTagItem("query")?.ToString() ?? "unknown" },
                    { "method", activity.OperationName },
                    { "entrymethod", GetEntryMethod(activity) },
                    { "application", activity.GetTagItem("application")?.ToString() ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown" },
                    { "container", activity.GetTagItem("container")?.ToString() ?? "unknown" },
                    { "database", activity.GetTagItem("database")?.ToString() ?? "unknown" }
                };
                if (metricEnricher is not null)
                {
                    var additionalTags = metricEnricher.ExtractTags(activity);
                    foreach (var tag in additionalTags ?? [])
                    {
                        tags.Add(tag.Key, tag.Value);
                    }
                }

                queryCostMetric.Record(cost, tags);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unhandled exception while recording query cost metric for activity {ActivityName}", activity.OperationName);
        }
    }

    private static string GetEntryMethod(Activity activity)
    {
        // Walk up the activity chain to find the entry method
        var current = activity;
        while (current.Parent != null)
        {
            current = current.Parent;
        }
        return current.OperationName;
    }

    ActivitySamplingResult IActivityListenerLogic.Sample(ref ActivityCreationOptions<ActivityContext> creationOptions) => ActivitySamplingResult.AllData;
}
