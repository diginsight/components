#region using
#endregion

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Diginsight.Components;

public static class StringExtensions {
    public static string? Mask(this string? clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret) || clientSecret.Length <= 3) { return clientSecret; }

        return clientSecret.Substring(0, 3) + new string('*', clientSecret.Length - 3);
    }
}
