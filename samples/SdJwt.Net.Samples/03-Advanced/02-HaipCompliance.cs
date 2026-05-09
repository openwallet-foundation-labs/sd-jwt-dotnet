using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Samples.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.Advanced;

/// <summary>
/// Tutorial 02: HAIP Compliance.
/// </summary>
public static class HaipCompliance
{
    /// <summary>
    /// Runs the HAIP Final sample.
    /// </summary>
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 02: HAIP Compliance (OpenID4VC HAIP 1.0 Final)");

        Console.WriteLine("HAIP Final is a flow and credential-profile interoperability profile.");
        Console.WriteLine("It does not define Level 1, Level 2, or Level 3 compliance tiers.");
        Console.WriteLine("Those names remain in SdJwt.Net.HAIP only as legacy policy helpers.");
        Console.WriteLine();

        PrintCatalog();
        ValidateSdJwtVcIssuanceAndPresentation();
        ValidateMdocDigitalCredentialsApi();
        ShowFailClosedValidation();
        IssueExampleSdJwtVc();

        ConsoleHelpers.PrintCompletion("Tutorial 02: HAIP Compliance", new[]
        {
            "Selected HAIP Final flows and credential profiles",
            "Validated OID4VCI and OID4VP SD-JWT VC capability declarations",
            "Validated an ISO mdoc Digital Credentials API capability declaration",
            "Observed fail-closed reporting when required capabilities are missing",
            "Issued an ES256 SD-JWT credential aligned with HAIP Final minimum JOSE support"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 03 - Multi-Credential Flows");

        return Task.CompletedTask;
    }

    private static void PrintCatalog()
    {
        ConsoleHelpers.PrintStep(1, "Requirement catalog");

        Console.WriteLine("The package exposes a machine-readable HAIP Final requirement catalog:");
        foreach (var requirement in HaipRequirementCatalog.Requirements.Take(8))
        {
            Console.WriteLine($"  - {requirement.Id}: {requirement.Title}");
        }

        Console.WriteLine($"  ... {HaipRequirementCatalog.Requirements.Count} requirements tracked");
        Console.WriteLine();
    }

    private static void ValidateSdJwtVcIssuanceAndPresentation()
    {
        ConsoleHelpers.PrintStep(2, "SD-JWT VC issuance and presentation profile");

        var options = CreateSdJwtVcOptions();
        var result = new HaipProfileValidator().Validate(options);

        PrintValidationResult(result);
    }

    private static void ValidateMdocDigitalCredentialsApi()
    {
        ConsoleHelpers.PrintStep(3, "ISO mdoc Digital Credentials API profile");

        var options = new HaipProfileOptions();
        options.Flows.Add(HaipFlow.Oid4VpDigitalCredentialsApiPresentation);
        options.CredentialProfiles.Add(HaipCredentialProfile.MsoMdoc);
        options.SupportedCredentialFormats.Add(HaipConstants.MsoMdocFormat);
        options.SupportedJoseAlgorithms.Add(HaipConstants.RequiredJoseAlgorithm);
        options.SupportedCoseAlgorithms.Add(-7);
        options.SupportedHashAlgorithms.Add(HaipConstants.RequiredHashAlgorithm);
        options.SupportsDigitalCredentialsApi = true;
        options.SupportsDcql = true;
        options.ValidatesMdocDeviceSignature = true;
        options.ValidatesMdocX5Chain = true;

        var result = new HaipProfileValidator().Validate(options);

        PrintValidationResult(result);
    }

    private static void ShowFailClosedValidation()
    {
        ConsoleHelpers.PrintStep(4, "Fail-closed validation");

        var options = CreateSdJwtVcOptions();
        options.SupportsDpop = false;
        options.SupportsDpopNonce = false;

        var result = new HaipProfileValidator().Validate(options);

        PrintValidationResult(result);
    }

    private static void IssueExampleSdJwtVc()
    {
        ConsoleHelpers.PrintStep(5, "ES256 SD-JWT VC issuance");

        using var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var issuerKey = new ECDsaSecurityKey(signingKey) { KeyId = "haip-final-issuer-key" };
        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://issuer.example",
            [JwtRegisteredClaimNames.Sub] = "did:example:holder",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
            ["vct"] = "https://issuer.example/credentials/identity",
            ["given_name"] = "Alice",
            ["family_name"] = "Example",
            ["birthdate"] = "1990-01-15"
        };

        var issuance = issuer.Issue(claims, new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                given_name = true,
                family_name = true,
                birthdate = true
            }
        });

        Console.WriteLine($"  JOSE algorithm: {HaipConstants.RequiredJoseAlgorithm}");
        ConsoleHelpers.PrintPreview("  SD-JWT", issuance.Issuance, 50);
    }

    private static HaipProfileOptions CreateSdJwtVcOptions()
    {
        var options = new HaipProfileOptions();
        options.Flows.Add(HaipFlow.Oid4VciIssuance);
        options.Flows.Add(HaipFlow.Oid4VpRedirectPresentation);
        options.CredentialProfiles.Add(HaipCredentialProfile.SdJwtVc);
        options.SupportedCredentialFormats.Add(HaipConstants.SdJwtVcFormat);
        options.SupportedJoseAlgorithms.Add(HaipConstants.RequiredJoseAlgorithm);
        options.SupportedHashAlgorithms.Add(HaipConstants.RequiredHashAlgorithm);
        options.SupportsAuthorizationCodeFlow = true;
        options.EnforcesPkceS256 = true;
        options.SupportsPushedAuthorizationRequests = true;
        options.SupportsDpop = true;
        options.SupportsDpopNonce = true;
        options.ValidatesWalletAttestation = true;
        options.ValidatesKeyAttestation = true;
        options.SupportsDcql = true;
        options.SupportsSignedPresentationRequests = true;
        options.ValidatesVerifierAttestation = true;
        options.SupportsSdJwtVcCompactSerialization = true;
        options.UsesCnfJwkForSdJwtVcHolderBinding = true;
        options.RequiresKbJwtForHolderBoundSdJwtVc = true;
        options.SupportsStatusListClaim = true;
        options.SupportsSdJwtVcIssuerX5c = true;

        return options;
    }

    private static void PrintValidationResult(HaipComplianceResult result)
    {
        Console.WriteLine($"  Compliant: {result.IsCompliant}");

        if (result.Metadata != null &&
            result.Metadata.TryGetValue("applicable_requirements", out var requirementIds) &&
            requirementIds is string[] ids)
        {
            Console.WriteLine($"  Applicable requirements: {ids.Length}");
        }

        foreach (var violation in result.Violations)
        {
            Console.WriteLine($"  - {violation.Description}");
        }

        Console.WriteLine();
    }
}
