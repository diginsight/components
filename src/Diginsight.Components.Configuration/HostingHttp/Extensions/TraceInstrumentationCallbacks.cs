using System.Diagnostics;

namespace Diginsight.Components.Configuration;

public sealed class TraceInstrumentationCallbacks
{
    public Func<Activity, HttpRequestMessage, bool> ShouldSetRequestContentTag { get; set; } = static (_, _) => false;

    public Func<Activity, HttpResponseMessage, bool> ShouldSetResponseContentTag { get; set; } = static (_, _) => false;

    public Func<Activity, Exception, bool> ShouldSetStackTraceTag { get; set; } = static (_, _) => true;
}
