using Azure;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Diginsight.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Diginsight.Components.Azure.Metrics;

public sealed class QueryCostMetricRecorder : IActivityListenerLogic
{
    private readonly ILogger logger;
    //private readonly Histogram<double> queryCostMetric;
    private readonly IMetricRecordingFilter? metricFilter;
    private readonly IMetricRecordingEnricher? metricEnricher;

    private readonly Lazy<Histogram<double>> lazyMetric;
    private Histogram<double> Metric => lazyMetric.Value;


    public QueryCostMetricRecorder(
        IServiceProvider serviceProvider,
        ILogger<QueryCostMetricRecorder> logger,
        IClassAwareOptionsMonitor<DiginsightActivitiesOptions> activitiesOptionsMonitor,
        IClassAwareOptionsMonitor<OpenTelemetryOptions> openTelemetryOptionsMonitor,
        IMeterFactory meterFactory
    )
    {
        this.logger = logger;
        //this.queryCostMetric = QueryMetrics.QueryCost;

        IOpenTelemetryOptions openTelemetryOptions = openTelemetryOptionsMonitor.CurrentValue;
        var applicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
        var metricName = QueryMetrics.QueryCost.Name;

        this.lazyMetric = new Lazy<Histogram<double>>(() =>
        {
            IDiginsightActivitiesMetricOptions options = activitiesOptionsMonitor.CurrentValue;
            return meterFactory.Create(options.MeterName)
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
                var callers = GetDiginsightCallers(activity);
                var entryMethod = callers.Last();
                var diginsightCallers = callers.Where(a=> !a.OperationName.Contains("diginsight", StringComparison.InvariantCultureIgnoreCase));
                var caller1 = diginsightCallers.FirstOrDefault();
                var caller2 = diginsightCallers.Skip(1).FirstOrDefault();

                var tags = new List<KeyValuePair<string, object?>>();
                //tags.Add(new KeyValuePair<string, object?>("query", activity.GetTagItem("query")?.ToString()));
                tags.Add(new KeyValuePair<string, object?>("method", activity.OperationName));
                tags.Add(new KeyValuePair<string, object?>("caller1", caller1));
                tags.Add(new KeyValuePair<string, object?>("caller2", caller2));
                tags.Add(new KeyValuePair<string, object?>("entrymethod", entryMethod));
                tags.Add(new KeyValuePair<string, object?>("application", activity.GetTagItem("application")?.ToString() ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name));
                tags.Add(new KeyValuePair<string, object?>("container", activity.GetTagItem("container")?.ToString()));
                tags.Add(new KeyValuePair<string, object?>("database", activity.GetTagItem("database")?.ToString()));
                if (metricEnricher is not null)
                {
                    var additionalTags = metricEnricher.ExtractTags(activity);
                    foreach (var tag in additionalTags ?? [])
                    {
                        tags.Add(new KeyValuePair<string, object?>(tag.Key, tag.Value));
                    }
                }

                var tagsArray = tags.ToArray();
                Metric.Record(cost, tagsArray);
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
        while (current.Parent != null) { current = current.Parent; }
        return current.OperationName;
    }

    private static Activity[] GetDiginsightCallers(Activity activity)
    {
        var callers = new List<Activity>();
        var current = activity;
        while (current.Parent != null)
        {
            current = current.Parent;
            callers.Add(current);
        }

        return callers.ToArray();
    }


    ActivitySamplingResult IActivityListenerLogic.Sample(ref ActivityCreationOptions<ActivityContext> creationOptions) => ActivitySamplingResult.AllData;
}
