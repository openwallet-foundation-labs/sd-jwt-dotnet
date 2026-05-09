using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Models;
using SdJwt.Net.Mdoc.Namespaces;
using SdJwt.Net.Mdoc.Verifier;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Verifier;

/// <summary>
/// Tests for MdocVerifier credential verification.
/// </summary>
public class MdocVerifierTests : TestBase
{
    [Fact]
    public void Constructor_WithDefaultOptions_CreatesVerifier()
    {
        // Act
        var verifier = new MdocVerifier();

        // Assert
        verifier.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomOptions_CreatesVerifier()
    {
        // Arrange
        var options = new MdocVerificationOptions
        {
            VerifyValidity = true,
            VerifyDeviceSignature = false
        };

        // Act
        var verifier = new MdocVerifier(options);

        // Assert
        verifier.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCryptoProvider_CreatesVerifier()
    {
        // Arrange
        var options = new MdocVerificationOptions();
        var cryptoProvider = new DefaultCoseCryptoProvider();

        // Act
        var verifier = new MdocVerifier(options, cryptoProvider);

        // Assert
        verifier.Should().NotBeNull();
    }

    [Fact]
    public void Verify_WithEmptyDeviceResponse_ReturnsEmptyResults()
    {
        // Arrange
        var verifier = new MdocVerifier();
        var response = new DeviceResponse();

        // Act
        var results = verifier.Verify(response);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void VerifyDocument_WithDisallowedDocType_ReturnsInvalid()
    {
        // Arrange
        var options = new MdocVerificationOptions
        {
            AllowedDocTypes = new List<string> { "org.example.other" }
        };
        var verifier = new MdocVerifier(options);
        var document = new Document
        {
            DocType = MdlDocType,
            IssuerSigned = new IssuerSigned()
        };

        // Act
        var result = verifier.VerifyDocument(document);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not allowed"));
    }

    [Fact]
    public void VerifyDocument_WithAllowedDocType_ContinuesVerification()
    {
        // Arrange
        var options = new MdocVerificationOptions
        {
            AllowedDocTypes = new List<string> { MdlDocType }
        };
        var verifier = new MdocVerifier(options);
        var document = new Document
        {
            DocType = MdlDocType,
            IssuerSigned = new IssuerSigned()
        };

        // Act - will fail due to empty IssuerAuth, but not due to doc type
        var result = verifier.VerifyDocument(document);

        // Assert - should fail for other reasons, not doc type
        result.Errors.Should().NotContain(e => e.Contains("not allowed"));
    }

    [Fact]
    public void MdocVerificationOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new MdocVerificationOptions();

        // Assert
        options.AllowedDocTypes.Should().BeEmpty();
        options.VerifyCertificateChain.Should().BeTrue();
        options.VerifyValidity.Should().BeTrue();
        options.VerifyDeviceSignature.Should().BeTrue();
        options.ClockSkewTolerance.Should().Be(TimeSpan.FromMinutes(5));
        options.CurrentTime.Should().BeNull();
        options.TrustedIssuers.Should().BeEmpty();
    }

    [Fact]
    public void MdocVerificationResult_Success_CreatesValidResult()
    {
        // Act
        var result = MdocVerificationResult.Success();

        // Assert
        result.IsValid.Should().BeTrue();
        result.IssuerSignatureValid.Should().BeTrue();
        result.DigestsValid.Should().BeTrue();
        result.ValidityPeriodValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void MdocVerificationResult_Failure_CreatesInvalidResult()
    {
        // Act
        var result = MdocVerificationResult.Failure("Test error");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Test error");
    }

    [Fact]
    public void Verify_WithDeviceResponse_ReturnsResultPerDocument()
    {
        // Arrange
        var verifier = new MdocVerifier();
        var response = new DeviceResponse();
        response.Documents.Add(new Document
        {
            DocType = MdlDocType,
            IssuerSigned = new IssuerSigned()
        });
        response.Documents.Add(new Document
        {
            DocType = "org.example.other",
            IssuerSigned = new IssuerSigned()
        });

        // Act
        var results = verifier.Verify(response);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public void VerifyDocument_WithSessionTranscript_AcceptsTranscript()
    {
        // Arrange
        var verifier = new MdocVerifier();
        var document = new Document
        {
            DocType = MdlDocType,
            IssuerSigned = new IssuerSigned()
        };
        var transcript = SessionTranscript.ForOpenId4Vp(
            "client123",
            "nonce456",
            null,
            "https://verifier.example.com/callback");

        // Act
        var result = verifier.VerifyDocument(document, transcript);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyDocument_WithES384Credential_VerifiesDigests()
    {
        // Arrange - create a self-signed ES384 cert
        using var issuerKey384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var certRequest = new CertificateRequest(
            "CN=Test Issuer", issuerKey384, HashAlgorithmName.SHA384);
        using var cert = certRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddYears(1));
        var certBytes = cert.RawData;

        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(issuerKey384)
            .WithIssuerCertificate(certBytes)
            .WithDeviceKey(deviceCoseKey)
            .WithAlgorithm(CoseAlgorithm.ES384)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .AddClaim(MdlNamespace, "given_name", "John")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        var options = new MdocVerificationOptions
        {
            VerifyDeviceSignature = false,
            VerifyCertificateChain = false
        };
        var verifier = new MdocVerifier(options);

        // Act
        var result = verifier.VerifyDocument(document);

        // Assert
        result.IssuerSignatureValid.Should().BeTrue();
        result.DigestsValid.Should().BeTrue();
        result.MobileSecurityObject!.DigestAlgorithm.Should().Be("SHA-384");
    }

    [Fact]
    public void VerifyDocument_WithDeviceMacNull_AndDeviceSignatureNull_SkipsDeviceVerification()
    {
        // Arrange - a document with DeviceSigned but no auth and no SessionTranscript
        // The verifier skips device signature verification when DeviceAuth has no data
        var options = new MdocVerificationOptions
        {
            VerifyDeviceSignature = true,
            VerifyCertificateChain = false
        };
        var verifier = new MdocVerifier(options);
        var document = new Document
        {
            DocType = MdlDocType,
            IssuerSigned = new IssuerSigned(),
            DeviceSigned = new DeviceSigned
            {
                DeviceAuth = new DeviceAuth()
            }
        };

        // Act - no SessionTranscript provided, so device verification is skipped
        var result = verifier.VerifyDocument(document);

        // Assert - device sig verification was skipped, not failed
        result.DeviceSignatureValid.Should().BeTrue();
    }

    [Fact]
    public void MdocVerificationOptions_TrustedIssuers_DefaultsToEmpty()
    {
        // Act
        var options = new MdocVerificationOptions();

        // Assert
        options.TrustedIssuers.Should().NotBeNull();
        options.TrustedIssuers.Should().BeEmpty();
    }
}
