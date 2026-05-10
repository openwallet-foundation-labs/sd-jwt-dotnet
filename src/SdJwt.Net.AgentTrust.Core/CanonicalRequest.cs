namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result of request canonicalization for hash computation and binding validation.
/// </summary>
public sealed record CanonicalRequest
{
    /// <summary>
    /// Canonicalized HTTP method.
    /// </summary>
    public required string Method
    {
        get; init;
    }

    /// <summary>
    /// Canonicalized URI.
    /// </summary>
    public required string Uri
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the canonicalized request body.
    /// </summary>
    public string? BodyHash
    {
        get; init;
    }

    /// <summary>
    /// Content type of the request.
    /// </summary>
    public string? ContentType
    {
        get; init;
    }
}
