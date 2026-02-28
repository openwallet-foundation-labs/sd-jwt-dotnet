using FluentAssertions;
using SdJwt.Net.Mdoc.Namespaces;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Namespaces;

/// <summary>
/// Tests for mDL namespace constants and data elements.
/// </summary>
public class MdlNamespaceTests
{
    [Fact]
    public void Namespace_HasCorrectValue()
    {
        // Assert
        MdlNamespace.Namespace.Should().Be("org.iso.18013.5.1");
    }

    [Fact]
    public void DocType_HasCorrectValue()
    {
        // Assert
        MdlNamespace.DocType.Should().Be("org.iso.18013.5.1.mDL");
    }

    [Theory]
    [InlineData(MdlDataElement.FamilyName, "family_name")]
    [InlineData(MdlDataElement.GivenName, "given_name")]
    [InlineData(MdlDataElement.BirthDate, "birth_date")]
    [InlineData(MdlDataElement.IssueDate, "issue_date")]
    [InlineData(MdlDataElement.ExpiryDate, "expiry_date")]
    [InlineData(MdlDataElement.IssuingCountry, "issuing_country")]
    [InlineData(MdlDataElement.IssuingAuthority, "issuing_authority")]
    [InlineData(MdlDataElement.DocumentNumber, "document_number")]
    [InlineData(MdlDataElement.Portrait, "portrait")]
    [InlineData(MdlDataElement.DrivingPrivileges, "driving_privileges")]
    [InlineData(MdlDataElement.AgeOver18, "age_over_18")]
    [InlineData(MdlDataElement.AgeOver21, "age_over_21")]
    public void MdlDataElement_ToElementIdentifier_ReturnsCorrectString(MdlDataElement element, string expected)
    {
        // Act
        var result = element.ToElementIdentifier();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void AllMandatoryElements_ArePresent()
    {
        // ISO 18013-5 mandatory elements
        var mandatoryElements = new[]
        {
            MdlDataElement.FamilyName,
            MdlDataElement.GivenName,
            MdlDataElement.BirthDate,
            MdlDataElement.IssueDate,
            MdlDataElement.ExpiryDate,
            MdlDataElement.IssuingCountry,
            MdlDataElement.IssuingAuthority,
            MdlDataElement.DocumentNumber,
            MdlDataElement.Portrait,
            MdlDataElement.DrivingPrivileges,
            MdlDataElement.UnDistinguishingSign
        };

        // Assert
        foreach (var element in mandatoryElements)
        {
            Enum.IsDefined(typeof(MdlDataElement), element).Should().BeTrue();
        }
    }

    [Fact]
    public void MandatoryElements_ContainsAllRequired()
    {
        // Assert
        MdlNamespace.MandatoryElements.Should().HaveCount(11);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.FamilyName);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.GivenName);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.BirthDate);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.IssueDate);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.ExpiryDate);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.IssuingCountry);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.IssuingAuthority);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.DocumentNumber);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.Portrait);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.DrivingPrivileges);
        MdlNamespace.MandatoryElements.Should().Contain(MdlDataElement.UnDistinguishingSign);
    }

    [Fact]
    public void IsMandatory_ForFamilyName_ReturnsTrue()
    {
        // Act
        var result = MdlNamespace.IsMandatory(MdlDataElement.FamilyName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMandatory_ForAgeOver18_ReturnsFalse()
    {
        // Act
        var result = MdlNamespace.IsMandatory(MdlDataElement.AgeOver18);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("family_name", MdlDataElement.FamilyName)]
    [InlineData("given_name", MdlDataElement.GivenName)]
    [InlineData("birth_date", MdlDataElement.BirthDate)]
    [InlineData("age_over_18", MdlDataElement.AgeOver18)]
    public void FromElementIdentifier_ReturnsCorrectElement(string identifier, MdlDataElement expected)
    {
        // Act
        var result = MdlDataElementExtensions.FromElementIdentifier(identifier);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FromElementIdentifier_WithInvalidIdentifier_ReturnsNull()
    {
        // Act
        var result = MdlDataElementExtensions.FromElementIdentifier("invalid_element");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFullIdentifier_ReturnsNamespaceWithElement()
    {
        // Act
        var result = MdlNamespace.GetFullIdentifier(MdlDataElement.FamilyName);

        // Assert
        result.Should().Be("org.iso.18013.5.1.family_name");
    }

    [Fact]
    public void CreateClaims_CreatesDictionaryWithCorrectStructure()
    {
        // Arrange
        var claims = new Dictionary<MdlDataElement, object>
        {
            { MdlDataElement.FamilyName, "Doe" },
            { MdlDataElement.GivenName, "John" }
        };

        // Act
        var result = MdlNamespace.CreateClaims(claims);

        // Assert
        result.Should().ContainKey(MdlNamespace.Namespace);
        result[MdlNamespace.Namespace].Should().ContainKey("family_name");
        result[MdlNamespace.Namespace].Should().ContainKey("given_name");
        result[MdlNamespace.Namespace]["family_name"].Should().Be("Doe");
        result[MdlNamespace.Namespace]["given_name"].Should().Be("John");
    }
}
