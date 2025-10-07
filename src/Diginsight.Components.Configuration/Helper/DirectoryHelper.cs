namespace Diginsight.Components.Configuration;

public static class DirectoryHelper
{
    public static string? GetRepositoryRoot(string currentDirectory)
    {
        var directoryInfo = new DirectoryInfo(currentDirectory);
        while (directoryInfo is not null && !Directory.Exists(Path.Combine(directoryInfo.FullName, ".git")))
        {
            directoryInfo = directoryInfo.Parent;
        }
        return directoryInfo?.FullName;
    }
}
