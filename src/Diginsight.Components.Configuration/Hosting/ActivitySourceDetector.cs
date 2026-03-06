using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

internal sealed class ActivitySourceDetector : IActivityListenerLogic
{
    private readonly ILogger logger;
    private readonly ConcurrentDictionary<string, ValueTuple> seenActivitySources = new();

    public ActivitySourceDetector(ILogger<ActivitySourceDetector> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Detects and logs a newly-seen activity source name.
    /// Called from <see cref="ActivitySourceDetectorRegistration.ShouldListenTo"/>
    /// so that detection happens without requiring activities to be created.
    /// </summary>
    public void DetectSource(string activitySourceName)
    {
        if (seenActivitySources.TryAdd(activitySourceName, default))
        {
            logger.LogDebug("New activity source detected: {ActivitySource}", activitySourceName);
        }
    }

    public void ActivityStarted(Activity activity) { }
}
