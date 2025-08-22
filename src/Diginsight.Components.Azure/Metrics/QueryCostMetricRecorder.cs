using Azure;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Diginsight.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
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
    public string[] IgnoreQueryCallers { get; set; } = [];
}

public sealed class QueryCostMetricRecorder : IActivityListenerLogic
{
    // Enhanced regex patterns for better CosmosDB SQL normalization
    private static readonly Regex GuidPattern = new Regex(@"\b[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NumberPattern = new Regex(@"\b\d{4,}\b", RegexOptions.Compiled);
    private static readonly Regex StringLiteralPattern = new Regex(@"'[^']{6,}'", RegexOptions.Compiled);
    private static readonly Regex DateTimePattern = new Regex(@"\b\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?(?:Z|[+-]\d{2}:\d{2})?\b", RegexOptions.Compiled);
    // CosmosDB specific patterns for more sophisticated normalization
    private static readonly Regex PropertyAccessPattern = new Regex(@"(root\[""[^""]+\""\])\s*=\s*([""'][^""']*[""']|\{GUID\}|\{NUMBER\}|\{STRING\}|\{DATETIME\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex InClausePattern = new Regex(@"IN\s*\([^)]+\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BetweenClausePattern = new Regex(@"BETWEEN\s+([""'][^""']*[""']|\{[A-Z]+\}|\d+)\s+AND\s+([""'][^""']*[""']|\{[A-Z]+\}|\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ArrayContainsPattern = new Regex(@"ARRAY_CONTAINS\s*\([^,]+,\s*([""'][^""']*[""']|\{[A-Z]+\})\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OrderByPattern = new Regex(@"ORDER\s+BY\s+[^()]+?(ASC|DESC)?(?:\s*,\s*[^()]+?(ASC|DESC)?)*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ILogger logger;
    private readonly IClassAwareOptionsMonitor<QueryCostMetricRecorderOptions> queryCostMetricRecorderOptions;
    private readonly IMetricRecordingFilter? metricFilter;
    private readonly IMetricRecordingEnricher? metricEnricher;

    private readonly Lazy<Histogram<double>> lazyMetric;
    private Histogram<double> Metric => lazyMetric.Value;

    // Cache for compiled regex patterns to avoid recompilation on every evaluation
    private readonly ConcurrentDictionary<string, Regex> ignoreQueryCallersRegexCache = new();

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

                var queryCostMetricRecorderOptionsValue = queryCostMetricRecorderOptions.CurrentValue;
                var addNormalizedQueryTag = queryCostMetricRecorderOptionsValue.AddNormalizedQueryTag;
                var addQueryCallers = queryCostMetricRecorderOptionsValue.AddQueryCallers;
                var ignoreQueryCallers = queryCostMetricRecorderOptionsValue.IgnoreQueryCallers;

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
                    if (ignoreQueryCallers?.Length > 0)
                    {
                        diginsightCallers = diginsightCallers.Where(caller => !ShouldIgnoreCaller(caller.OperationName, ignoreQueryCallers));
                    }

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
    /// Determines whether a caller should be ignored based on the configured ignore patterns.
    /// Uses a cached regex dictionary for performance optimization.
    /// </summary>
    /// <param name="operationName">The operation name to check</param>
    /// <param name="ignorePatterns">Array of patterns to match against</param>
    /// <returns>True if the caller should be ignored, false otherwise</returns>
    private bool ShouldIgnoreCaller(string operationName, string[] ignorePatterns)
    {
        if (string.IsNullOrEmpty(operationName) || ignorePatterns?.Length == 0)
            return false;

        foreach (var pattern in ignorePatterns!)
        {
            if (string.IsNullOrEmpty(pattern))
                continue;

            // Support both exact matches and wildcard patterns
            if (pattern.Contains('*'))
            {
                // Get or create compiled regex from cache
                var regex = ignoreQueryCallersRegexCache.GetOrAdd(pattern, patternKey =>
                {
                    try
                    {
                        // Convert wildcard pattern to regex
                        var regexPattern = "^" + Regex.Escape(patternKey).Replace(@"\*", ".*") + "$";
                        return new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to compile regex pattern for IgnoreQueryCallers: {Pattern}", patternKey);
                        // Return a regex that never matches if compilation fails
                        return new Regex("(?!.*)", RegexOptions.Compiled);
                    }
                });

                if (regex.IsMatch(operationName))
                    return true;
            }
            else
            {
                // Exact match (case insensitive)
                if (string.Equals(operationName, pattern, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
        }

        return false;
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
    /// Normalizes a CosmosDB SQL query string to reduce cardinality by replacing high-cardinality values 
    /// with semantic placeholders while preserving query structure and intent.
    /// </summary>
    /// <param name="rawQueryData">The raw query data (may be JSON-encoded or plain text)</param>
    /// <returns>A normalized query string suitable for metrics with semantic meaning preserved</returns>
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

            // Step 2: Apply sophisticated normalization patterns for CosmosDB queries
            query = NormalizeCosmosDbQuery(query);

            // Step 3: Apply length limit after normalization (to preserve more semantic meaning)
            var queryCostMetricRecorderOptionsValue = queryCostMetricRecorderOptions.CurrentValue;
            var normalizedQueryMaxLen = queryCostMetricRecorderOptionsValue.NormalizedQueryMaxLen;
            if (normalizedQueryMaxLen >= 0 && query.Length > normalizedQueryMaxLen) 
            { 
                query = query.Substring(0, normalizedQueryMaxLen) + "..."; 
            }

            return query;
        }
        catch (Exception ex)
        {
            // If normalization fails, extract query prefix up to FROM clause for better context
            logger.LogDebug(ex, "Query normalization failed for: {RawQuery}", rawQueryData);
            return ExtractQueryPrefixOnFailure(rawQueryData);
        }
    }

    /// <summary>
    /// Applies CosmosDB-specific normalization patterns to preserve query structure 
    /// while reducing cardinality through intelligent value replacement.
    /// </summary>
    /// <param name="query">The SQL query to normalize</param>
    /// <returns>Normalized query with preserved semantic structure</returns>
    private static string NormalizeCosmosDbQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
            return query;

        // Step 1: Normalize whitespace first for consistent processing
        query = Regex.Replace(query, @"\s+", " ").Trim();

        // Step 2: Apply value replacements (order matters - specific to general)
        
        // Replace GUIDs first (most specific)
        query = GuidPattern.Replace(query, "{GUID}");
        
        // Replace datetime values
        query = DateTimePattern.Replace(query, "{DATETIME}");
        
        // Replace long string literals (but preserve short ones like "Type" values)
        query = StringLiteralPattern.Replace(query, "'{STRING}'");
        
        // Replace large numbers (likely IDs, but preserve small numbers that might be meaningful)
        query = NumberPattern.Replace(query, "{NUMBER}");

        // Step 3: Apply CosmosDB-specific structural normalizations
        
        // Normalize IN clauses with multiple values
        query = InClausePattern.Replace(query, "IN ({ITEMS})");
        
        // Normalize BETWEEN clauses
        query = BetweenClausePattern.Replace(query, "BETWEEN {VALUE} AND {VALUE}");
        
        // Normalize ARRAY_CONTAINS functions
        query = ArrayContainsPattern.Replace(query, match => 
        {
            var prefix = match.Value.Substring(0, match.Value.LastIndexOf(',') + 1);
            return $"{prefix} {{VALUE}})";
        });

        // Step 4: Normalize complex expressions while preserving query intent
        query = NormalizeWhereClause(query);
        
        // Step 5: Normalize ORDER BY clauses (preserve structure but remove specifics)
        query = OrderByPattern.Replace(query, "ORDER BY {FIELDS}");

        // Step 6: Final cleanup - normalize any remaining whitespace irregularities
        query = Regex.Replace(query, @"\s+", " ").Trim();

        return query;
    }

    /// <summary>
    /// Normalizes WHERE clause expressions to group similar query patterns 
    /// while preserving logical structure.
    /// </summary>
    /// <param name="query">Query containing WHERE clause</param>
    /// <returns>Query with normalized WHERE conditions</returns>
    private static string NormalizeWhereClause(string query)
    {
        // Handle complex WHERE clauses with multiple ANDs/ORs
        // This approach preserves the logical structure while normalizing values
        
        // Pattern: (condition1 AND condition2 AND ...)
        var complexAndPattern = new Regex(
            @"\(\s*([^()]+)\s+AND\s+([^()]+)(?:\s+AND\s+[^()]+)*\s*\)",
            RegexOptions.IgnoreCase);

        query = complexAndPattern.Replace(query, match =>
        {
            var conditions = match.Value
                .Trim('(', ')')
                .Split(new[] { " AND " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToArray();

            // Sort conditions to normalize order (e.g., Type conditions first, then others)
            var normalizedConditions = conditions
                .OrderBy(c => c.Contains("\"Type\"") ? 0 : 1) // Type conditions first
                .ThenBy(c => c) // Then alphabetically
                .ToArray();

            return $"({string.Join(" AND ", normalizedConditions)})";
        });

        // Similar handling for OR clauses if needed
        var complexOrPattern = new Regex(
            @"\(\s*([^()]+)\s+OR\s+([^()]+)(?:\s+OR\s+[^()]+)*\s*\)",
            RegexOptions.IgnoreCase);

        query = complexOrPattern.Replace(query, match =>
        {
            var conditions = match.Value
                .Trim('(', ')')
                .Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToArray();

            var normalizedConditions = conditions
                .OrderBy(c => c.Contains("\"Type\"") ? 0 : 1)
                .ThenBy(c => c)
                .ToArray();

            return $"({string.Join(" OR ", normalizedConditions)})";
        });

        return query;
    }

    /// <summary>
    /// Extracts a meaningful query prefix when normalization fails, providing up to the FROM clause
    /// with a clear indication that normalization failed.
    /// </summary>
    /// <param name="rawQueryData">The original raw query data</param>
    /// <returns>A query prefix with failure indication</returns>
    private static string ExtractQueryPrefixOnFailure(string? rawQueryData)
    {
        if (string.IsNullOrEmpty(rawQueryData))
            return "{QUERY_NORMALIZATION_FAILED}";

        try
        {
            // First try to extract from JSON if it looks like JSON
            string? query = rawQueryData;
            if (IsJsonEncoded(rawQueryData))
            {
                try
                {
                    using var document = JsonDocument.Parse(rawQueryData);
                    if (document.RootElement.TryGetProperty("query", out var queryElement))
                    {
                        query = queryElement.GetString();
                    }
                }
                catch
                {
                    // If JSON extraction fails, use original
                    query = rawQueryData;
                }
            }

            if (string.IsNullOrEmpty(query))
                return "{QUERY_NORMALIZATION_FAILED}";

            // Normalize whitespace first for easier processing
            query = System.Text.RegularExpressions.Regex.Replace(query, @"\s+", " ").Trim();

            // Look for FROM clause (case insensitive)
            var fromMatch = System.Text.RegularExpressions.Regex.Match(
                query, 
                @"^(.+?\bFROM\s+\w+)", 
                RegexOptions.IgnoreCase);

            if (fromMatch.Success)
            {
                var prefix = fromMatch.Groups[1].Value.Trim();
                return $"{prefix} ... (query normalization failed)";
            }

            // If no FROM clause found, take first 50 characters as fallback
            if (query.Length > 50)
            {
                return $"{query.Substring(0, 50)}... (query normalization failed)";
            }

            return $"{query} (query normalization failed)";
        }
        catch
        {
            // Ultimate fallback if even prefix extraction fails
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
