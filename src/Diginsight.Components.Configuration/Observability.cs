using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Diginsight.Components.Configuration;

internal static class Observability
{
    public static readonly ActivitySource ActivitySource = new(Assembly.GetExecutingAssembly().GetName().Name!);
    public static ILoggerFactory LoggerFactory { get; set; } = LoggerFactoryStaticAccessor.LoggerFactory;
    //static Observability() => ObservabilityRegistry.RegisterComponent(factory => LoggerFactory = factory);
}
internal static class ObservabilityHelper
{
    internal static ILoggerFactory LoggerFactory = default!;

    internal static ILoggerFactory ConfigureLoggerFactory()
    {
        return LoggerFactory;
    }
}

