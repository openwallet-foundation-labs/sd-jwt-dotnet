namespace SdJwt.Net.Wallet.Status;

/// <summary>
/// Document status values returned by status resolvers.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document is valid and active.
    /// </summary>
    Valid,

    /// <summary>
    /// Document is invalid or revoked.
    /// </summary>
    Invalid,

    /// <summary>
    /// Document is suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// Document has an application-specific status.
    /// </summary>
    ApplicationSpecific,

    /// <summary>
    /// Status is reserved or unknown.
    /// </summary>
    Reserved
}
