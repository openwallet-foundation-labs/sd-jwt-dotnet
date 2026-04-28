namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Result of a delegation chain validation.
/// </summary>
public record DelegationChainValidationResult
{
    /// <summary>
    /// Whether the delegation chain is valid.
    /// </summary>
    public bool IsValid
    {
        get; private init;
    }

    /// <summary>
    /// Error message if invalid.
    /// </summary>
    public string? Error
    {
        get; private init;
    }

    /// <summary>
    /// Error code if invalid.
    /// </summary>
    public string? ErrorCode
    {
        get; private init;
    }

    /// <summary>
    /// Effective depth of the chain.
    /// </summary>
    public int Depth
    {
        get; private init;
    }

    /// <summary>
    /// Root issuer of the chain.
    /// </summary>
    public string? RootIssuer
    {
        get; private init;
    }

    /// <summary>
    /// Creates a valid result.
    /// </summary>
    public static DelegationChainValidationResult Valid(int depth, string rootIssuer) => new()
    {
        IsValid = true,
        Depth = depth,
        RootIssuer = rootIssuer
    };

    /// <summary>
    /// Creates an invalid result.
    /// </summary>
    public static DelegationChainValidationResult Invalid(string error, string errorCode) => new()
    {
        IsValid = false,
        Error = error,
        ErrorCode = errorCode
    };
}
