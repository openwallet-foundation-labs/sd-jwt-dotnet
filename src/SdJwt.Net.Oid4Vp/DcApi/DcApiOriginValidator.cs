namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Validates browser origins for DC API requests.
/// Origin validation is critical for security to prevent malicious sites
/// from intercepting credential responses.
/// </summary>
public class DcApiOriginValidator
{
    /// <summary>
    /// Validates that the response origin matches the expected client_id.
    /// </summary>
    /// <param name="responseOrigin">Origin from the DC API response.</param>
    /// <param name="expectedClientId">Expected client_id (verifier URL).</param>
    /// <returns>True if origins match, false otherwise.</returns>
    public bool ValidateOrigin(string responseOrigin, string expectedClientId)
    {
        if (string.IsNullOrWhiteSpace(responseOrigin) || string.IsNullOrWhiteSpace(expectedClientId))
        {
            return false;
        }

        try
        {
            var responseOriginExtracted = ExtractOrigin(responseOrigin);
            var expectedOriginExtracted = ExtractOrigin(expectedClientId);

            return string.Equals(
                responseOriginExtracted,
                expectedOriginExtracted,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts the origin from a URL.
    /// Origin is defined as scheme + host + port (if non-default).
    /// </summary>
    /// <param name="url">The URL to extract the origin from.</param>
    /// <returns>The origin portion of the URL.</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is invalid.</exception>
    public static string ExtractOrigin(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid URL format: {url}", nameof(url));
        }

        // Use default port detection
        var isDefaultPort = IsDefaultPort(uri);

        if (isDefaultPort)
        {
            return $"{uri.Scheme}://{uri.Host}";
        }

        return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
    }

    /// <summary>
    /// Determines if the URI uses the default port for its scheme.
    /// </summary>
    private static bool IsDefaultPort(Uri uri)
    {
        return uri.IsDefaultPort ||
               (uri.Scheme == "https" && uri.Port == 443) ||
               (uri.Scheme == "http" && uri.Port == 80);
    }
}
