namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result of token verification.
/// </summary>
public record CapabilityVerificationResult
{
    /// <summary>
    /// Whether the token is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Error message.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Structured error code.
    /// </summary>
    public string? ErrorCode
    {
        get; set;
    }

    /// <summary>
    /// Verified capability claim.
    /// </summary>
    public CapabilityClaim? Capability
    {
        get; set;
    }

    /// <summary>
    /// Verified context claim.
    /// </summary>
    public CapabilityContext? Context
    {
        get; set;
    }

    /// <summary>
    /// Token identifier.
    /// </summary>
    public string? TokenId
    {
        get; set;
    }

    /// <summary>
    /// Issuer identity.
    /// </summary>
    public string? Issuer
    {
        get; set;
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static CapabilityVerificationResult Success(
        CapabilityClaim capability,
        CapabilityContext context,
        string tokenId,
        string issuer)
    {
        return new CapabilityVerificationResult
        {
            IsValid = true,
            Capability = capability,
            Context = context,
            TokenId = tokenId,
            Issuer = issuer
        };
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static CapabilityVerificationResult Failure(string error, string errorCode)
    {
        return new CapabilityVerificationResult
        {
            IsValid = false,
            Error = error,
            ErrorCode = errorCode
        };
    }
}

