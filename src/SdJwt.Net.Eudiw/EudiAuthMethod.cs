namespace SdJwt.Net.Eudiw;

/// <summary>
/// Authentication methods for EUDI credential issuance.
/// </summary>
public enum EudiAuthMethod
{
    /// <summary>
    /// OAuth 2.0 Authorization Code flow.
    /// </summary>
    AuthorizationCode,

    /// <summary>
    /// Pre-Authorized Code flow.
    /// </summary>
    PreAuthorized,

    /// <summary>
    /// eIDAS authentication.
    /// </summary>
    Eidas
}
