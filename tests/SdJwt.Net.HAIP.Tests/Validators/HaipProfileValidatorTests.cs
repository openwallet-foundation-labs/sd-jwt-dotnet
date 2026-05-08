using FluentAssertions;
using SdJwt.Net.HAIP.Validators;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Validators;

public class HaipProfileValidatorTests
{
    [Fact]
    public void Validate_WithCompleteSdJwtVcIssuanceAndPresentationProfile_ShouldBeCompliant()
    {
        var options = CreateCompleteSdJwtVcOptions();
        var validator = new HaipProfileValidator();

        var result = validator.Validate(options);

        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.AuditTrail.HaipVersion.Should().Be(HaipConstants.FinalSpecificationVersion);
        result.Metadata.Should().ContainKey("applicable_requirements");
    }

    [Fact]
    public void Validate_WithoutSelectedFlow_ShouldFail()
    {
        var options = CreateCompleteSdJwtVcOptions();
        options.Flows.Clear();
        var validator = new HaipProfileValidator();

        var result = validator.Validate(options);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Description.Contains("flow", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithOid4VciMissingDpop_ShouldFail()
    {
        var options = CreateCompleteSdJwtVcOptions();
        options.SupportsDpop = false;
        var validator = new HaipProfileValidator();

        var result = validator.Validate(options);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Description.Contains("DPoP", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WithSdJwtVcMissingDcFormat_ShouldFail()
    {
        var options = CreateCompleteSdJwtVcOptions();
        options.SupportedCredentialFormats.Clear();
        var validator = new HaipProfileValidator();

        var result = validator.Validate(options);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Description.Contains("dc+sd-jwt", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WithCompleteMdocDigitalCredentialsApiProfile_ShouldBeCompliant()
    {
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
        var validator = new HaipProfileValidator();

        var result = validator.Validate(options);

        result.IsCompliant.Should().BeTrue();
    }

    private static HaipProfileOptions CreateCompleteSdJwtVcOptions()
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
}
