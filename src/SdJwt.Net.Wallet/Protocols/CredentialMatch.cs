using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Represents a credential match for the request.
/// </summary>
public class CredentialMatch
{
    /// <summary>
    /// The input descriptor ID this matches.
    /// </summary>
    public string InputDescriptorId { get; set; } = string.Empty;

    /// <summary>
    /// The matching credential.
    /// </summary>
    public StoredCredential? Credential
    {
        get; set;
    }

    /// <summary>
    /// Claims that will be disclosed.
    /// </summary>
    public IReadOnlyList<string> DisclosedClaims { get; set; } = [];

    /// <summary>
    /// Whether all required claims are satisfied.
    /// </summary>
    public bool SatisfiesRequirements
    {
        get; set;
    }

    /// <summary>
    /// Missing required claims if any.
    /// </summary>
    public IReadOnlyList<string> MissingClaims { get; set; } = [];
}
