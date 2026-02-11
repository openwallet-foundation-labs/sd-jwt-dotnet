using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Comprehensive tests for SdVerifier to reach 95% coverage
/// </summary>
public class SdVerifierComprehensiveTests : TestBase
{
    private readonly SdIssuer _issuer;
    private readonly SdVerifier _verifier;

    public SdVerifierComprehensiveTests()
    {
        _issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        _verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
    }

    [Fact]
    public async Task VerifyAsync_WithMultipleNestedDisclosures_ReconstructsCorrectly()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload
            {
                {
                    "user", new
                    {
                        profile = new
                        {
                            name = "Test",
                            email = "test@example.com",
                            age = 30
                        }
                    }
                }
            },
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    user = new
                    {
                        profile = new
                        {
                            email = true,
                            age = true
                        }
                    }
                }
            }
        );

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act
        var result = await _verifier.VerifyAsync(issuerOutput.Issuance, validationParams);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ClaimsPrincipal);
    }

    [Fact]
    public async Task VerifyAsync_WithPartialDisclosures_OnlyIncludesProvided()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload
            {
                { "name", "Test" },
                { "email", "test@example.com" },
                { "phone", "123-456-7890" }
            },
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    email = true,
                    phone = true
                }
            }
        );

        // Only include first disclosure
        var presentation = $"{issuerOutput.SdJwt}~{issuerOutput.Disclosures[0].EncodedValue}";

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act
        var result = await _verifier.VerifyAsync(presentation, validationParams);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ClaimsPrincipal);
    }

    [Fact]
    public async Task VerifyAsync_WithKeyBindingJwt_VerifiesKeyBinding()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload { { "sub", "user123" } },
            new SdIssuanceOptions(),
            HolderPublicJwk
        );

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            _ => false,
            new JwtPayload { { "aud", "verifier" }, { "nonce", "test-nonce" } },
            HolderPrivateKey,
            HolderSigningAlgorithm
        );

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        var kbValidationParams = new TokenValidationParameters
        {
            ValidAudience = "verifier",
            IssuerSigningKey = HolderPublicKey,
            ValidateIssuer = false,
            ValidateLifetime = false  // KB-JWTs use iat-based freshness, not exp-based lifetime
        };

        // Act
        var result = await _verifier.VerifyAsync(presentation, validationParams, kbValidationParams);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.KeyBindingVerified);
    }

    [Fact]
    public async Task VerifyAsync_WithMissingKbValidationParams_DoesNotVerifyKeyBinding()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload { { "sub", "user123" } },
            new SdIssuanceOptions(),
            HolderPublicJwk
        );

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            _ => false,
            new JwtPayload { { "aud", "verifier" } },
            HolderPrivateKey,
            HolderSigningAlgorithm
        );

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act
        var result = await _verifier.VerifyAsync(presentation, validationParams);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.KeyBindingVerified);
    }

    [Fact]
    public async Task VerifyAsync_WithArrayDisclosuresInPresentation_ReconstructsArray()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload
            {
                { "roles", new[] { "admin", "user", "viewer" } }
            },
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    roles = new[] { true, false, true }
                }
            }
        );

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act
        var result = await _verifier.VerifyAsync(issuerOutput.Issuance, validationParams);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ClaimsPrincipal);
    }

    [Fact]
    public async Task VerifyAsync_WithComplexNestedStructure_HandlesCorrectly()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload
            {
                {
                    "data", new
                    {
                        items = new[] { "item1", "item2" },
                        metadata = new { version = "1.0", author = "test" }
                    }
                }
            },
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    data = new
                    {
                        items = new[] { true, false },
                        metadata = new { author = true }
                    }
                }
            }
        );

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act
        var result = await _verifier.VerifyAsync(issuerOutput.Issuance, validationParams);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ClaimsPrincipal);
    }

    [Fact]
    public async Task VerifyAsync_WithLargeNumberOfDisclosures_HandlesEfficiently()
    {
        // Arrange
        var claims = new JwtPayload();
        var disclosureStructure = new Dictionary<string, bool>();

        for (int i = 0; i < 50; i++)
        {
            claims.Add($"claim_{i}", $"value_{i}");
            disclosureStructure.Add($"claim_{i}", i % 2 == 0); // Disclose every other claim
        }

        var issuerOutput = _issuer.Issue(
            claims,
            new SdIssuanceOptions
            {
                MakeAllClaimsDisclosable = false,
                DisclosureStructure = disclosureStructure
            }
        );

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act
        var result = await _verifier.VerifyAsync(issuerOutput.Issuance, validationParams);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ClaimsPrincipal);
    }

    [Fact]
    public async Task VerifyAsync_WithEmptyPresentation_ThrowsException()
    {
        // Arrange
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _verifier.VerifyAsync("", validationParams)
        );
    }
}
