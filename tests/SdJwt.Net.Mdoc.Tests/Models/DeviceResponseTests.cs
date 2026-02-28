using FluentAssertions;
using SdJwt.Net.Mdoc.Models;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Models;

/// <summary>
/// Tests for DeviceResponse structure per ISO 18013-5 Section 8.3.2.1.2.2.
/// </summary>
public class DeviceResponseTests
{
    [Fact]
    public void Constructor_CreatesDefaultValues()
    {
        // Act
        var response = new DeviceResponse();

        // Assert
        response.Version.Should().Be("1.0");
        response.Documents.Should().NotBeNull();
        response.Documents.Should().BeEmpty();
        response.Status.Should().Be(0);
    }

    [Fact]
    public void AddDocument_IncreasesCount()
    {
        // Arrange
        var response = new DeviceResponse();
        var document = new Document { DocType = "org.iso.18013.5.1.mDL" };

        // Act
        response.Documents.Add(document);

        // Assert
        response.Documents.Should().HaveCount(1);
    }

    [Fact]
    public void Status_Zero_IndicatesSuccess()
    {
        // Arrange
        var response = new DeviceResponse { Status = 0 };

        // Assert
        response.Status.Should().Be(0);
    }

    [Fact]
    public void Status_NonZero_IndicatesError()
    {
        // Arrange
        var response = new DeviceResponse { Status = 10 };

        // Assert
        response.Status.Should().Be(10);
    }

    [Fact]
    public void DocumentErrors_WhenSet_StoresCorrectly()
    {
        // Arrange
        var response = new DeviceResponse();
        var error = new DocumentError
        {
            DocType = "org.iso.18013.5.1.mDL",
            ErrorCode = 1
        };

        // Act
        response.DocumentErrors = new List<DocumentError> { error };

        // Assert
        response.DocumentErrors.Should().HaveCount(1);
        response.DocumentErrors![0].DocType.Should().Be("org.iso.18013.5.1.mDL");
    }

    [Fact]
    public void ToCbor_WithDocuments_ReturnsValidCborBytes()
    {
        // Arrange
        var response = new DeviceResponse();
        response.Documents.Add(new Document
        {
            DocType = "org.iso.18013.5.1.mDL",
            IssuerSigned = new IssuerSigned()
        });

        // Act
        var cborBytes = response.ToCbor();

        // Assert
        cborBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromCbor_WithValidData_RestoresDeviceResponse()
    {
        // Arrange
        var original = new DeviceResponse();
        original.Documents.Add(new Document
        {
            DocType = "org.iso.18013.5.1.mDL",
            IssuerSigned = new IssuerSigned()
        });

        var cborBytes = original.ToCbor();

        // Act
        var restored = DeviceResponse.FromCbor(cborBytes);

        // Assert
        restored.Version.Should().Be(original.Version);
        restored.Documents.Should().HaveCount(1);
        restored.Documents[0].DocType.Should().Be("org.iso.18013.5.1.mDL");
    }

    [Fact]
    public void Version_CanBeSet()
    {
        // Arrange
        var response = new DeviceResponse { Version = "2.0" };

        // Assert
        response.Version.Should().Be("2.0");
    }

    [Fact]
    public void ToCborObject_ReturnsNonNull()
    {
        // Arrange
        var response = new DeviceResponse();

        // Act
        var cborObject = response.ToCborObject();

        // Assert
        cborObject.Should().NotBeNull();
    }

    [Fact]
    public void DocumentErrors_DefaultIsNull()
    {
        // Act
        var response = new DeviceResponse();

        // Assert
        response.DocumentErrors.Should().BeNull();
    }
}
