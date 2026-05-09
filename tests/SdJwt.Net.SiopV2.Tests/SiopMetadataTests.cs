using FluentAssertions;
using SdJwt.Net.SiopV2;
using Xunit;

namespace SdJwt.Net.SiopV2.Tests;

public class SiopMetadataTests
{
    [Fact]
    public void CreateSiopV2Defaults_ReturnsStaticSiopConfiguration()
    {
        var metadata = SiopProviderMetadata.CreateSiopV2Defaults();

        metadata.AuthorizationEndpoint.Should().Be("siopv2:");
        metadata.ResponseTypesSupported.Should().Contain(SiopConstants.ResponseTypes.IdToken);
        metadata.SubjectSyntaxTypesSupported.Should().Contain(SiopConstants.SubjectSyntaxTypes.JwkThumbprint);
        metadata.IdTokenTypesSupported.Should().Contain(SiopConstants.IdTokenTypes.SubjectSigned);
    }

    [Fact]
    public void CreateOpenIdDefaults_ReturnsCombinedVpAndIdTokenConfiguration()
    {
        var metadata = SiopProviderMetadata.CreateOpenIdDefaults();

        metadata.AuthorizationEndpoint.Should().Be("openid:");
        metadata.ResponseTypesSupported.Should().Contain(SiopConstants.ResponseTypes.VpToken);
        metadata.ResponseTypesSupported.Should().Contain(SiopConstants.ResponseTypes.IdToken);
    }
}
