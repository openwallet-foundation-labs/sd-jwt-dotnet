using FluentAssertions;
using SdJwt.Net.Mdoc.Handover;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Handover;

/// <summary>
/// Tests for SessionTranscript structure for OpenID4VP mdoc presentations.
/// </summary>
public class SessionTranscriptTests
{
    [Fact]
    public void ForOpenId4Vp_CreatesValidTranscript()
    {
        // Arrange
        var clientId = "https://verifier.example.com";
        var nonce = "n-0S6_WzA2Mj";
        var responseUri = "https://verifier.example.com/callback";

        // Act
        var transcript = SessionTranscript.ForOpenId4Vp(clientId, nonce, null, responseUri);

        // Assert
        transcript.Should().NotBeNull();
        transcript.DeviceEngagement.Should().BeNull();
        transcript.EReaderKeyPub.Should().BeNull();
        transcript.Handover.Should().NotBeNull();
        transcript.Handover.Should().BeOfType<OpenId4VpHandover>();
    }

    [Fact]
    public void ForOpenId4Vp_WithMdocGeneratedNonce_IncludesInHandover()
    {
        // Arrange
        var clientId = "https://verifier.example.com";
        var nonce = "n-0S6_WzA2Mj";
        var mdocGeneratedNonce = "mdoc-generated-nonce";
        var responseUri = "https://verifier.example.com/callback";

        // Act
        var transcript = SessionTranscript.ForOpenId4Vp(clientId, nonce, mdocGeneratedNonce, responseUri);

        // Assert
        transcript.Handover.Should().BeOfType<OpenId4VpHandover>();
        var handover = (OpenId4VpHandover)transcript.Handover!;
        handover.MdocGeneratedNonce.Should().Be(mdocGeneratedNonce);
    }

    [Fact]
    public void ForOpenId4VpDcApi_CreatesValidTranscript()
    {
        // Arrange
        var origin = "https://verifier.example.com";
        var nonce = "n-0S6_WzA2Mj";

        // Act
        var transcript = SessionTranscript.ForOpenId4VpDcApi(origin, nonce, null);

        // Assert
        transcript.Should().NotBeNull();
        transcript.DeviceEngagement.Should().BeNull();
        transcript.EReaderKeyPub.Should().BeNull();
        transcript.Handover.Should().BeOfType<OpenId4VpDcApiHandover>();
    }

    [Fact]
    public void ToCbor_ReturnsValidCborArray()
    {
        // Arrange
        var transcript = SessionTranscript.ForOpenId4Vp(
            "client123",
            "nonce456",
            null,
            "https://response.uri");

        // Act
        var cborBytes = transcript.ToCbor();

        // Assert
        cborBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromCbor_WithValidData_RestoresTranscript()
    {
        // Arrange
        var original = SessionTranscript.ForOpenId4Vp(
            "client123",
            "nonce456",
            null,
            "https://response.uri");

        var cborBytes = original.ToCbor();

        // Act
        var restored = SessionTranscript.FromCbor(cborBytes);

        // Assert
        restored.Should().NotBeNull();
        restored.DeviceEngagement.Should().BeNull();
        restored.EReaderKeyPub.Should().BeNull();
    }

    [Fact]
    public void Constructor_CreatesEmptyTranscript()
    {
        // Act
        var transcript = new SessionTranscript();

        // Assert
        transcript.DeviceEngagement.Should().BeNull();
        transcript.EReaderKeyPub.Should().BeNull();
        transcript.Handover.Should().BeNull();
    }

    [Fact]
    public void ToCborObject_ReturnsNonNull()
    {
        // Arrange
        var transcript = SessionTranscript.ForOpenId4Vp(
            "client123",
            "nonce456",
            null,
            "https://response.uri");

        // Act
        var cborObject = transcript.ToCborObject();

        // Assert
        cborObject.Should().NotBeNull();
    }

    [Fact]
    public void DeviceEngagement_WhenSet_CanBeRetrieved()
    {
        // Arrange
        var transcript = new SessionTranscript
        {
            DeviceEngagement = new byte[] { 0x01, 0x02, 0x03 }
        };

        // Assert
        transcript.DeviceEngagement.Should().BeEquivalentTo(new byte[] { 0x01, 0x02, 0x03 });
    }

    [Fact]
    public void EReaderKeyPub_WhenSet_CanBeRetrieved()
    {
        // Arrange
        var transcript = new SessionTranscript
        {
            EReaderKeyPub = new byte[] { 0x04, 0x05, 0x06 }
        };

        // Assert
        transcript.EReaderKeyPub.Should().BeEquivalentTo(new byte[] { 0x04, 0x05, 0x06 });
    }
}
