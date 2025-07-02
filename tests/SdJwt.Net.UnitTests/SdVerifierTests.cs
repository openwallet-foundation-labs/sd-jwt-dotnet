using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Verifier;

namespace SdJwt.Net.Tests;

public class SdVerifierTests : TestBase
{
    private readonly SdIssuer _issuer;
    private readonly SdVerifier _verifier;

    public SdVerifierTests()
    {
        _issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        _verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
    }

    [Fact]
    public void Constructor_WithNullKeyProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SdVerifier(null!));
    }

    [Fact]
    public async Task VerifyAsync_WithNulls_ThrowsException()
    {
        var validationParams = new TokenValidationParameters();
        await Assert.ThrowsAsync<ArgumentException>(() => _verifier.VerifyAsync(" ", validationParams));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _verifier.VerifyAsync("some-jwt", null!));
    }

    [Fact]
    public async Task VerifyAsync_WithInvalidSignature_ThrowsException()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(new(), new SdIssuanceOptions());
        var tamperedJwt = string.Concat(issuerOutput.SdJwt.AsSpan(0, issuerOutput.SdJwt.Length - 5), "abcde");
        var presentation = $"{tamperedJwt}";

        var validationParams = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false };

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenInvalidSignatureException>(
            () => _verifier.VerifyAsync(presentation, validationParams)
        );
    }

    [Fact]
    public async Task VerifyAsync_WithSuperfluousDisclosure_ThrowsException()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new() { { "email", "a@b.com" } },
            new SdIssuanceOptions { DisclosureStructure = new { email = true } }
        );

        var fakeDisclosure = new Disclosure("salt", "fake_claim", "fake_value").EncodedValue;
        var presentation = $"{issuerOutput.SdJwt}~{issuerOutput.Disclosures[0].EncodedValue}~{fakeDisclosure}";

        var validationParams = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(
            () => _verifier.VerifyAsync(presentation, validationParams)
        );
        Assert.Contains("do not correspond to any digest", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_WithDuplicateDisclosure_ThrowsException()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new() { { "email", "a@b.com" } },
            new SdIssuanceOptions { DisclosureStructure = new { email = true } }
        );

        var disclosure = issuerOutput.Disclosures[0].EncodedValue;
        var presentation = $"{issuerOutput.SdJwt}~{disclosure}~{disclosure}";

        var validationParams = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(
            () => _verifier.VerifyAsync(presentation, validationParams)
        );
        Assert.Contains("Duplicate disclosure detected", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_WithInvalidKbJwtSignature_ThrowsException()
    {
        // Arrange
        var issuerOutput = _issuer.Issue([], new SdIssuanceOptions(), HolderPublicJwk);
        var holder = new SdJwtHolder(issuerOutput.Issuance);

        // Sign KB-JWT with a different key, but set the KeyId to match
        using var wrongKeyEcdsa = System.Security.Cryptography.ECDsa.Create(System.Security.Cryptography.ECCurve.NamedCurves.nistP256);
        var wrongKey = new ECDsaSecurityKey(wrongKeyEcdsa) { KeyId = "holder-key-1" }; // <-- Add KeyId

        var presentation = holder.CreatePresentation(
            _ => false,
            new() { { "aud", "v" }, { "nonce", "n" } },
            wrongKey,
            HolderSigningAlgorithm);

        var validationParams = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false };
        var kbValidationParams = new TokenValidationParameters { ValidAudience = "v", IssuerSigningKey = HolderPublicKey };

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenInvalidSignatureException>(
            () => _verifier.VerifyAsync(presentation, validationParams, kbValidationParams)
        );
    }

    [Fact]
    public async Task VerifyAsync_WithMismatchedSdHash_ThrowsException()
    {
            // Arrange
        // 1. Create a standard, valid presentation. The KB-JWT's sd_hash is based on this first SD-JWT.
        var originalIssuerOutput = _issuer.Issue(new() { { "claim", "original" } }, new SdIssuanceOptions(), HolderPublicJwk);
        var holder = new SdJwtHolder(originalIssuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            _ => false,
            new() { { "aud", "v" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        // 2. Create a *second*, completely different but still validly signed SD-JWT.
        var tamperedIssuerOutput = _issuer.Issue(new() { { "claim", "tampered" } }, new SdIssuanceOptions(), HolderPublicJwk);

        // 3. Create the tampered presentation by swapping the SD-JWT part.
        // The KB-JWT is now bound to the original SD-JWT, but the presentation contains the tampered one.
        var tamperedPresentation = tamperedIssuerOutput.SdJwt + presentation.Substring(presentation.IndexOf('~'));

        var validationParams = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false };
        var kbValidationParams = new TokenValidationParameters { ValidAudience = "v", IssuerSigningKey = HolderPublicKey, ValidateIssuer = false };

        // Act & Assert
        // The verifier should now successfully validate the signature of the tampered SD-JWT,
        // but then fail at the sd_hash check.
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(
            () => _verifier.VerifyAsync(tamperedPresentation, validationParams, kbValidationParams)
        );

        // We expect a generic SecurityTokenException, not a signature validation error.
        Assert.IsNotType<SecurityTokenInvalidSignatureException>(ex);
        Assert.Contains("sd_hash in Key Binding JWT does not match", ex.Message);
    }
}