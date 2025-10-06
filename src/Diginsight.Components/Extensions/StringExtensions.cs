#region using
#endregion

//using Microsoft.AspNetCore.Http;
namespace Diginsight.Components;

public static class StringExtensions {
    public static string? Mask(this string? clientSecret, int maxLen = -1)
    {
        if (string.IsNullOrEmpty(clientSecret) || clientSecret.Length <= 3) { return clientSecret; }

        var len = clientSecret.Length - 3;
        if (maxLen >= 0 && maxLen < len) { len = maxLen; }
        return clientSecret.Substring(0, 3) + new string('*', len);
    }
}
