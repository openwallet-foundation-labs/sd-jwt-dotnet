using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.HAIP.Models;

namespace SdJwt.Net.HAIP.Validators;

/// <summary>
/// Validates declared implementation capabilities against OpenID4VC HAIP 1.0 Final.
/// </summary>
public sealed class HaipProfileValidator
{
    private readonly ILogger<HaipProfileValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HaipProfileValidator"/> class.
    /// </summary>
    /// <param name="logger">Optional logger used for diagnostics.</param>
    public HaipProfileValidator(ILogger<HaipProfileValidator>? logger = null)
    {
        _logger = logger ?? NullLogger<HaipProfileValidator>.Instance;
    }

    /// <summary>
    /// Validates the provided options against HAIP 1.0 Final requirements.
    /// </summary>
    /// <param name="options">The implementation capabilities and policy switches to validate.</param>
    /// <returns>The HAIP compliance validation result.</returns>
    public HaipComplianceResult Validate(HaipProfileOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var result = new HaipComplianceResult
        {
            IsCompliant = true
        };
        result.AuditTrail.ValidatorId = nameof(HaipProfileValidator);
        result.AuditTrail.HaipVersion = HaipConstants.FinalSpecificationVersion;
        result.AuditTrail.AddStep("Starting HAIP Final profile validation", true);
        result.Metadata = new Dictionary<string, object>
        {
            ["applicable_requirements"] = HaipRequirementCatalog.GetRequirements(options)
                .Select(requirement => requirement.Id)
                .ToArray()
        };

        ValidateScope(options, result);
        ValidateCryptography(options, result);

        if (options.Flows.Contains(HaipFlow.Oid4VciIssuance))
        {
            ValidateOid4Vci(options, result);
        }

        if (options.Flows.Contains(HaipFlow.Oid4VpRedirectPresentation))
        {
            ValidateOid4VpRedirect(options, result);
        }

        if (options.Flows.Contains(HaipFlow.Oid4VpDigitalCredentialsApiPresentation))
        {
            ValidateOid4VpDigitalCredentialsApi(options, result);
        }

        if (options.CredentialProfiles.Contains(HaipCredentialProfile.SdJwtVc))
        {
            ValidateSdJwtVcProfile(options, result);
        }

        if (options.CredentialProfiles.Contains(HaipCredentialProfile.MsoMdoc))
        {
            ValidateMdocProfile(options, result);
        }

        result.IsCompliant = !result.Violations.Any(v => v.Severity == HaipSeverity.Critical);
        result.AuditTrail.Complete();

        _logger.LogInformation(
            "HAIP Final profile validation completed. Compliant: {IsCompliant}, Violations: {ViolationCount}",
            result.IsCompliant,
            result.Violations.Count);

        return result;
    }

    private static void ValidateScope(HaipProfileOptions options, HaipComplianceResult result)
    {
        if (options.Flows.Count == 0)
        {
            AddCritical(result, "At least one HAIP flow must be selected.", "Select OID4VCI, OID4VP redirect, or OID4VP DC API flow.");
        }

        if (options.CredentialProfiles.Count == 0)
        {
            AddCritical(result, "At least one HAIP credential profile must be selected.", "Select SD-JWT VC, ISO mdoc, or both.");
        }
    }

    private static void ValidateCryptography(HaipProfileOptions options, HaipComplianceResult result)
    {
        if (!options.SupportedJoseAlgorithms.Contains(HaipConstants.RequiredJoseAlgorithm))
        {
            AddWeakCrypto(result, "HAIP Final requires support for JOSE ES256 validation.");
        }

        if (!options.SupportedHashAlgorithms.Contains(HaipConstants.RequiredHashAlgorithm))
        {
            AddWeakCrypto(result, "HAIP Final requires SHA-256 digest generation and validation support.");
        }
    }

    private static void ValidateOid4Vci(HaipProfileOptions options, HaipComplianceResult result)
    {
        Require(options.SupportsAuthorizationCodeFlow, result, "OID4VCI HAIP requires authorization code flow support.");
        Require(options.EnforcesPkceS256, result, "OID4VCI HAIP requires PKCE with S256.");
        Require(options.SupportsDpop, result, "OID4VCI HAIP requires DPoP sender-constrained access token support.");
        Require(options.SupportsDpopNonce, result, "OID4VCI HAIP requires DPoP nonce handling.");

        if (options.UsesAuthorizationEndpoint)
        {
            Require(options.SupportsPushedAuthorizationRequests, result, "OID4VCI HAIP requires PAR where the Authorization Endpoint is used.");
        }

        if (options.SupportsKeyBoundCredentialConfigurations)
        {
            Require(
                options.PublishesNonceEndpointForKeyBinding,
                result,
                "OID4VCI HAIP requires nonce_endpoint metadata when key-bound credential configurations are advertised.");
        }

        Require(options.ValidatesWalletAttestation, result, "OID4VCI HAIP requires cryptographic Wallet Attestation validation when Wallet Attestation is used.");
        Require(options.ValidatesKeyAttestation, result, "OID4VCI HAIP requires cryptographic Key Attestation validation when Key Attestation is used.");
    }

    private static void ValidateOid4VpRedirect(HaipProfileOptions options, HaipComplianceResult result)
    {
        Require(options.SupportsDcql, result, "OID4VP HAIP requires DCQL support for credential queries.");
        Require(options.SupportsSignedPresentationRequests, result, "OID4VP HAIP requires signed presentation request support where signed requests are used.");
        Require(options.ValidatesVerifierAttestation, result, "OID4VP HAIP requires verifier attestation validation when verifier attestation is used.");
    }

    private static void ValidateOid4VpDigitalCredentialsApi(HaipProfileOptions options, HaipComplianceResult result)
    {
        Require(options.SupportsDigitalCredentialsApi, result, "HAIP DC API flow requires W3C Digital Credentials API support.");
        Require(options.SupportsDcql, result, "HAIP DC API flow requires DCQL support.");
    }

    private static void ValidateSdJwtVcProfile(HaipProfileOptions options, HaipComplianceResult result)
    {
        Require(
            options.SupportedCredentialFormats.Contains(HaipConstants.SdJwtVcFormat),
            result,
            "HAIP SD-JWT VC profile requires credential format identifier dc+sd-jwt.");
        Require(options.SupportsSdJwtVcCompactSerialization, result, "HAIP SD-JWT VC profile requires compact serialization support.");
        Require(options.UsesCnfJwkForSdJwtVcHolderBinding, result, "HAIP SD-JWT VC profile requires cnf.jwk when holder binding is required.");
        Require(options.RequiresKbJwtForHolderBoundSdJwtVc, result, "HAIP SD-JWT VC profile requires KB-JWT on holder-bound presentations.");
        Require(options.SupportsStatusListClaim, result, "HAIP SD-JWT VC profile requires status.status_list support when credential status is present.");
        Require(options.SupportsSdJwtVcIssuerX5c, result, "HAIP SD-JWT VC profile requires x5c issuer key resolution support.");
    }

    private static void ValidateMdocProfile(HaipProfileOptions options, HaipComplianceResult result)
    {
        Require(
            options.SupportedCredentialFormats.Contains(HaipConstants.MsoMdocFormat),
            result,
            "HAIP mdoc profile requires credential format identifier mso_mdoc.");
        Require(
            options.SupportedCoseAlgorithms.Any(alg => HaipConstants.RequiredCoseAlgorithms.Contains(alg)),
            result,
            "HAIP mdoc profile requires support for COSE ES256 validation.");
        Require(options.ValidatesMdocDeviceSignature, result, "HAIP mdoc profile requires device signature validation.");
        Require(options.ValidatesMdocX5Chain, result, "HAIP mdoc profile requires mdoc x5chain trust validation where x5chain is used.");
    }

    private static void Require(bool condition, HaipComplianceResult result, string description)
    {
        if (!condition)
        {
            AddCritical(result, description, "Implement and enable this HAIP Final requirement for the selected flow/profile.");
        }
    }

    private static void AddWeakCrypto(HaipComplianceResult result, string description)
    {
        result.AddViolation(description, HaipViolationType.WeakCryptography, HaipSeverity.Critical);
    }

    private static void AddCritical(HaipComplianceResult result, string description, string recommendation)
    {
        result.AddViolation(description, HaipViolationType.InsufficientAssuranceLevel, HaipSeverity.Critical, recommendation);
    }
}
