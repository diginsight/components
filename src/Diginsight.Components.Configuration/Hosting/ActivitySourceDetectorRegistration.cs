using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

internal sealed class ActivitySourceDetectorRegistration : IActivityListenerRegistration
{
    public IActivityListenerLogic Logic { get; }

    public ActivitySourceDetectorRegistration(IServiceProvider serviceProvider)
    {
        Logic = ActivatorUtilities.CreateInstance<ActivitySourceDetector>(serviceProvider);
    }

    public bool ShouldListenTo(ActivitySource activitySource) => true;
}
