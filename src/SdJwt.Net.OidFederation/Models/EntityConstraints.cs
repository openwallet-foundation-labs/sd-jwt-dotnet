using System.Text.Json.Serialization;

namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Represents entity constraints as defined in OpenID Federation 1.0.
/// Used to define operational limitations for federation entities.
/// </summary>
public class EntityConstraints
{
    /// <summary>
    /// Gets or sets the maximum path length for trust chains involving this entity.
    /// Optional. Limits how deep the trust chain can be when this entity is involved.
    /// </summary>
    [JsonPropertyName("max_path_length")]
    public int? MaxPathLength { get; set; }

    /// <summary>
    /// Gets or sets the allowed leaf entity types.
    /// Optional. Restricts which entity types can be at the end of trust chains.
    /// </summary>
    [JsonPropertyName("naming_constraints")]
    public NamingConstraints? NamingConstraints { get; set; }

    /// <summary>
    /// Gets or sets additional custom constraints.
    /// Allows extension for domain-specific constraint types.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalConstraints { get; set; }

    /// <summary>
    /// Validates the entity constraints.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
    public void Validate()
    {
        if (MaxPathLength.HasValue && MaxPathLength.Value < 0)
            throw new InvalidOperationException("MaxPathLength must be non-negative");

        NamingConstraints?.Validate();
    }
}

/// <summary>
/// Represents naming constraints for entity URLs.
/// </summary>
public class NamingConstraints
{
    /// <summary>
    /// Gets or sets the permitted URL patterns.
    /// Optional. Array of URL patterns that subordinate entities must match.
    /// </summary>
    [JsonPropertyName("permitted")]
    public string[]? Permitted { get; set; }

    /// <summary>
    /// Gets or sets the excluded URL patterns.
    /// Optional. Array of URL patterns that subordinate entities must not match.
    /// </summary>
    [JsonPropertyName("excluded")]
    public string[]? Excluded { get; set; }

    /// <summary>
    /// Validates the naming constraints.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
    public void Validate()
    {
        if (Permitted != null)
        {
            foreach (var pattern in Permitted)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    throw new InvalidOperationException("Permitted pattern cannot be null or empty");
            }
        }

        if (Excluded != null)
        {
            foreach (var pattern in Excluded)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    throw new InvalidOperationException("Excluded pattern cannot be null or empty");
            }
        }
    }

    /// <summary>
    /// Checks if a URL is permitted by these naming constraints.
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <returns>True if the URL is permitted, false otherwise</returns>
    public bool IsPermitted(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Check excluded patterns first
        if (Excluded != null)
        {
            foreach (var pattern in Excluded)
            {
                if (MatchesPattern(url, pattern))
                    return false;
            }
        }

        // If no permitted patterns, allow by default
        if (Permitted == null || Permitted.Length == 0)
            return true;

        // Check permitted patterns
        foreach (var pattern in Permitted)
        {
            if (MatchesPattern(url, pattern))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Simple pattern matching for URL patterns.
    /// Supports basic wildcard patterns with *.
    /// </summary>
    /// <param name="url">The URL to match</param>
    /// <param name="pattern">The pattern to match against</param>
    /// <returns>True if the URL matches the pattern</returns>
    private static bool MatchesPattern(string url, string pattern)
    {
        if (pattern == "*")
            return true;

        if (!pattern.Contains('*'))
            return url.Equals(pattern, StringComparison.OrdinalIgnoreCase);

        // Convert pattern to regex-like behavior
        // This is a simplified implementation - production might want more sophisticated matching
        var regexPattern = pattern
            .Replace(".", "\\.")
            .Replace("*", ".*");

        return System.Text.RegularExpressions.Regex.IsMatch(url, $"^{regexPattern}$", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}