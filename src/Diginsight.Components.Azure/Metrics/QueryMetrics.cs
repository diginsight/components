using System.Diagnostics.Metrics;

namespace Diginsight.Components.Azure.Metrics
{
    public static class QueryMetrics
    {
        private static readonly Meter QueryMeter = new("Diginsight.Components.Azure", "1.0");
        
        public static readonly Histogram<double> QueryCost = QueryMeter.CreateHistogram<double>(
            "diginsight.query_cost",
            unit: "RU",
            description: "CosmosDB query cost in Request Units");
    }
}
