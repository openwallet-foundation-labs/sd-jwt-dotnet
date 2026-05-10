namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// An audit receipt with a cryptographic signature for tamper evidence.
/// </summary>
public record SignedAuditReceipt
{
    /// <summary>
    /// The underlying audit receipt.
    /// </summary>
    public AuditReceipt Receipt { get; set; } = new();

    /// <summary>
    /// Compact JWS signature over the receipt payload.
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Key identifier used for the signature.
    /// </summary>
    public string? KeyId
    {
        get; set;
    }

    /// <summary>
    /// Signing algorithm used (e.g., ES256, PS256).
    /// </summary>
    public string? Algorithm
    {
        get; set;
    }

    /// <summary>
    /// SHA-256 hash of the previous receipt for hash-chain linking.
    /// </summary>
    public string? PreviousReceiptHash
    {
        get; set;
    }

    /// <summary>
    /// Receipt chain context for structured chain linking.
    /// </summary>
    public ReceiptChainContext? Chain
    {
        get; set;
    }
}
