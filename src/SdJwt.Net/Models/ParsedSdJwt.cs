using System.IdentityModel.Tokens.Jwt;

namespace SdJwt.Net.Models;

/// <summary>
/// Represents the parsed components of an SD-JWT issuance string, containing the
/// core JWT and its associated disclosures. This class does not perform any
/// cryptographic validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ParsedSdJwt"/> class.
/// </remarks>
/// <param name="rawSdJwt">The raw SD-JWT string.</param>
/// <param name="unverifiedSdJwt">The deserialized, unverified JWT.</param>
/// <param name="disclosures">The list of associated disclosures.</param>
public class ParsedSdJwt(string rawSdJwt, JwtSecurityToken unverifiedSdJwt, IReadOnlyList<Disclosure> disclosures)
{
    /// <summary>
    /// The Base64Url encoded SD-JWT part of the issuance string.
    /// </summary>
    public string RawSdJwt { get; } = rawSdJwt;

    /// <summary>
    /// The deserialized, but unverified, <see cref="JwtSecurityToken"/>. This allows
    /// inspection of headers and payload before validation.
    /// </summary>
    public JwtSecurityToken UnverifiedSdJwt { get; } = unverifiedSdJwt;

    /// <summary>
    /// A read-only list of all disclosures that were part of the issuance string.
    /// </summary>
    public IReadOnlyList<Disclosure> Disclosures { get; } = disclosures;

    /// <summary>
    /// Deconstructs the parsed SD-JWT into its core components.
    /// </summary>
    public void Deconstruct(out string rawSdJwt, out IReadOnlyList<Disclosure> disclosures)
    {
        rawSdJwt = RawSdJwt;
        disclosures = Disclosures;
    }
}
