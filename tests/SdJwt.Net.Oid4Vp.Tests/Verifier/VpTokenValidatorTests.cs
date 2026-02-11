using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Verifier;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;
using JsonWebKeyMs = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace SdJwt.Net.Oid4Vp.Tests.Verifier;

/// <summary>
/// Comprehensive tests for VpTokenValidator to verify OID4VP compliance,
/// especially nonce validation, audience validation, and freshness validation.
/// </summary>
public class VpTokenValidatorTests : IDisposable
{
    private readonly SecurityKey _issuerSigningKey;
    private readonly SecurityKey _holderPrivateKey;
    private readonly SecurityKey _holderPublicKey;
    private readonly JsonWebKeyMs _holderPublicJwk;
    private readonly VpTokenValidator _validator;
    private readonly VpTokenValidator _validatorWithoutVc;
    private readonly string _validNonce;
    private readonly string _validClientId;
    private readonly ECDsa _issuerEcdsa;
    private readonly ECDsa _holderEcdsa;

    public VpTokenValidatorTests()
    {
        // Create keys
        _issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        _issuerSigningKey = new ECDsaSecurityKey(_issuerEcdsa) { KeyId = "issuer-key" };
        _holderPrivateKey = new ECDsaSecurityKey(_holderEcdsa) { KeyId = "holder-key" };
        _holderPublicKey = new ECDsaSecurityKey(_holderEcdsa) { KeyId = "holder-key" };
        _holderPublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(_holderPublicKey);

        // Create validators
        _validator = new VpTokenValidator(
            jwtToken => Task.FromResult(_issuerSigningKey),
            useSdJwtVcValidation: true);

        _validatorWithoutVc = new VpTokenValidator(
            jwtToken => Task.FromResult(_issuerSigningKey),
            useSdJwtVcValidation: false);

        _validClientId = "https://verifier.example.com";
        _validNonce = "test-nonce-" + Guid.NewGuid().ToString();
    }

    public void Dispose()
    {
        _issuerEcdsa?.Dispose();
        _holderEcdsa?.Dispose();
    }

    #region Helper Methods

    private VpTokenValidationOptions GetValidOptions()
    {
        return new VpTokenValidationOptions
        {
            ValidateIssuer = false, // Disable for tests to focus on OID4VP-specific validations
            ValidateKeyBindingAudience = true,
            ValidateKeyBindingFreshness = true,
            ValidateKeyBindingLifetime = false, // Use freshness validation (iat-based) instead of lifetime validation (exp-based)
            ExpectedClientId = _validClientId,
            MaxKeyBindingAge = TimeSpan.FromMinutes(10),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            ValidIssuers = new[] { "https://issuer.example.com" }
        };
    }

    private string CreateValidVpToken(string? nonce = null, string? audience = null, DateTimeOffset? issuedAt = null)
    {
        nonce ??= _validNonce;
        audience ??= _validClientId;
        issuedAt ??= DateTimeOffset.UtcNow;

        // Create SD-JWT VC
        var vcIssuer = new SdJwtVcIssuer(_issuerSigningKey, SecurityAlgorithms.EcdsaSha256);

        var payload = new SdJwtVcPayload
        {
            Issuer = "https://issuer.example.com",
            Subject = "did:example:holder123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["name"] = "Test User",
                ["email"] = "test@example.com",
                ["position"] = "Software Engineer"
            }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { email = true }
        };

        var issuance = vcIssuer.Issue(
            "https://credentials.example.com/employee",
            payload,
            options,
            _holderPublicJwk
        );

        // Create presentation with KB-JWT
        var holder = new SdJwtHolder(issuance.Issuance);
        var presentation = holder.CreatePresentation(
            disclosure => true, // Include all disclosures
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = audience,
                [JwtRegisteredClaimNames.Iat] = issuedAt.Value.ToUnixTimeSeconds(),
                [JwtRegisteredClaimNames.Nonce] = nonce
            },
            _holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        return presentation;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithSdJwtVcValidation_CreatesValidatorSuccessfully()
    {
        // Act
        var validator = new VpTokenValidator(
            jwtToken => Task.FromResult(_issuerSigningKey),
            useSdJwtVcValidation: true);

        // Assert
        validator.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithoutSdJwtVcValidation_CreatesValidatorSuccessfully()
    {
        // Act
        var validator = new VpTokenValidator(
            jwtToken => Task.FromResult(_issuerSigningKey),
            useSdJwtVcValidation: false);

        // Assert
        validator.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullKeyProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new VpTokenValidator(null!, useSdJwtVcValidation: true));
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void CreateForOid4Vp_WithClientId_ReturnsConfiguredOptions()
    {
        // Act
        var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");

        // Assert
        options.ValidateIssuer.Should().BeTrue();
        options.ValidateKeyBindingAudience.Should().BeTrue();
        options.ValidateKeyBindingFreshness.Should().BeTrue();
        options.ExpectedClientId.Should().Be("https://verifier.example.com");
        options.MaxKeyBindingAge.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void CreateForTesting_ReturnsRelaxedOptions()
    {
        // Act
        var options = VpTokenValidationOptions.CreateForTesting();

        // Assert
        options.ValidateIssuer.Should().BeFalse();
        options.ValidateKeyBindingAudience.Should().BeFalse();
        options.ValidateKeyBindingFreshness.Should().BeFalse();
        options.MaxKeyBindingAge.Should().Be(TimeSpan.FromHours(24));
    }

    #endregion

    #region Nonce Validation Tests (CRITICAL)

    [Fact]
    public async Task ValidateVpTokenAsync_WithCorrectNonce_Succeeds()
    {
        // Arrange
        var vpToken = CreateValidVpToken(_validNonce);
        var options = GetValidOptions();

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithWrongNonce_Fails()
    {
        // Arrange
        var vpToken = CreateValidVpToken("correct-nonce");
        var wrongNonce = "wrong-nonce";
        var options = GetValidOptions();

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, wrongNonce, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNull().And.ContainAny("nonce", "Nonce");
    }

    [Fact]
    public async Task ValidateVpTokenAsync_SdJwtVcPath_ValidatesNonceCorrectly()
    {
        // Arrange
        var validatorWithVc = new VpTokenValidator(
            jwtToken => Task.FromResult(_issuerSigningKey),
            useSdJwtVcValidation: true);

        var vpToken = CreateValidVpToken("nonce-123");
        var wrongNonce = "nonce-456";
        var options = GetValidOptions();

        // Act
        var result = await validatorWithVc.ValidateVpTokenAsync(vpToken, wrongNonce, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNull().And.ContainAny("nonce", "Nonce");
    }

    [Fact]
    public async Task ValidateVpTokenAsync_GenericSdJwtPath_ValidatesNonceCorrectly()
    {
        // Arrange
        var validatorGeneric = new VpTokenValidator(
            jwtToken => Task.FromResult(_issuerSigningKey),
            useSdJwtVcValidation: false);

        var vpToken = CreateValidVpToken("nonce-abc");
        var wrongNonce = "nonce-xyz";
        var options = GetValidOptions();

        // Act
        var result = await validatorGeneric.ValidateVpTokenAsync(vpToken, wrongNonce, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNull().And.ContainAny("nonce", "Nonce");
    }

    [Fact]
    public async Task ValidateVpTokenAsync_ReplayAttack_IsPrevented()
    {
        // Arrange
        var token = CreateValidVpToken("nonce-original");
        var options = GetValidOptions();

        // Act - First use with correct nonce succeeds
        var result1 = await _validator.ValidateVpTokenAsync(token, "nonce-original", options);

        // Act - Replay with different nonce should fail
        var result2 = await _validator.ValidateVpTokenAsync(token, "nonce-replay", options);

        // Assert
        result1.IsValid.Should().BeTrue();
        result2.IsValid.Should().BeFalse();
        result2.Error.Should().NotBeNull().And.ContainAny("nonce", "Nonce");
    }

    #endregion

    #region Audience Validation Tests

    [Fact]
    public async Task ValidateVpTokenAsync_WithCorrectAudience_Succeeds()
    {
        // Arrange
        var vpToken = CreateValidVpToken(audience: _validClientId);
        var options = GetValidOptions();

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithWrongAudience_FailsWhenEnabled()
    {
        // Arrange
        var wrongAudience = "https://wrong-verifier.example.com";
        var vpToken = CreateValidVpToken(audience: wrongAudience);
        var options = GetValidOptions();

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithWrongAudience_SucceedsWhenDisabled()
    {
        // Arrange
        var wrongAudience = "https://wrong-verifier.example.com";
        var vpToken = CreateValidVpToken(audience: wrongAudience);
        var options = new VpTokenValidationOptions
        {
            ValidateKeyBindingAudience = false,
            ValidateKeyBindingFreshness = false,
            ValidateKeyBindingLifetime = false,  // KB-JWTs use iat-based freshness, not exp-based lifetime
            ValidateIssuer = false,
            ValidateLifetime = true
        };

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
    }

    #endregion

    #region Freshness Validation Tests

    [Fact]
    public async Task ValidateVpTokenAsync_WithFreshToken_Succeeds()
    {
        // Arrange
        var vpToken = CreateValidVpToken(issuedAt: DateTimeOffset.UtcNow);
        var options = GetValidOptions();
        options.MaxKeyBindingAge = TimeSpan.FromMinutes(10);

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithStaleToken_Fails()
    {
        // Arrange
        var oldIssuedAt = DateTimeOffset.UtcNow.AddMinutes(-20);
        var vpToken = CreateValidVpToken(issuedAt: oldIssuedAt);
        var options = GetValidOptions();
        options.MaxKeyBindingAge = TimeSpan.FromMinutes(10);

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNull().And.Match("*too old*");
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithStaleToken_SucceedsWhenDisabled()
    {
        // Arrange
        var oldIssuedAt = DateTimeOffset.UtcNow.AddMinutes(-20);
        var vpToken = CreateValidVpToken(issuedAt: oldIssuedAt);
        var options = new VpTokenValidationOptions
        {
            ValidateKeyBindingFreshness = false,
            ValidateKeyBindingAudience = false,
            ValidateKeyBindingLifetime = false,  // KB-JWTs use iat-based freshness, not exp-based lifetime
            ValidateIssuer = false,
            ValidateLifetime = false
        };

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
    }

    #endregion

    #region SD-JWT VC Validation Tests

    [Fact]
    public async Task ValidateVpTokenAsync_WithSdJwtVc_ValidatesVctClaim()
    {
        // Arrange
        var vpToken = CreateValidVpToken();
        var options = VpTokenValidationOptions.CreateForOid4Vp(_validClientId);
        options.ValidIssuers = new[] { "https://issuer.example.com" };

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
        result.Claims.Should().ContainKey("vct");
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithSdJwtVc_ValidatesIssClaim()
    {
        // Arrange
        var vpToken = CreateValidVpToken();
        var options = VpTokenValidationOptions.CreateForOid4Vp(_validClientId);
        options.ValidIssuers = new[] { "https://issuer.example.com" };

        // Act
        var result = await _validator.ValidateVpTokenAsync(vpToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
        result.Claims.Should().ContainKey("iss");
        result.Claims["iss"].Should().Be("https://issuer.example.com");
    }

    #endregion

    #region Error Scenario Tests

    [Fact]
    public async Task ValidateVpTokenAsync_WithInvalidToken_Fails()
    {
        // Arrange
        var invalidToken = "invalid.token.string";
        var options = GetValidOptions();

        // Act
        var result = await _validator.ValidateVpTokenAsync(invalidToken, _validNonce, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithEmptyToken_ThrowsException()
    {
        // Arrange
        var options = GetValidOptions();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _validator.ValidateVpTokenAsync(string.Empty, _validNonce, options));
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithEmptyNonce_ThrowsException()
    {
        // Arrange
        var vpToken = CreateValidVpToken();
        var options = VpTokenValidationOptions.CreateForOid4Vp(_validClientId);
        options.ValidIssuers = new[] { "https://issuer.example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _validator.ValidateVpTokenAsync(vpToken, string.Empty, options));
    }

    [Fact]
    public async Task ValidateVpTokenAsync_WithNullOptions_ThrowsException()
    {
        // Arrange
        var vpToken = CreateValidVpToken();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _validator.ValidateVpTokenAsync(vpToken, _validNonce, null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ValidateAsync_WithAuthorizationResponse_ValidatesAllTokens()
    {
        // Arrange
        var vpToken = CreateValidVpToken();
        var response = new AuthorizationResponse
        {
            VpToken = new[] { vpToken },
            PresentationSubmission = new PresentationSubmission
            {
                Id = "submission-1",
                DefinitionId = "def-1",
                DescriptorMap = new[]
                {
                    new InputDescriptorMapping
                    {
                        Id = "input-1",
                        Format = "vc+sd-jwt",
                        Path = "$"
                    }
                }
            }
        };

        var options = VpTokenValidationOptions.CreateForOid4Vp(_validClientId);
        options.ValidIssuers = new[] { "https://issuer.example.com" };

        // Act
        var result = await _validator.ValidateAsync(response, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
        result.ValidatedTokens.Should().HaveCount(1);
        result.ValidatedTokens[0].IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithMultipleTokens_ValidatesAll()
    {
        // Arrange
        var vpToken1 = CreateValidVpToken();
        var vpToken2 = CreateValidVpToken();

        var response = new AuthorizationResponse
        {
            VpToken = new[] { vpToken1, vpToken2 },
            PresentationSubmission = new PresentationSubmission
            {
                Id = "submission-1",
                DefinitionId = "def-1",
                DescriptorMap = new[]
                {
                    new InputDescriptorMapping { Id = "input-1", Format = "vc+sd-jwt", Path = "$[0]" },
                    new InputDescriptorMapping { Id = "input-2", Format = "vc+sd-jwt", Path = "$[1]" }
                }
            }
        };

        var options = VpTokenValidationOptions.CreateForOid4Vp(_validClientId);
        options.ValidIssuers = new[] { "https://issuer.example.com" };

        // Act
        var result = await _validator.ValidateAsync(response, _validNonce, options);

        // Assert
        result.IsValid.Should().BeTrue(result.Error);
        result.ValidatedTokens.Should().HaveCount(2);
        result.ValidatedTokens.All(t => t.IsValid).Should().BeTrue();
    }

    #endregion
}
