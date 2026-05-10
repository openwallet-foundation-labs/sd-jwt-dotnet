namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result of a proof-of-possession validation (DPoP, mTLS, or SD-JWT+KB).
/// </summary>
public record ProofValidationResult
{
    /// <summary>
    /// Whether the proof is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Proof type that was validated (dpop, mtls, kb).
    /// </summary>
    public string? ProofType
    {
        get; set;
    }

    /// <summary>
    /// Error description when proof is invalid.
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
    /// JWK thumbprint of the validated proof key.
    /// </summary>
    public string? JwkThumbprint
    {
        get; set;
    }

    /// <summary>
    /// Creates a successful proof validation result.
    /// </summary>
    public static ProofValidationResult Success(string proofType, string? jwkThumbprint = null)
    {
        return new ProofValidationResult
        {
            IsValid = true,
            ProofType = proofType,
            JwkThumbprint = jwkThumbprint
        };
    }

    /// <summary>
    /// Creates a failed proof validation result.
    /// </summary>
    public static ProofValidationResult Failure(string error, string errorCode, string proofType)
    {
        return new ProofValidationResult
        {
            IsValid = false,
            Error = error,
            ErrorCode = errorCode,
            ProofType = proofType
        };
    }
}
