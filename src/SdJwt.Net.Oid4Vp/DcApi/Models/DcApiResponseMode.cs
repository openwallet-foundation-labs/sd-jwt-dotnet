namespace SdJwt.Net.Oid4Vp.DcApi.Models;

/// <summary>
/// Response modes supported by the Digital Credentials API.
/// </summary>
public enum DcApiResponseMode
{
    /// <summary>
    /// Plain response - VP token returned directly in response.
    /// </summary>
    DcApi,

    /// <summary>
    /// Encrypted response - VP token wrapped in JWE for privacy.
    /// </summary>
    DcApiJwt
}
