namespace SdJwt.Net.HAIP;

/// <summary>
/// OpenID4VC HAIP 1.0 Final flow selected for validation.
/// </summary>
public enum HaipFlow
{
    /// <summary>
    /// OpenID4VCI credential issuance flow.
    /// </summary>
    Oid4VciIssuance,

    /// <summary>
    /// OpenID4VP presentation flow using redirects.
    /// </summary>
    Oid4VpRedirectPresentation,

    /// <summary>
    /// OpenID4VP presentation flow using the W3C Digital Credentials API.
    /// </summary>
    Oid4VpDigitalCredentialsApiPresentation
}

/// <summary>
/// Credential format profile selected for HAIP validation.
/// </summary>
public enum HaipCredentialProfile
{
    /// <summary>
    /// IETF SD-JWT VC profile.
    /// </summary>
    SdJwtVc,

    /// <summary>
    /// ISO mdoc profile.
    /// </summary>
    MsoMdoc
}

/// <summary>
/// Declarative capabilities and policy switches used to validate a HAIP 1.0 Final profile.
/// </summary>
public sealed class HaipProfileOptions
{
    /// <summary>
    /// Gets the flows selected for HAIP validation.
    /// </summary>
    public ISet<HaipFlow> Flows { get; } = new HashSet<HaipFlow>();

    /// <summary>
    /// Gets the credential profiles selected for HAIP validation.
    /// </summary>
    public ISet<HaipCredentialProfile> CredentialProfiles { get; } = new HashSet<HaipCredentialProfile>();

    /// <summary>
    /// Gets or sets a value indicating whether authorization code flow is supported for OID4VCI.
    /// </summary>
    public bool SupportsAuthorizationCodeFlow
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether PKCE with S256 is enforced.
    /// </summary>
    public bool EnforcesPkceS256
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether Pushed Authorization Requests are supported where applicable.
    /// </summary>
    public bool SupportsPushedAuthorizationRequests
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Authorization Endpoint is used.
    /// </summary>
    public bool UsesAuthorizationEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether DPoP sender-constrained access tokens are supported.
    /// </summary>
    public bool SupportsDpop
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether DPoP nonce handling is supported.
    /// </summary>
    public bool SupportsDpopNonce
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether Wallet Attestation is validated cryptographically.
    /// </summary>
    public bool ValidatesWalletAttestation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether Key Attestation is validated cryptographically.
    /// </summary>
    public bool ValidatesKeyAttestation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether credential configurations that require key binding publish a nonce endpoint.
    /// </summary>
    public bool PublishesNonceEndpointForKeyBinding
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether key-bound credential configurations are supported.
    /// </summary>
    public bool SupportsKeyBoundCredentialConfigurations
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether signed credential issuer metadata can be validated through x5c.
    /// </summary>
    public bool SupportsSignedIssuerMetadataWithX5c
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether DCQL is supported for OpenID4VP requests and responses.
    /// </summary>
    public bool SupportsDcql
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether OpenID4VP signed request objects are supported.
    /// </summary>
    public bool SupportsSignedPresentationRequests
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether verifier attestation information can be validated.
    /// </summary>
    public bool ValidatesVerifierAttestation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether W3C Digital Credentials API presentation is supported.
    /// </summary>
    public bool SupportsDigitalCredentialsApi
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets supported JOSE signing algorithms.
    /// </summary>
    public ISet<string> SupportedJoseAlgorithms { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets supported COSE signing algorithms.
    /// </summary>
    public ISet<int> SupportedCoseAlgorithms { get; } = new HashSet<int>();

    /// <summary>
    /// Gets or sets supported hash algorithms.
    /// </summary>
    public ISet<string> SupportedHashAlgorithms { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets supported credential format identifiers.
    /// </summary>
    public ISet<string> SupportedCredentialFormats { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets a value indicating whether SD-JWT VC compact serialization is supported.
    /// </summary>
    public bool SupportsSdJwtVcCompactSerialization
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether SD-JWT VC holder binding uses cnf.jwk when required.
    /// </summary>
    public bool UsesCnfJwkForSdJwtVcHolderBinding
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether KB-JWT is required for holder-bound SD-JWT VC presentations.
    /// </summary>
    public bool RequiresKbJwtForHolderBoundSdJwtVc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether SD-JWT VC status uses Token Status List status_list.
    /// </summary>
    public bool SupportsStatusListClaim
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether SD-JWT VC issuer key resolution supports x5c.
    /// </summary>
    public bool SupportsSdJwtVcIssuerX5c
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether mdoc device signature validation is supported.
    /// </summary>
    public bool ValidatesMdocDeviceSignature
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether mdoc x5chain trust validation is supported.
    /// </summary>
    public bool ValidatesMdocX5Chain
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether mdoc MSO revocation checking is supported per ISO 18013-5.
    /// </summary>
    public bool SupportsMdocMsoRevocation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether OID4VP response encryption is supported
    /// using ECDH-ES with P-256 and A128CBC-HS256 per HAIP 1.0 Final Section 5.
    /// </summary>
    public bool SupportsResponseEncryption
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether AKI-based trusted_authorities verification
    /// is supported per HAIP 1.0 Final Section 5.
    /// </summary>
    public bool SupportsAkiTrustedAuthorities
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the FAPI2 Security Profile iss authorization
    /// response parameter is validated per HAIP 1.0 Final Section 4.
    /// </summary>
    public bool ValidatesFapi2IssAuthorizationResponse
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the direct_post.jwt response mode is supported
    /// per HAIP 1.0 Final Section 5 for OID4VP redirect flow.
    /// </summary>
    public bool SupportsDirectPostJwtResponseMode
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the dc_api.jwt response mode is supported
    /// per HAIP 1.0 Final Section 5 for the DC API flow.
    /// </summary>
    public bool SupportsDcApiJwtResponseMode
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether JAR (JWT Authorization Request) with request_uri
    /// is enforced per HAIP 1.0 Final Section 5 for OID4VP redirect flow.
    /// </summary>
    public bool SupportsJarRequestUri
    {
        get; set;
    }
}
