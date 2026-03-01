using FluentAssertions;
using SdJwt.Net.Eudiw.TrustFramework;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.TrustFramework;

/// <summary>
/// Tests for Trust Service Type enumeration per eIDAS framework.
/// </summary>
public class TrustServiceTypeTests
{
    [Fact]
    public void TrustServiceType_ContainsQualifiedCertificateSignature()
    {
        // Assert - For qualified electronic signatures
        Enum.IsDefined(typeof(TrustServiceType), TrustServiceType.QualifiedCertificateSignature)
            .Should().BeTrue();
    }

    [Fact]
    public void TrustServiceType_ContainsQualifiedCertificateSeal()
    {
        // Assert - For qualified electronic seals
        Enum.IsDefined(typeof(TrustServiceType), TrustServiceType.QualifiedCertificateSeal)
            .Should().BeTrue();
    }

    [Fact]
    public void TrustServiceType_ContainsQualifiedAttestation()
    {
        // Assert - For QEAA providers
        Enum.IsDefined(typeof(TrustServiceType), TrustServiceType.QualifiedAttestation)
            .Should().BeTrue();
    }

    [Fact]
    public void TrustServiceType_ContainsPidProvider()
    {
        // Assert - For PID providers
        Enum.IsDefined(typeof(TrustServiceType), TrustServiceType.PidProvider)
            .Should().BeTrue();
    }

    [Fact]
    public void TrustServiceType_ContainsElectronicAttestation()
    {
        // Assert - For EAA providers
        Enum.IsDefined(typeof(TrustServiceType), TrustServiceType.ElectronicAttestation)
            .Should().BeTrue();
    }

    [Fact]
    public void TrustServiceType_HasCorrectCount()
    {
        // Assert - Should have 5 service types as defined in eIDAS
        var values = Enum.GetValues<TrustServiceType>();
        values.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(TrustServiceType.QualifiedCertificateSignature, true)]
    [InlineData(TrustServiceType.QualifiedCertificateSeal, true)]
    [InlineData(TrustServiceType.QualifiedAttestation, true)]
    [InlineData(TrustServiceType.PidProvider, true)]
    [InlineData(TrustServiceType.ElectronicAttestation, false)]
    public void IsQualifiedService_ReturnsExpectedValue(TrustServiceType type, bool expectedQualified)
    {
        // Act
        var result = TrustServiceTypeExtensions.IsQualifiedService(type);

        // Assert
        result.Should().Be(expectedQualified);
    }

    [Theory]
    [InlineData(TrustServiceType.QualifiedCertificateSignature, "http://uri.etsi.org/TrstSvc/Svctype/CA/QC")]
    [InlineData(TrustServiceType.QualifiedCertificateSeal, "http://uri.etsi.org/TrstSvc/Svctype/CA/QC")]
    [InlineData(TrustServiceType.QualifiedAttestation, "http://uri.etsi.org/TrstSvc/Svctype/Certstatus/QEAA")]
    public void GetServiceTypeUri_ReturnsCorrectUri(TrustServiceType type, string expectedUri)
    {
        // Act
        var uri = TrustServiceTypeExtensions.GetServiceTypeUri(type);

        // Assert
        uri.Should().Contain(expectedUri.Split('/').Last());
    }
}
