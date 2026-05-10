namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result of a revocation operation.
/// </summary>
public sealed record RevocationResult
{
    /// <summary>
    /// Whether the revocation was accepted.
    /// </summary>
    public bool IsRevoked
    {
        get; init;
    }

    /// <summary>
    /// Error message if revocation failed.
    /// </summary>
    public string? Error
    {
        get; init;
    }

    /// <summary>
    /// Creates a successful revocation result.
    /// </summary>
    public static RevocationResult Success() => new() { IsRevoked = true };

    /// <summary>
    /// Creates a failed revocation result.
    /// </summary>
    public static RevocationResult Failure(string error) => new() { IsRevoked = false, Error = error };
}
