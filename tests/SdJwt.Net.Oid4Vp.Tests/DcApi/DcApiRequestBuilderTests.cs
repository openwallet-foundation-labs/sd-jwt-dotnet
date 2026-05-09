using FluentAssertions;
using SdJwt.Net.Oid4Vp.DcApi;
using SdJwt.Net.Oid4Vp.DcApi.Models;
using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.DcApi;

/// <summary>
/// Tests for DcApiRequestBuilder following TDD methodology.
/// </summary>
public class DcApiRequestBuilderTests
{
    [Fact]
    public void BuildUnsigned_WithRequiredParameters_ReturnsValidRequest()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithNonce("test-nonce-123")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        request.Digital.Requests.Should().ContainSingle();
        request.Digital.Requests[0].Protocol.Should().Be(DcApiConstants.Protocols.OpenId4VpV1Unsigned);
        var data = request.Digital.Requests[0].Data.Should().BeOfType<DcApiAuthorizationRequest>().Subject;
        data.ClientId.Should().BeNull();
        data.Nonce.Should().Be("test-nonce-123");
    }

    [Fact]
    public void BuildSigned_WithoutClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var act = () => builder
            .AsSignedRequest()
            .WithNonce("test-nonce")
            .WithExpectedOrigins("https://verifier.example.com")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ClientId*");
    }

    [Fact]
    public void BuildSigned_WithoutExpectedOrigins_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var act = () => builder
            .AsSignedRequest()
            .WithClientId("web-origin:https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ExpectedOrigins*");
    }

    [Fact]
    public void Build_WithoutNonce_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var act = () => builder
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Nonce*");
    }

    [Fact]
    public void Build_WithoutPresentationDefinition_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();

        // Act
        var act = () => builder
            .WithNonce("test-nonce")
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PresentationDefinition*");
    }

    [Fact]
    public void Build_WithDcApiResponseMode_SetsCorrectResponseMode()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .WithResponseMode(DcApiResponseMode.DcApi)
            .Build();

        // Assert
        var data = request.Digital.Requests[0].Data.Should().BeOfType<DcApiAuthorizationRequest>().Subject;
        data.ResponseMode.Should().Be(DcApiConstants.ResponseModes.DcApi);
    }

    [Fact]
    public void Build_WithDcApiJwtResponseMode_SetsCorrectResponseMode()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .WithResponseMode(DcApiResponseMode.DcApiJwt)
            .Build();

        // Assert
        var data = request.Digital.Requests[0].Data.Should().BeOfType<DcApiAuthorizationRequest>().Subject;
        data.ResponseMode.Should().Be(DcApiConstants.ResponseModes.DcApiJwt);
    }

    [Fact]
    public void Build_DefaultResponseMode_IsDcApi()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        var data = request.Digital.Requests[0].Data.Should().BeOfType<DcApiAuthorizationRequest>().Subject;
        data.ResponseMode.Should().Be(DcApiConstants.ResponseModes.DcApi);
    }

    [Fact]
    public void BuildSigned_DefaultClientIdScheme_IsWebOrigin()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .AsSignedRequest()
            .WithClientId("https://verifier.example.com")
            .WithExpectedOrigins("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        var data = request.Digital.Requests[0].Data.Should().BeOfType<DcApiAuthorizationRequest>().Subject;
        data.ClientIdScheme.Should().Be(DcApiConstants.WebOriginScheme);
    }

    [Fact]
    public void WithClientIdScheme_SetsCustomScheme()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .AsSignedRequest()
            .WithClientId("https://verifier.example.com")
            .WithClientIdScheme("x509_san_dns")
            .WithExpectedOrigins("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        var data = request.Digital.Requests[0].Data.Should().BeOfType<DcApiAuthorizationRequest>().Subject;
        data.ClientIdScheme.Should().Be("x509_san_dns");
    }

    [Fact]
    public void ToNavigatorCredentialsPayload_ReturnsCurrentDigitalCredentialJson()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        var request = builder
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Act
        var payload = request.ToNavigatorCredentialsPayload();

        // Assert
        payload.Should().NotBeNullOrEmpty();
        payload.Should().Contain("\"digital\":{\"requests\"");
        payload.Should().Contain("\"protocol\":\"openid4vp-v1-unsigned\"");
        payload.Should().Contain("\"data\"");
        payload.Should().NotContain("\"providers\"");
        payload.Should().NotContain("\"request\"");
    }

    [Fact]
    public void BuildSigned_WithExpectedOrigins_SetsSignedProtocolAndExpectedOrigins()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .AsSignedRequest()
            .WithClientId("web-origin:https://verifier.example.com")
            .WithExpectedOrigins("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        request.Digital.Requests[0].Protocol.Should().Be(DcApiConstants.Protocols.OpenId4VpV1Signed);
        var data = request.Digital.Requests[0].Data.Should().BeOfType<DcApiAuthorizationRequest>().Subject;
        data.ClientId.Should().Be("web-origin:https://verifier.example.com");
        data.ExpectedOrigins.Should().BeEquivalentTo("https://verifier.example.com");
    }

    private static PresentationDefinition CreateTestPresentationDefinition()
    {
        return new PresentationDefinition
        {
            Id = "test-definition",
            InputDescriptors = new[]
            {
                new InputDescriptor
                {
                    Id = "identity_credential",
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field { Path = new[] { "$.given_name" } }
                        }
                    }
                }
            }
        };
    }
}
