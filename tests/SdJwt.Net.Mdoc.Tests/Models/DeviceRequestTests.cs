using FluentAssertions;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Models;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Models;

/// <summary>
/// Tests for DeviceRequest, DocRequest, and ItemsRequest models.
/// </summary>
public class DeviceRequestTests
{
    [Fact]
    public void ToCbor_WithValidDeviceRequest_RoundTrips()
    {
        // Arrange
        var request = new DeviceRequest
        {
            DocRequests = new List<DocRequest>
            {
                new DocRequest
                {
                    ItemsRequest = new ItemsRequest
                    {
                        DocType = "org.iso.18013.5.1.mDL",
                        NameSpaces = new Dictionary<string, Dictionary<string, bool>>
                        {
                            ["org.iso.18013.5.1"] = new Dictionary<string, bool>
                            {
                                ["family_name"] = true,
                                ["given_name"] = true,
                                ["age_over_18"] = false
                            }
                        }
                    }
                }
            }
        };

        // Act
        var cbor = request.ToCbor();
        var parsed = DeviceRequest.FromCbor(cbor);

        // Assert
        parsed.Version.Should().Be("1.0");
        parsed.DocRequests.Should().HaveCount(1);
        parsed.DocRequests[0].ItemsRequest.DocType.Should().Be("org.iso.18013.5.1.mDL");
        parsed.DocRequests[0].ItemsRequest.NameSpaces.Should().ContainKey("org.iso.18013.5.1");
        parsed.DocRequests[0].ItemsRequest.NameSpaces["org.iso.18013.5.1"].Should().HaveCount(3);
    }

    [Fact]
    public void ToCbor_WithReaderAuth_PreservesReaderAuthBytes()
    {
        // Arrange - ReaderAuth is a COSE_Sign1 structure, use valid CBOR for testing
        var readerAuthBytes = PeterO.Cbor.CBORObject.NewArray()
            .Add(PeterO.Cbor.CBORObject.FromObject(new byte[] { 0x01 }))
            .Add(PeterO.Cbor.CBORObject.NewMap())
            .Add(PeterO.Cbor.CBORObject.Null)
            .Add(PeterO.Cbor.CBORObject.FromObject(new byte[] { 0x02, 0x03 }))
            .EncodeToBytes();
        var request = new DeviceRequest
        {
            DocRequests = new List<DocRequest>
            {
                new DocRequest
                {
                    ItemsRequest = new ItemsRequest
                    {
                        DocType = "org.iso.18013.5.1.mDL",
                        NameSpaces = new Dictionary<string, Dictionary<string, bool>>
                        {
                            ["org.iso.18013.5.1"] = new Dictionary<string, bool>
                            {
                                ["family_name"] = true
                            }
                        }
                    },
                    ReaderAuth = readerAuthBytes
                }
            }
        };

        // Act
        var cbor = request.ToCbor();
        var parsed = DeviceRequest.FromCbor(cbor);

        // Assert
        parsed.DocRequests[0].ReaderAuth.Should().NotBeNull();
    }

    [Fact]
    public void FromCbor_WithEmptyDocRequests_CreatesEmptyList()
    {
        // Arrange
        var request = new DeviceRequest();

        // Act
        var cbor = request.ToCbor();
        var parsed = DeviceRequest.FromCbor(cbor);

        // Assert
        parsed.DocRequests.Should().BeEmpty();
    }

    [Fact]
    public void ItemsRequest_WithRequestInfo_RoundTrips()
    {
        // Arrange
        var itemsRequest = new ItemsRequest
        {
            DocType = "org.iso.18013.5.1.mDL",
            NameSpaces = new Dictionary<string, Dictionary<string, bool>>
            {
                ["org.iso.18013.5.1"] = new Dictionary<string, bool>
                {
                    ["family_name"] = true
                }
            },
            RequestInfo = new Dictionary<string, object>
            {
                ["purpose"] = "age_verification"
            }
        };

        // Act
        var cbor = itemsRequest.ToCbor();
        var parsed = ItemsRequest.FromCborObject(PeterO.Cbor.CBORObject.DecodeFromBytes(cbor));

        // Assert
        parsed.DocType.Should().Be("org.iso.18013.5.1.mDL");
        parsed.NameSpaces.Should().ContainKey("org.iso.18013.5.1");
    }

    [Fact]
    public void ToCbor_WithMultipleDocRequests_PreservesAll()
    {
        // Arrange
        var request = new DeviceRequest
        {
            DocRequests = new List<DocRequest>
            {
                new DocRequest
                {
                    ItemsRequest = new ItemsRequest
                    {
                        DocType = "org.iso.18013.5.1.mDL",
                        NameSpaces = new Dictionary<string, Dictionary<string, bool>>
                        {
                            ["org.iso.18013.5.1"] = new Dictionary<string, bool>
                            {
                                ["family_name"] = true
                            }
                        }
                    }
                },
                new DocRequest
                {
                    ItemsRequest = new ItemsRequest
                    {
                        DocType = "org.example.pid",
                        NameSpaces = new Dictionary<string, Dictionary<string, bool>>
                        {
                            ["org.example.pid"] = new Dictionary<string, bool>
                            {
                                ["name"] = true
                            }
                        }
                    }
                }
            }
        };

        // Act
        var cbor = request.ToCbor();
        var parsed = DeviceRequest.FromCbor(cbor);

        // Assert
        parsed.DocRequests.Should().HaveCount(2);
        parsed.DocRequests[0].ItemsRequest.DocType.Should().Be("org.iso.18013.5.1.mDL");
        parsed.DocRequests[1].ItemsRequest.DocType.Should().Be("org.example.pid");
    }
}
