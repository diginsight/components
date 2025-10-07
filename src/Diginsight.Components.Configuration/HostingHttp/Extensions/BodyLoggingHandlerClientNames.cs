namespace Diginsight.Components.Configuration;

public sealed class BodyLoggingHandlerClientNames : HashSet<string>
{
    public BodyLoggingHandlerClientNames()
        : base(StringComparer.OrdinalIgnoreCase) { }
}