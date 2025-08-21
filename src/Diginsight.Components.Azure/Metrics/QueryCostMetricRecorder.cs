using Azure;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Diginsight.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Diginsight.Components.Azure.Metrics;

public class QueryCostMetricRecorderOptions : IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public bool AddNormalizedQueryTag { get; set; } = false;
    public int NormalizedQueryMaxLen { get; set; } = 500;
    public int AddQueryCallers { get; set; } = 0;
}

public sealed class QueryCostMetricRecorder : IActivityListenerLogic
{
    private static readonly Regex GuidPattern = new Regex(@"\b[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}\b", RegexOptions.Compiled);
    private static readonly Regex NumberPattern = new Regex(@"\b\d{4,}\b", RegexOptions.Compiled); // Numbers with 4 or more digits (likely IDs, timestamps, etc.)
    private static readonly Regex StringLiteralPattern = new Regex(@"'[^']{8,}'", RegexOptions.Compiled); // String literals longer than 8 characters
    private static readonly Regex DateTimePattern = new Regex(@"\b\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?(?:Z|[+-]\d{2}:\d{2})?\b", RegexOptions.Compiled);
    // private readonly Histogram<double> queryCostMetric;

    private readonly ILogger logger;
    private readonly IClassAwareOptionsMonitor<QueryCostMetricRecorderOptions> queryCostMetricRecorderOptions;
    private readonly IMetricRecordingFilter? metricFilter;
    private readonly IMetricRecordingEnricher? metricEnricher;

    private readonly Lazy<Histogram<double>> lazyMetric;
    private Histogram<double> Metric => lazyMetric.Value;


    public QueryCostMetricRecorder(
        IServiceProvider serviceProvider,
        ILogger<QueryCostMetricRecorder> logger,
        IClassAwareOptionsMonitor<DiginsightActivitiesOptions> activitiesOptionsMonitor,
        IClassAwareOptionsMonitor<OpenTelemetryOptions> openTelemetryOptionsMonitor,
        IClassAwareOptionsMonitor<QueryCostMetricRecorderOptions> queryCostMetricRecorderOptions,
        IMeterFactory meterFactory
    )
    {
        this.logger = logger;
        this.queryCostMetricRecorderOptions = queryCostMetricRecorderOptions;

        IOpenTelemetryOptions openTelemetryOptions = openTelemetryOptionsMonitor.CurrentValue;
        var applicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
        var metricName = QueryMetrics.QueryCost.Name;

        this.lazyMetric = new Lazy<Histogram<double>>(() =>
        {
            IDiginsightActivitiesMetricOptions options = activitiesOptionsMonitor.CurrentValue;
            return meterFactory.Create(options.MeterName)
                               .CreateHistogram<double>(QueryMetrics.QueryCost.Name, QueryMetrics.QueryCost.Unit, QueryMetrics.QueryCost.Description);
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

                //var caller1 = diginsightCallers.FirstOrDefault();
                //var caller2 = diginsightCallers.Skip(1).FirstOrDefault();
                //var caller3 = diginsightCallers.Skip(2).FirstOrDefault();

                var queryCostMetricRecorderOptionsValue = queryCostMetricRecorderOptions.CurrentValue;
                var addNormalizedQueryTag = queryCostMetricRecorderOptionsValue.AddNormalizedQueryTag;
                var addQueryCallers = queryCostMetricRecorderOptionsValue.AddQueryCallers;

                var tags = new List<KeyValuePair<string, object?>>();
                if (addNormalizedQueryTag)
                {
                    var rawQuery = activity.GetTagItem("query")?.ToString();
                    var normalizedQuery = NormalizeQueryForMetrics(rawQuery);
                    tags.Add(new KeyValuePair<string, object?>("query", normalizedQuery));
                }
                if (addQueryCallers != 0)
                {
                    var diginsightCallers = callers.Where(a => !a.OperationName.Contains("diginsight", StringComparison.InvariantCultureIgnoreCase));

                    var callerIndex = 0;
                    while (callerIndex < addQueryCallers && callerIndex < diginsightCallers.Count())
                    {
                        var caller = diginsightCallers.ElementAt(callerIndex);
                        tags.Add(new KeyValuePair<string, object?>($"caller{callerIndex + 1}", caller.OperationName));
                        callerIndex++;
                    }
                }
                tags.Add(new KeyValuePair<string, object?>("method", activity.OperationName));
                tags.Add(new KeyValuePair<string, object?>("entrymethod", entryMethod?.OperationName));
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

    /// <summary>
    /// Determines if the input string appears to be JSON-encoded by checking for JSON object structure.
    /// </summary>
    /// <param name="input">The input string to check</param>
    /// <returns>True if the string appears to be JSON, false otherwise</returns>
    private static bool IsJsonEncoded(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        input = input.Trim();
        return (input.StartsWith('{') && input.EndsWith('}') && input.Contains('"')) ||
               (input.StartsWith('[') && input.EndsWith(']') && input.Contains('"'));
    }

    /// <summary>
    /// Extracts the actual query string from JSON-encoded query data.
    /// Handles input like: "{\"query\":\"SELECT VALUE root FROM root WHERE true\"}"
    /// </summary>
    /// <param name="jsonEncodedQuery">The JSON-encoded query string</param>
    /// <returns>The extracted query string or null if extraction fails</returns>
    private static string? ExtractQueryFromJson(string? jsonEncodedQuery)
    {
        if (string.IsNullOrEmpty(jsonEncodedQuery))
            return jsonEncodedQuery;

        try
        {
            // First, try to parse as JSON object with "query" property
            using var document = JsonDocument.Parse(jsonEncodedQuery);
            if (document.RootElement.TryGetProperty("query", out var queryElement))
            {
                return queryElement.GetString();
            }

            // If it's not a JSON object with query property, return the original string
            return jsonEncodedQuery;
        }
        catch (JsonException)
        {
            // If JSON parsing fails, assume it's already a plain query string
            return jsonEncodedQuery;
        }
        catch (Exception)
        {
            // For any other errors, return null to indicate extraction failed
            return null;
        }
    }

    /// <summary>
    /// Normalizes a query string to reduce cardinality by replacing high-cardinality values with placeholders.
    /// This helps prevent metric cardinality explosion when queries contain GUIDs, timestamps, or other unique values.
    /// </summary>
    /// <param name="rawQueryData">The raw query data (may be JSON-encoded or plain text)</param>
    /// <returns>A normalized query string suitable for metrics</returns>
    private string? NormalizeQueryForMetrics(string? rawQueryData)
    {
        if (string.IsNullOrEmpty(rawQueryData))
            return rawQueryData;

        try
        {
            // Step 1: Extract the actual query from JSON if needed
            string? query = rawQueryData;
            var isJsonEncoded = IsJsonEncoded(rawQueryData);
            if (isJsonEncoded) { query = ExtractQueryFromJson(rawQueryData); }
            if (string.IsNullOrEmpty(query)) { return "{QUERY_EXTRACTION_FAILED}"; }

            // Step 2: Limit query length to prevent very long queries from creating unique metrics
            var queryCostMetricRecorderOptionsValue = queryCostMetricRecorderOptions.CurrentValue;
            var normalizedQueryMaxLen = queryCostMetricRecorderOptionsValue.NormalizedQueryMaxLen;
            if (normalizedQueryMaxLen>=0 && query.Length > normalizedQueryMaxLen) { query = query.Substring(0, normalizedQueryMaxLen) + "..."; }

            // Step 3: Apply normalization patterns to reduce cardinality
            // Replace GUIDs with placeholder
            query = GuidPattern.Replace(query, "{GUID}");
            // Replace large numbers (likely IDs, timestamps, etc.) with placeholder
            query = NumberPattern.Replace(query, "{NUMBER}");
            // Replace long string literals with placeholder
            query = StringLiteralPattern.Replace(query, "'{STRING}'");
            // Replace datetime values with placeholder
            query = DateTimePattern.Replace(query, "{DATETIME}");

            // Normalize whitespace and common SQL formatting
            query = System.Text.RegularExpressions.Regex.Replace(query, @"\s+", " ").Trim();

            return query;
        }
        catch (Exception)
        {
            // If normalization fails for any reason, return a generic placeholder
            // to avoid metric recording failure
            return "{QUERY_NORMALIZATION_FAILED}";
        }
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
