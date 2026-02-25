using System.IdentityModel.Tokens.Jwt;

namespace SdJwt.Net.Models;

/// <summary>
/// Represents the parsed components of a full SD-JWT presentation string.
/// This includes the SD-JWT, its disclosures, and an optional Key Binding JWT.
/// This class does not perform any cryptographic validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ParsedPresentation"/> class.
/// </remarks>
/// <param name="rawSdJwt">The raw SD-JWT string.</param>
/// <param name="unverifiedSdJwt">The deserialized, unverified SD-JWT.</param>
/// <param name="disclosures">The list of associated disclosures.</param>
/// <param name="rawKeyBindingJwt">The raw Key Binding JWT string, if present.</param>
/// <param name="unverifiedKeyBindingJwt">The deserialized, unverified Key Binding JWT, if present.</param>
public class ParsedPresentation(
    string rawSdJwt,
    JwtSecurityToken unverifiedSdJwt,
    IReadOnlyList<Disclosure> disclosures,
    string? rawKeyBindingJwt,
    JwtSecurityToken? unverifiedKeyBindingJwt) : ParsedSdJwt(rawSdJwt, unverifiedSdJwt, disclosures) {
        /// <summary>
        /// The Base64Url encoded Key Binding JWT part of the presentation, if present.
        /// </summary>
        public string? RawKeyBindingJwt { get; } = rawKeyBindingJwt;

        /// <summary>
        /// The deserialized, but unverified, <see cref="JwtSecurityToken"/> for the
        /// Key Binding JWT, if present.
        /// </summary>
        public JwtSecurityToken? UnverifiedKeyBindingJwt { get; } = unverifiedKeyBindingJwt;

        /// <summary>
        /// Deconstructs the parsed presentation into its core components.
        /// </summary>
        public void Deconstruct(out string rawSdJwt, out IReadOnlyList<Disclosure> disclosures, out string? rawKeyBindingJwt) {
                rawSdJwt = RawSdJwt;
                disclosures = Disclosures;
                rawKeyBindingJwt = RawKeyBindingJwt;
        }
}