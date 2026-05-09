using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Holder;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Models;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Holder;

/// <summary>
/// Tests for MdocPresenter holder presentation logic.
/// </summary>
public class MdocPresenterTests : TestBase
{
    [Fact]
    public async Task PresentAsync_WithDeviceRequest_SelectsRequestedElements()
    {
        // Arrange
        var document = await CreateTestDocument();
        var request = new DeviceRequest
        {
            DocRequests = new List<DocRequest>
            {
                new DocRequest
                {
                    ItemsRequest = new ItemsRequest
                    {
                        DocType = MdlDocType,
                        NameSpaces = new Dictionary<string, Dictionary<string, bool>>
                        {
                            [MdlNamespace] = new Dictionary<string, bool>
                            {
                                ["family_name"] = true,
                                ["age_over_18"] = true
                            }
                        }
                    }
                }
            }
        };
        var presenter = new MdocPresenter();

        // Act
        var response = await presenter.PresentAsync(document, request);

        // Assert
        response.Documents.Should().HaveCount(1);
        response.Documents[0].DocType.Should().Be(MdlDocType);
        var items = response.Documents[0].IssuerSigned.NameSpaces[MdlNamespace];
        items.Should().HaveCount(2);
        items.Select(i => i.ElementIdentifier).Should().Contain("family_name");
        items.Select(i => i.ElementIdentifier).Should().Contain("age_over_18");
    }

    [Fact]
    public async Task PresentAsync_WithNonMatchingDocType_ReturnsDocumentError()
    {
        // Arrange
        var document = await CreateTestDocument();
        var request = new DeviceRequest
        {
            DocRequests = new List<DocRequest>
            {
                new DocRequest
                {
                    ItemsRequest = new ItemsRequest
                    {
                        DocType = "org.example.other",
                        NameSpaces = new Dictionary<string, Dictionary<string, bool>>
                        {
                            ["org.example.other"] = new Dictionary<string, bool>
                            {
                                ["name"] = true
                            }
                        }
                    }
                }
            }
        };
        var presenter = new MdocPresenter();

        // Act
        var response = await presenter.PresentAsync(document, request);

        // Assert
        response.Documents.Should().BeEmpty();
        response.DocumentErrors.Should().HaveCount(1);
        response.DocumentErrors![0].DocType.Should().Be("org.example.other");
    }

    [Fact]
    public async Task PresentAsync_WithManualSelection_SelectsSpecifiedElements()
    {
        // Arrange
        var document = await CreateTestDocument();
        var disclosedElements = new Dictionary<string, List<string>>
        {
            [MdlNamespace] = new List<string> { "given_name" }
        };
        var presenter = new MdocPresenter();

        // Act
        var response = await presenter.PresentAsync(document, disclosedElements);

        // Assert
        response.Documents.Should().HaveCount(1);
        var items = response.Documents[0].IssuerSigned.NameSpaces[MdlNamespace];
        items.Should().HaveCount(1);
        items[0].ElementIdentifier.Should().Be("given_name");
    }

    [Fact]
    public async Task PresentAsync_WithDeviceKey_CreatesDeviceSignature()
    {
        // Arrange
        var document = await CreateTestDocument();
        var request = new DeviceRequest
        {
            DocRequests = new List<DocRequest>
            {
                new DocRequest
                {
                    ItemsRequest = new ItemsRequest
                    {
                        DocType = MdlDocType,
                        NameSpaces = new Dictionary<string, Dictionary<string, bool>>
                        {
                            [MdlNamespace] = new Dictionary<string, bool>
                            {
                                ["family_name"] = true
                            }
                        }
                    }
                }
            }
        };
        var transcript = SessionTranscript.ForOpenId4Vp(
            "client123", "nonce456", null, "https://verifier.example.com/cb");
        var presenter = new MdocPresenter();

        // Act
        var response = await presenter.PresentAsync(
            document, request, DeviceKey, CoseAlgorithm.ES256, transcript);

        // Assert
        response.Documents.Should().HaveCount(1);
        response.Documents[0].DeviceSigned.Should().NotBeNull();
        response.Documents[0].DeviceSigned!.DeviceAuth.Should().NotBeNull();
        response.Documents[0].DeviceSigned!.DeviceAuth!.DeviceSignature.Should().NotBeNull();
    }

    [Fact]
    public async Task PresentAsync_WithNullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var presenter = new MdocPresenter();
        var request = new DeviceRequest();

        // Act
        var act = () => presenter.PresentAsync(null!, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PresentAsync_PreservesIssuerAuth()
    {
        // Arrange
        var document = await CreateTestDocument();
        var disclosedElements = new Dictionary<string, List<string>>
        {
            [MdlNamespace] = new List<string> { "family_name" }
        };
        var presenter = new MdocPresenter();

        // Act
        var response = await presenter.PresentAsync(document, disclosedElements);

        // Assert
        response.Documents[0].IssuerSigned.IssuerAuth
            .Should().BeEquivalentTo(document.IssuerSigned.IssuerAuth);
    }

    private async Task<Document> CreateTestDocument()
    {
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);
        return await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .AddClaim(MdlNamespace, "given_name", "John")
            .AddClaim(MdlNamespace, "age_over_18", true)
            .AddClaim(MdlNamespace, "birth_date", "1990-01-15")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());
    }
}
