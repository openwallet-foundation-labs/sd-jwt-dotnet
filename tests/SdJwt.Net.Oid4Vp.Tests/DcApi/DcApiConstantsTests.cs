using FluentAssertions;
using SdJwt.Net.Oid4Vp.DcApi;
using SdJwt.Net.Oid4Vp.DcApi.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.DcApi;

/// <summary>
/// Tests for DC API constants and models.
/// </summary>
public class DcApiConstantsTests
{
    #region Protocol Constants

    [Fact]
    public void Protocol_IsOpenId4Vp()
    {
        DcApiConstants.Protocol.Should().Be("openid4vp");
    }

    [Fact]
    public void WebOriginScheme_IsCorrect()
    {
        DcApiConstants.WebOriginScheme.Should().Be("web-origin");
    }

    [Fact]
    public void CredentialType_IsDigital()
    {
        DcApiConstants.CredentialType.Should().Be("digital");
    }

    #endregion

    #region Response Mode Constants

    [Fact]
    public void ResponseModes_DcApi_IsCorrect()
    {
        DcApiConstants.ResponseModes.DcApi.Should().Be("dc_api");
    }

    [Fact]
    public void ResponseModes_DcApiJwt_IsCorrect()
    {
        DcApiConstants.ResponseModes.DcApiJwt.Should().Be("dc_api.jwt");
    }

    #endregion

    #region Response Mode Enum Tests

    [Fact]
    public void DcApiResponseMode_DcApi_Exists()
    {
        DcApiResponseMode.DcApi.Should().BeDefined();
    }

    [Fact]
    public void DcApiResponseMode_DcApiJwt_Exists()
    {
        DcApiResponseMode.DcApiJwt.Should().BeDefined();
    }

    #endregion
}
