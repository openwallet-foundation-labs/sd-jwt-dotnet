namespace SdJwt.Net.OidFederation.Logic;

/// <summary>
/// Result of verifying a Trust Mark JWT's signature and claims.
/// </summary>
public sealed class TrustMarkVerificationResult
{
    /// <summary>
    /// Whether the Trust Mark JWT was successfully verified.
    /// </summary>
    public bool IsValid
    {
        get; private set;
    }

    /// <summary>
    /// Error description when verification fails.
    /// </summary>
    public string? Error
    {
        get; private set;
    }

    /// <summary>
    /// Structured error code when verification fails.
    /// </summary>
    public string? ErrorCode
    {
        get; private set;
    }

    /// <summary>
    /// The verified Trust Mark type identifier (<c>trust_mark_type</c>, or legacy <c>id</c>).
    /// </summary>
    public string? TrustMarkType
    {
        get; private set;
    }

    /// <summary>
    /// The verified issuer (<c>iss</c>).
    /// </summary>
    public string? Issuer
    {
        get; private set;
    }

    /// <summary>
    /// The verified subject (<c>sub</c>).
    /// </summary>
    public string? Subject
    {
        get; private set;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static TrustMarkVerificationResult Success(string? trustMarkType, string? issuer, string? subject)
        => new()
        {
            IsValid = true,
            TrustMarkType = trustMarkType,
            Issuer = issuer,
            Subject = subject
        };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static TrustMarkVerificationResult Failure(string error, string errorCode)
        => new()
        {
            IsValid = false,
            Error = error,
            ErrorCode = errorCode
        };
}
