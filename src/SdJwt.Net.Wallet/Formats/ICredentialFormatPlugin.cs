using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Formats;

/// <summary>
/// Plugin interface for credential format handling.
/// Enables extensible support for different credential formats.
/// </summary>
public interface ICredentialFormatPlugin
{
    /// <summary>
    /// Format identifier (e.g., "vc+sd-jwt", "mso_mdoc").
    /// </summary>
    string FormatId { get; }

    /// <summary>
    /// Display name for the format.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Parses a credential in this format.
    /// </summary>
    /// <param name="credential">The raw credential string.</param>
    /// <param name="options">Parse options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed credential.</returns>
    Task<ParsedCredential> ParseAsync(
        string credential,
        ParseOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a presentation with selective disclosure.
    /// </summary>
    /// <param name="credential">The parsed credential.</param>
    /// <param name="disclosurePaths">Paths of claims to disclose.</param>
    /// <param name="context">Presentation context.</param>
    /// <param name="keyManager">Key manager for signing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The presentation string.</returns>
    Task<string> CreatePresentationAsync(
        ParsedCredential credential,
        IReadOnlyList<string> disclosurePaths,
        PresentationContext context,
        IKeyManager keyManager,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the credential signature and structure.
    /// </summary>
    /// <param name="credential">The parsed credential.</param>
    /// <param name="context">Validation context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<ValidationResult> ValidateAsync(
        ParsedCredential credential,
        ValidationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if this plugin can handle the given credential.
    /// </summary>
    /// <param name="credential">The raw credential string.</param>
    /// <returns>True if this plugin can handle the credential.</returns>
    bool CanHandle(string credential);
}

/// <summary>
/// Options for parsing credentials.
/// </summary>
public class ParseOptions
{
    /// <summary>
    /// Whether to strictly validate the credential structure.
    /// </summary>
    public bool StrictValidation { get; set; } = true;

    /// <summary>
    /// Whether to extract all disclosures.
    /// </summary>
    public bool ExtractDisclosures { get; set; } = true;
}

/// <summary>
/// Context for creating presentations.
/// </summary>
public class PresentationContext
{
    /// <summary>
    /// The audience (verifier) for the presentation.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Nonce from the verifier.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Key ID to use for key binding JWT.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Signing algorithm for key binding JWT.
    /// </summary>
    public string SigningAlgorithm { get; set; } = "ES256";

    /// <summary>
    /// Optional state value.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Issued at time. Defaults to now.
    /// </summary>
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
}

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
    public string? ExpectedIssuer { get; set; }

    /// <summary>
    /// Expected audience.
    /// </summary>
    public string? ExpectedAudience { get; set; }

    /// <summary>
    /// Clock skew tolerance.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(5);
}
