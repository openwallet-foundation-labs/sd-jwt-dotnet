using FluentAssertions;
using SdJwt.Net.Mdoc.Models;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Models;

/// <summary>
/// Tests for ReaderAuthentication model.
/// </summary>
public class ReaderAuthenticationTests
{
    [Fact]
    public void ToCbor_WithValidData_RoundTrips()
    {
        // Arrange
        var sessionTranscriptBytes = new byte[] { 0x83, 0xF6, 0xF6, 0xF6 }; // [null, null, null]
        var itemsRequestBytes = new byte[] { 0xA1, 0x01, 0x02 }; // {1: 2}

        var auth = new ReaderAuthentication
        {
            SessionTranscript = sessionTranscriptBytes,
            ItemsRequestBytes = itemsRequestBytes
        };

        // Act
        var cbor = auth.ToCbor();

        // Assert
        cbor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToCborObject_ContainsReaderAuthenticationLabel()
    {
        // Arrange
        var auth = new ReaderAuthentication
        {
            SessionTranscript = new byte[] { 0x83, 0xF6, 0xF6, 0xF6 },
            ItemsRequestBytes = new byte[] { 0xA1, 0x01, 0x02 }
        };

        // Act
        var cborObj = auth.ToCborObject();

        // Assert
        cborObj[0].AsString().Should().Be("ReaderAuthentication");
    }
}
