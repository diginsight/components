namespace Diginsight.Components;

public class DebugHelper
{
    //private static ConcurrentDictionary<string, object> _dicOverrides = new ConcurrentDictionary<string, object>();

    public static bool IsDebugBuild { get; set; } = false;
    public static bool IsReleaseBuild { get => !IsDebugBuild; set => IsDebugBuild = !value; }

    static DebugHelper()
    {
#if DEBUG
        IsDebugBuild = true;
#endif
    }

    public void IfDebug(Action action)
    {
        if (!IsDebugBuild) { return; }
        action();
    }
}
