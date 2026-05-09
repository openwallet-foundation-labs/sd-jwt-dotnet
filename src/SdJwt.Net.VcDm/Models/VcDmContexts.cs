namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Well-known @context URL constants for W3C Verifiable Credentials Data Model.
/// </summary>
public static class VcDmContexts
{
    /// <summary>
    /// W3C VCDM 2.0 base context URL. MUST be the first entry in @context.
    /// </summary>
    public const string V2 = "https://www.w3.org/ns/credentials/v2";

    /// <summary>
    /// W3C VCDM 1.1 base context URL (deprecated). Accepted when reading for backward compatibility.
    /// Do not generate this value; use <see cref="V2"/> instead.
    /// </summary>
    public const string V1 = "https://www.w3.org/2018/credentials/v1";

    /// <summary>
    /// W3C Bitstring Status List 2021 context (superseded by V2 built-in).
    /// </summary>
    public const string StatusList2021 = "https://w3id.org/vc/status-list/2021/v1";

    /// <summary>
    /// Returns true when the given URL is an accepted base VCDM context (v2 or backward-compat v1).
    /// </summary>
    public static bool IsAcceptedBaseContext(string url) =>
        url == V2 || url == V1;
}
