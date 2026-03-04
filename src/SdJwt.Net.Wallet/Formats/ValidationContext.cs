namespace SdJwt.Net.Wallet.Formats;

/// <summary>
/// Context for validation.
/// </summary>
public class ValidationContext
{
    /// <summary>
    /// Whether to validate the signature.
    /// </summary>
    public bool ValidateSignature { get; set; } = true;

    /// <summary>
    /// Whether to check expiration.
    /// </summary>
    public bool ValidateExpiration { get; set; } = true;

    /// <summary>
    /// Whether to check revocation status.
    /// </summary>
    public bool CheckRevocationStatus { get; set; } = false;

    /// <summary>
    /// Expected issuer.
    /// </summary>
    public string? ExpectedIssuer
    {
        get; set;
    }

    /// <summary>
    /// Expected audience.
    /// </summary>
    public string? ExpectedAudience
    {
        get; set;
    }

    /// <summary>
    /// Clock skew tolerance.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(5);
}
