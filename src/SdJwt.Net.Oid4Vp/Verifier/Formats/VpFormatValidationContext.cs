using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.Oid4Vp.Verifier.Formats;

/// <summary>
/// Context shared by OID4VP format-specific presentation validators.
/// </summary>
public sealed class VpFormatValidationContext
{
    /// <summary>
    /// Gets or sets the nonce from the Authorization Request.
    /// </summary>
    public string ExpectedNonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected verifier client identifier.
    /// </summary>
    public string? ExpectedClientId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets issuer token validation parameters for JWT-based credentials.
    /// </summary>
    public TokenValidationParameters? ValidationParameters
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets key-binding JWT validation parameters for SD-JWT VC.
    /// </summary>
    public TokenValidationParameters? KeyBindingValidationParameters
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether holder binding is required for the presentation.
    /// </summary>
    public bool RequireCryptographicHolderBinding { get; set; } = true;
}
