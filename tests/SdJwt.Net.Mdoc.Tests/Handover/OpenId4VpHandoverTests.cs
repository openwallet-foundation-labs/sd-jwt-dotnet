using FluentAssertions;
using SdJwt.Net.Mdoc.Handover;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Handover;

/// <summary>
/// Tests for OpenID4VP Handover structure per OpenID4VP Appendix B.2.
/// </summary>
public class OpenId4VpHandoverTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsHandover()
    {
        // Arrange
        var clientId = "https://verifier.example.com";
        var responseUri = "https://verifier.example.com/callback";
        var nonce = "n-0S6_WzA2Mj";
        var mdocGeneratedNonce = "mdoc-nonce-123";

        // Act
        var handover = OpenId4VpHandover.Create(clientId, responseUri, nonce, mdocGeneratedNonce);

        // Assert
        handover.Should().NotBeNull();
        handover.ClientId.Should().Be(clientId);
        handover.ResponseUri.Should().Be(responseUri);
        handover.Nonce.Should().Be(nonce);
        handover.MdocGeneratedNonce.Should().Be(mdocGeneratedNonce);
    }

    [Fact]
    public void Create_WithEmptyMdocGeneratedNonce_CreatesHandover()
    {
        // Arrange
        var clientId = "https://verifier.example.com";
        var responseUri = "https://verifier.example.com/callback";
        var nonce = "n-0S6_WzA2Mj";

        // Act
        var handover = OpenId4VpHandover.Create(clientId, responseUri, nonce, string.Empty);

        // Assert
        handover.Should().NotBeNull();
        handover.MdocGeneratedNonce.Should().BeEmpty();
    }

    [Fact]
    public void ToCbor_ReturnsValidCborArray()
    {
        // Arrange
        var handover = OpenId4VpHandover.Create(
            "client123",
            "https://response.uri",
            "nonce456",
            "mdoc-nonce");

        // Act
        var cborBytes = handover.ToCbor();

        // Assert
        cborBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToCborObject_ReturnsNonNull()
    {
        // Arrange
        var handover = OpenId4VpHandover.Create(
            "client123",
            "https://response.uri",
            "nonce456",
            "mdoc-nonce");

        // Act
        var cborObject = handover.ToCborObject();

        // Assert
        cborObject.Should().NotBeNull();
    }

    [Fact]
    public void CreateSessionTranscript_ReturnsTranscriptWithHandover()
    {
        // Arrange
        var handover = OpenId4VpHandover.Create(
            "client123",
            "https://response.uri",
            "nonce456",
            "mdoc-nonce");

        // Act
        var transcript = handover.CreateSessionTranscript();

        // Assert
        transcript.Should().NotBeNull();
        transcript.Handover.Should().BeSameAs(handover);
        transcript.DeviceEngagement.Should().BeNull();
        transcript.EReaderKeyPub.Should().BeNull();
    }

    [Fact]
    public void Properties_DefaultValues_AreEmpty()
    {
        // Act
        var handover = new OpenId4VpHandover();

        // Assert
        handover.ClientId.Should().BeEmpty();
        handover.ResponseUri.Should().BeEmpty();
        handover.Nonce.Should().BeEmpty();
        handover.MdocGeneratedNonce.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var handover = new OpenId4VpHandover
        {
            ClientId = "test-client",
            ResponseUri = "https://test.uri",
            Nonce = "test-nonce",
            MdocGeneratedNonce = "mdoc-nonce"
        };

        // Assert
        handover.ClientId.Should().Be("test-client");
        handover.ResponseUri.Should().Be("https://test.uri");
        handover.Nonce.Should().Be("test-nonce");
        handover.MdocGeneratedNonce.Should().Be("mdoc-nonce");
    }
}
