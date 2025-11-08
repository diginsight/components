namespace Diginsight.Components.Azure;

public static class CosmosDbHelper
{
    /// <summary>
    /// Sanitizes a string (typically a URL) to be used as a CosmosDB document ID.
    /// CosmosDB IDs cannot contain: /, \, ?, #
    /// </summary>
    /// <param name="rawId">The raw identifier (e.g., URL)</param>
    /// <returns>URL-encoded identifier safe for use as CosmosDB document ID</returns>
    public static string SanitizeCosmosDbId(string rawId)
    {
        if (string.IsNullOrEmpty(rawId)) { throw new ArgumentException("Document ID cannot be null or empty", nameof(rawId)); }

        // URL-encode the entire string to escape all special characters this ensures all /, \, ?, # characters are properly encoded
        var sanitized = Uri.EscapeDataString(rawId);

        // CosmosDB has a 255 character limit for IDs
        if (sanitized.Length > 255)
        {
            // If too long, use a hash of the original with a prefix
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawId));
            var base64Hash = Convert.ToBase64String(hash)
                .Replace("/", "_")
                .Replace("+", "-")
                .TrimEnd('=');

            // Include a prefix of the original for readability
            var prefix = sanitized.Substring(0, Math.Min(200, sanitized.Length));
            sanitized = $"{prefix}_{base64Hash}";
        }

        return sanitized;
    }
}
