namespace SdJwt.Net.Eudiw;

/// <summary>
/// Options for PID credential request.
/// </summary>
public class PidRequestOptions
{
    /// <summary>
    /// Authentication method to use for issuance.
    /// </summary>
    public EudiAuthMethod AuthMethod { get; set; } = EudiAuthMethod.AuthorizationCode;

    /// <summary>
    /// Pre-authorized code if using pre-auth flow.
    /// </summary>
    public string? PreAuthorizedCode
    {
        get; set;
    }

    /// <summary>
    /// User PIN if required.
    /// </summary>
    public string? UserPin
    {
        get; set;
    }

    /// <summary>
    /// Key algorithm for credential binding.
    /// </summary>
    public string KeyAlgorithm { get; set; } = "ES256";

    /// <summary>
    /// Redirect URI for authorization code flow.
    /// </summary>
    public string? RedirectUri
    {
        get; set;
    }
}
