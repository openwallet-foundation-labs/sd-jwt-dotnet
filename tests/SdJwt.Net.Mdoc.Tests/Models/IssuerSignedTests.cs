using FluentAssertions;
using SdJwt.Net.Mdoc.Models;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Models;

/// <summary>
/// Tests for IssuerSigned structure.
/// </summary>
public class IssuerSignedTests
{
    [Fact]
    public void Constructor_CreatesEmptyNameSpaces()
    {
        // Act
        var issuerSigned = new IssuerSigned();

        // Assert
        issuerSigned.NameSpaces.Should().NotBeNull();
        issuerSigned.NameSpaces.Should().BeEmpty();
    }

    [Fact]
    public void AddNameSpace_StoresItems()
    {
        // Arrange
        var issuerSigned = new IssuerSigned();
        var items = new List<IssuerSignedItem>
        {
            new()
            {
                DigestId = 0,
                ElementIdentifier = "family_name",
                ElementValue = "Doe"
            }
        };

        // Act
        issuerSigned.NameSpaces["org.iso.18013.5.1"] = items;

        // Assert
        issuerSigned.NameSpaces.Should().ContainKey("org.iso.18013.5.1");
        issuerSigned.NameSpaces["org.iso.18013.5.1"].Should().HaveCount(1);
    }

    [Fact]
    public void IssuerAuth_WhenSet_StoresCoseSign1()
    {
        // Arrange
        var issuerSigned = new IssuerSigned();
        var issuerAuth = new byte[] { 0xD2, 0x84, 0x43, 0xA1 };

        // Act
        issuerSigned.IssuerAuth = issuerAuth;

        // Assert
        issuerSigned.IssuerAuth.Should().BeEquivalentTo(issuerAuth);
    }

    [Fact]
    public void ToCbor_ReturnsValidCborBytes()
    {
        // Arrange
        var issuerSigned = new IssuerSigned();
        issuerSigned.NameSpaces["org.iso.18013.5.1"] = new List<IssuerSignedItem>
        {
            new()
            {
                DigestId = 0,
                ElementIdentifier = "family_name",
                ElementValue = "Doe",
                Random = new byte[] { 0x01, 0x02, 0x03 }
            }
        };

        // Act
        var cborBytes = issuerSigned.ToCbor();

        // Assert
        cborBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromCbor_WithValidData_RestoresIssuerSigned()
    {
        // Arrange
        var original = new IssuerSigned();
        original.NameSpaces["org.iso.18013.5.1"] = new List<IssuerSignedItem>
        {
            new()
            {
                DigestId = 0,
                ElementIdentifier = "family_name",
                ElementValue = "Doe",
                Random = new byte[] { 0x01, 0x02, 0x03 }
            }
        };

        var cborBytes = original.ToCbor();

        // Act
        var restored = IssuerSigned.FromCbor(cborBytes);

        // Assert
        restored.NameSpaces.Should().ContainKey("org.iso.18013.5.1");
        restored.NameSpaces["org.iso.18013.5.1"].Should().HaveCount(1);
    }
}
