namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Represents the HTTP request data used for request binding validation.
/// Separates transport-level binding from the in-token <see cref="RequestBinding"/> claim.
/// </summary>
public sealed record HttpRequestBinding
{
    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE).
    /// </summary>
    public required string Method
    {
        get; init;
    }

    /// <summary>
    /// Canonicalized request URI.
    /// </summary>
    public required string Uri
    {
        get; init;
    }

    /// <summary>
    /// Content type of the request body.
    /// </summary>
    public string? ContentType
    {
        get; init;
    }

    /// <summary>
    /// Raw request body bytes for hash computation.
    /// </summary>
    public byte[]? Body
    {
        get; init;
    }
}
