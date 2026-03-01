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
    string FormatId
    {
        get;
    }

    /// <summary>
    /// Display name for the format.
    /// </summary>
    string DisplayName
    {
        get;
    }

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
