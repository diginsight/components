using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

internal sealed class ActivitySourceDetectorRegistration : IActivityListenerRegistration
{
    private readonly ActivitySourceDetector detector;

    public IActivityListenerLogic Logic { get; }

    public ActivitySourceDetectorRegistration(IServiceProvider serviceProvider)
    {
        detector = ActivatorUtilities.CreateInstance<ActivitySourceDetector>(serviceProvider);
        Logic = detector;
    }

    /// <summary>
    /// Detects the activity source by logging its name, then returns <c>false</c>
    /// so this listener does NOT cause activities to be created.
    /// <see cref="ActivityListener.ShouldListenTo"/> is called exactly once per
    /// (source, listener) pair, making it an ideal detection point.
    /// </summary>
    public bool ShouldListenTo(ActivitySource activitySource)
    {
        detector.DetectSource(activitySource.Name);
        return false;
    }
}
