using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Diginsight.Components.Configuration;

internal static class Observability
{
    public static readonly ActivitySource ActivitySource = new(Assembly.GetExecutingAssembly().GetName().Name!);
    public static ILoggerFactory LoggerFactory { get; set; } = null!;
    static Observability() => ObservabilityRegistry.RegisterComponent(factory => LoggerFactory = factory);
}
internal static class ObservabilityHelper
{
    internal static ILoggerFactory LoggerFactory = default!;

    internal static ILoggerFactory ConfigureLoggerFactory()
    {
        return LoggerFactory;
    }
}

