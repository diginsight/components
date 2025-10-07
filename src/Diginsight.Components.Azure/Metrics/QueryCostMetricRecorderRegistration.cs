using Diginsight.Diagnostics;
using System.Diagnostics;

namespace Diginsight.Components.Azure.Metrics;

public class QueryCostMetricRecorderRegistration : IActivityListenerRegistration
{
    public IActivityListenerLogic Logic { get; }

    public QueryCostMetricRecorderRegistration(QueryCostMetricRecorder recorder)
    {
        Logic = recorder;
    }

    public bool ShouldListenTo(ActivitySource activitySource)
    {
        // Listen to Diginsight activities that might contain CosmosDB operations
        return activitySource.Name.StartsWith("Diginsight") ||
            activitySource.Name.Contains("CosmosDB") ||
            activitySource.Name.Contains("Azure");
    }
}
