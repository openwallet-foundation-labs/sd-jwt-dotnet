using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Models;

namespace SdJwt.Net.Issuer;

/// <summary>
/// A specialized issuer for creating Verifiable Credentials (VCs) using the SD-JWT format,
/// compliant with the `draft-ietf-oauth-sd-jwt-vc` specification.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SdJwtVcIssuer"/> class.
/// </remarks>
/// <param name="signingKey">The security key to sign the SD-JWT.</param>
/// <param name="signingAlgorithm">The JWT signing algorithm (e.g., "ES256", "EdDSA").</param>
/// <param name="hashAlgorithm">The hashing algorithm for disclosures (e.g., "sha-256").</param>
/// <param name="logger">An optional logger for diagnostics.</param>
public class SdJwtVcIssuer(
    SecurityKey signingKey,
    string signingAlgorithm,
    string hashAlgorithm = SdJwtConstants.DefaultHashAlgorithm,
    ILogger<SdIssuer>? logger = null)
{
    private readonly SdIssuer _sdIssuer = new SdIssuer(signingKey, signingAlgorithm, hashAlgorithm, logger);

    /// <summary>
    /// Issues a new SD-JWT Verifiable Credential.
    /// </summary>
    /// <param name="vcPayload">The strongly-typed Verifiable Credential payload.</param>
    /// <param name="vcType">The Verifiable Credential type, which will be placed in the 'vct' claim.</param>
    /// <param name="options">Options for defining which claims are disclosable and how many decoys to add. The DisclosureStructure should target claims inside the 'vc' object.</param>
    /// <param name="holderPublicKey">Optional Holder public key for key binding.</param>
    /// <returns>An <see cref="IssuerOutput"/> containing the full issuance string.</returns>
    public IssuerOutput Issue(
        VerifiableCredentialPayload vcPayload,
        string vcType,
        SdIssuanceOptions options,
        JsonWebKey? holderPublicKey = null)
    {
        if (vcPayload == null) { throw new ArgumentNullException(nameof(vcPayload)); }
        if (string.IsNullOrWhiteSpace(vcType)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(vcType)); }
        if (options == null) { throw new ArgumentNullException(nameof(options)); }

        // According to the SD-JWT-VC spec, the JWT payload has a very specific structure.
        var jwtPayload = new JwtPayload
        {
            // The 'vct' claim is mandatory and at the top level.
            { "vct", vcType },
            
            // All VC data is wrapped in a 'vc' claim.
            { "vc", vcPayload }
        };

        // Pass this structured payload to the underlying SD-JWT issuer.
        return _sdIssuer.Issue(jwtPayload, options, holderPublicKey);
    }
}