namespace Diginsight.Components.Azure.Abstractions;

/// <summary>
/// Enumeration defining the available property naming policies for response formatting.
/// Used to control how property names are formatted in API responses.
/// </summary>
public enum PropertyNamingPolicy
{
    /// <summary>
    /// Uses the original property names without any transformation.
    /// </summary>
    Original,

    /// <summary>
    /// Converts property names to camelCase format (e.g., "firstName").
    /// </summary>
    CamelCase,

    /// <summary>
    /// Converts property names to lowercase kebab-case format (e.g., "first-name").
    /// </summary>
    KebabCaseLower,

    /// <summary>
    /// Converts property names to uppercase kebab-case format (e.g., "FIRST-NAME").
    /// </summary>
    KebabCaseUpper,

    /// <summary>
    /// Converts property names to lowercase snake_case format (e.g., "first_name").
    /// </summary>
    SnakeCaseLower,

    /// <summary>
    /// Converts property names to uppercase snake_case format (e.g., "FIRST_NAME").
    /// </summary>
    SnakeCaseUpper
}
