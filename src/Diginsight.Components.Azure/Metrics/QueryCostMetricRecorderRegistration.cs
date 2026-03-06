using Diginsight.Diagnostics;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Diginsight.Components.Azure.Metrics;

public class QueryCostMetricRecorderRegistration : IActivityListenerRegistration
{
    private readonly IDiginsightActivitiesOptions activitiesOptions;

    public IActivityListenerLogic Logic { get; }

    public QueryCostMetricRecorderRegistration(
        QueryCostMetricRecorder recorder,
        IOptions<DiginsightActivitiesOptions> activitiesOptions
    )
    {
        Logic = recorder;
        this.activitiesOptions = activitiesOptions.Value.Freeze();
    }

    public bool ShouldListenTo(ActivitySource activitySource)
    {
        // Respect the Diginsight:Activities:ActivitySources config.
        // The "query_cost" tag is set on activities from the Diginsight.Components.Azure
        // ActivitySource — if the user hasn't enabled that source in config, the recorder
        // should not force-create activities for it.
        string activitySourceName = activitySource.Name;
        bool anyMatch = false;

        foreach (KeyValuePair<string, bool> kvp in activitiesOptions.ActivitySources)
        {
            if (!ActivityUtils.NameMatchesPattern(activitySourceName, kvp.Key))
                continue;

            if (!kvp.Value)
                return false;

            anyMatch = true;
        }

        return anyMatch;
    }
}
