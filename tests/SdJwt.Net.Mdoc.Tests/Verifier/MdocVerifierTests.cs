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
}
