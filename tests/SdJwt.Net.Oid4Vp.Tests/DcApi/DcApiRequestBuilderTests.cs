using FluentAssertions;
using SdJwt.Net.Oid4Vp.DcApi;
using SdJwt.Net.Oid4Vp.DcApi.Models;
using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.DcApi;

/// <summary>
/// Tests for DcApiRequestBuilder following TDD methodology.
/// Tests are written first to define expected behavior.
/// </summary>
public class DcApiRequestBuilderTests
{
    #region Basic Builder Tests

    [Fact]
    public void Build_WithRequiredParameters_ReturnsValidRequest()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce-123")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        request.Should().NotBeNull();
        request.Protocol.Should().Be(DcApiConstants.Protocol);
        request.Request.Should().NotBeNull();
        request.Request.ClientId.Should().Be("https://verifier.example.com");
        request.Request.Nonce.Should().Be("test-nonce-123");
    }

    [Fact]
    public void Build_WithoutClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var act = () => builder
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ClientId*");
    }

    [Fact]
    public void Build_WithoutNonce_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var act = () => builder
            .WithClientId("https://verifier.example.com")
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
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce")
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PresentationDefinition*");
    }

    #endregion

    #region Response Mode Tests

    [Fact]
    public void Build_WithDcApiResponseMode_SetsCorrectResponseMode()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .WithResponseMode(DcApiResponseMode.DcApi)
            .Build();

        // Assert
        request.Request.ResponseMode.Should().Be(DcApiConstants.ResponseModes.DcApi);
    }

    [Fact]
    public void Build_WithDcApiJwtResponseMode_SetsCorrectResponseMode()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .WithResponseMode(DcApiResponseMode.DcApiJwt)
            .Build();

        // Assert
        request.Request.ResponseMode.Should().Be(DcApiConstants.ResponseModes.DcApiJwt);
    }

    [Fact]
    public void Build_DefaultResponseMode_IsDcApi()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        request.Request.ResponseMode.Should().Be(DcApiConstants.ResponseModes.DcApi);
    }

    #endregion

    #region Client ID Scheme Tests

    [Fact]
    public void Build_DefaultClientIdScheme_IsWebOrigin()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        request.Request.ClientIdScheme.Should().Be(DcApiConstants.WebOriginScheme);
    }

    [Fact]
    public void WithClientIdScheme_SetsCustomScheme()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        // Act
        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithClientIdScheme("x509_san_dns")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Assert
        request.Request.ClientIdScheme.Should().Be("x509_san_dns");
    }

    #endregion

    #region Navigator Payload Tests

    [Fact]
    public void ToNavigatorCredentialsPayload_ReturnsValidJson()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Act
        var payload = request.ToNavigatorCredentialsPayload();

        // Assert
        payload.Should().NotBeNullOrEmpty();
        payload.Should().Contain("openid4vp");
        payload.Should().Contain("https://verifier.example.com");
    }

    [Fact]
    public void ToNavigatorCredentialsPayload_ContainsProtocol()
    {
        // Arrange
        var builder = new DcApiRequestBuilder();
        var presentationDefinition = CreateTestPresentationDefinition();

        var request = builder
            .WithClientId("https://verifier.example.com")
            .WithNonce("test-nonce")
            .WithPresentationDefinition(presentationDefinition)
            .Build();

        // Act
        var payload = request.ToNavigatorCredentialsPayload();

        // Assert
        payload.Should().Contain("\"protocol\":\"openid4vp\"");
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
