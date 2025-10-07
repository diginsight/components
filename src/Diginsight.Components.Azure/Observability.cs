using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Diginsight.Components.Azure;

internal static class Observability
{
    public static readonly ActivitySource ActivitySource = new (Assembly.GetExecutingAssembly().GetName().Name!);

    public static ILoggerFactory LoggerFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => LoggerFactoryStaticAccessor.LoggerFactory ?? NullLoggerFactory.Instance;
    }
}
