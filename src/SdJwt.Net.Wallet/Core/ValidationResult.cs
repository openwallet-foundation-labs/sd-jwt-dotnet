namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Result of credential validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the credential is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Validation errors if not valid.
    /// </summary>
    public IList<string>? Errors
    {
        get; set;
    }

    /// <summary>
    /// Validation warnings (non-fatal).
    /// </summary>
    public IList<string>? Warnings
    {
        get; set;
    }

    /// <summary>
    /// Status check result if performed.
    /// </summary>
    public CredentialStatus? Status
    {
        get; set;
    }
}
