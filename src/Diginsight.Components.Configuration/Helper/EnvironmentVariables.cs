namespace Diginsight.Components.Configuration;

public static class EnvironmentVariables
{
    public static string? AppsettingsEnvironmentName => Environment.GetEnvironmentVariable(nameof(AppsettingsEnvironmentName)).HardTrim();
}



