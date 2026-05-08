using FluentAssertions;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Models;

public class HaipRequirementCatalogTests
{
    [Fact]
    public void Requirements_ShouldIncludeAllFinalFlowsAndCredentialProfiles()
    {
        var requirements = HaipRequirementCatalog.Requirements;

        requirements.Should().Contain(r => r.Flow == HaipFlow.Oid4VciIssuance);
        requirements.Should().Contain(r => r.Flow == HaipFlow.Oid4VpRedirectPresentation);
        requirements.Should().Contain(r => r.Flow == HaipFlow.Oid4VpDigitalCredentialsApiPresentation);
        requirements.Should().Contain(r => r.CredentialProfile == HaipCredentialProfile.SdJwtVc);
        requirements.Should().Contain(r => r.CredentialProfile == HaipCredentialProfile.MsoMdoc);
    }

    [Fact]
    public void GetRequirements_WithSelectedSdJwtVcIssuanceProfile_ShouldReturnMatchingCommonFlowAndProfileRequirements()
    {
        var options = new HaipProfileOptions();
        options.Flows.Add(HaipFlow.Oid4VciIssuance);
        options.CredentialProfiles.Add(HaipCredentialProfile.SdJwtVc);

        var requirements = HaipRequirementCatalog.GetRequirements(options);

        requirements.Should().Contain(r => r.Scope == HaipRequirementScope.Common);
        requirements.Should().Contain(r => r.Flow == HaipFlow.Oid4VciIssuance);
        requirements.Should().Contain(r => r.CredentialProfile == HaipCredentialProfile.SdJwtVc);
        requirements.Should().NotContain(r => r.Flow == HaipFlow.Oid4VpRedirectPresentation);
        requirements.Should().NotContain(r => r.CredentialProfile == HaipCredentialProfile.MsoMdoc);
    }

    [Fact]
    public void GetRequirements_WithNullOptions_ShouldThrowArgumentNullException()
    {
        var act = () => HaipRequirementCatalog.GetRequirements(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }
}
