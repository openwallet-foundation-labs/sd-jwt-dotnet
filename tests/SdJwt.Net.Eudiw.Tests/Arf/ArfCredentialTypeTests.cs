using FluentAssertions;
using SdJwt.Net.Eudiw.Arf;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.Arf;

/// <summary>
/// Tests for ARF credential type enumeration.
/// </summary>
public class ArfCredentialTypeTests
{
    [Fact]
    public void ArfCredentialType_ContainsPid()
    {
        // Assert - Person Identification Data
        Enum.IsDefined(typeof(ArfCredentialType), ArfCredentialType.Pid).Should().BeTrue();
    }

    [Fact]
    public void ArfCredentialType_ContainsMdl()
    {
        // Assert - Mobile Driving License
        Enum.IsDefined(typeof(ArfCredentialType), ArfCredentialType.Mdl).Should().BeTrue();
    }

    [Fact]
    public void ArfCredentialType_ContainsQeaa()
    {
        // Assert - Qualified Electronic Attestation of Attributes
        Enum.IsDefined(typeof(ArfCredentialType), ArfCredentialType.Qeaa).Should().BeTrue();
    }

    [Fact]
    public void ArfCredentialType_ContainsEaa()
    {
        // Assert - Electronic Attestation of Attributes
        Enum.IsDefined(typeof(ArfCredentialType), ArfCredentialType.Eaa).Should().BeTrue();
    }

    [Fact]
    public void ArfCredentialType_HasCorrectCount()
    {
        // Assert - Should have exactly 4 credential types defined in ARF
        var values = Enum.GetValues<ArfCredentialType>();
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(ArfCredentialType.Pid, "eu.europa.ec.eudi.pid.1")]
    [InlineData(ArfCredentialType.Mdl, "org.iso.18013.5.1.mDL")]
    public void GetDocType_ReturnsCorrectDocType(ArfCredentialType type, string expectedDocType)
    {
        // Act
        var docType = ArfCredentialTypeExtensions.GetDocType(type);

        // Assert
        docType.Should().Be(expectedDocType);
    }

    [Theory]
    [InlineData(ArfCredentialType.Pid, true)]
    [InlineData(ArfCredentialType.Mdl, true)]
    [InlineData(ArfCredentialType.Qeaa, false)]
    [InlineData(ArfCredentialType.Eaa, false)]
    public void IsMdocFormat_ReturnsCorrectValue(ArfCredentialType type, bool expectedResult)
    {
        // Act
        var result = ArfCredentialTypeExtensions.IsMdocFormat(type);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(ArfCredentialType.Pid, false)]
    [InlineData(ArfCredentialType.Mdl, false)]
    [InlineData(ArfCredentialType.Qeaa, true)]
    [InlineData(ArfCredentialType.Eaa, true)]
    public void IsSdJwtVcFormat_ReturnsCorrectValue(ArfCredentialType type, bool expectedResult)
    {
        // Act
        var result = ArfCredentialTypeExtensions.IsSdJwtVcFormat(type);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(ArfCredentialType.Pid, true)]
    [InlineData(ArfCredentialType.Mdl, false)]
    [InlineData(ArfCredentialType.Qeaa, true)]
    [InlineData(ArfCredentialType.Eaa, false)]
    public void RequiresQualifiedTrust_ReturnsCorrectValue(ArfCredentialType type, bool expectedResult)
    {
        // Act - PID and QEAA require qualified trust providers, mDL and EAA do not
        var result = ArfCredentialTypeExtensions.RequiresQualifiedTrust(type);

        // Assert
        result.Should().Be(expectedResult);
    }
}
