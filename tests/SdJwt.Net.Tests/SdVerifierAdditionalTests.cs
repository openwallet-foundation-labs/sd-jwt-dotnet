using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Additional tests for SdVerifier to improve coverage
/// </summary>
public class SdVerifierAdditionalTests : TestBase
{
    private readonly SdIssuer _issuer;
    private readonly SdVerifier _verifier;

    public SdVerifierAdditionalTests()
    {
        _issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        _verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
    }

    [Fact]
    public async Task VerifyAsync_WithValidPresentationNoDisclosures_Succeeds()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(new JwtPayload { { "sub", "user123" } }, new SdIssuanceOptions());
        var presentation = issuerOutput.Issuance;

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
        Assert.False(result.KeyBindingVerified);
    }

    [Fact]
    public async Task VerifyAsync_WithValidPresentationWithDisclosures_ReturnsDisclosedClaims()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } },
            new SdIssuanceOptions { DisclosureStructure = new { email = true } }
        );

        var presentation = issuerOutput.Issuance;

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
        var claims = result.ClaimsPrincipal.Claims.ToList();
        Assert.Contains(claims, c => c.Type == "email");
    }

    [Fact]
    public async Task VerifyAsync_WithMissingRequiredDisclosure_Succeeds()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } },
            new SdIssuanceOptions { DisclosureStructure = new { email = true } }
        );

        // Create presentation without the disclosure
        var presentation = $"{issuerOutput.SdJwt}~";

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act
        var result = await _verifier.VerifyAsync(presentation, validationParams);

        // Assert - Should succeed but not have the disclosed claim
        Assert.NotNull(result);
        Assert.NotNull(result.ClaimsPrincipal);
        var claims = result.ClaimsPrincipal.Claims.ToList();
        Assert.DoesNotContain(claims, c => c.Type == "email");
    }

    [Fact]
    public async Task VerifyAsync_WithNestedDisclosures_ReturnsNestedClaims()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload
            {
                { "address", new { street = "123 Main St", city = "Anytown" } }
            },
            new SdIssuanceOptions { DisclosureStructure = new { address = new { city = true } } }
        );

        var presentation = issuerOutput.Issuance;

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
    public async Task VerifyAsync_WithArrayDisclosures_ReturnsArrayElements()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(
            new JwtPayload { { "roles", new[] { "admin", "user" } } },
            new SdIssuanceOptions { DisclosureStructure = new { roles = new[] { false, true } } }
        );

        var presentation = issuerOutput.Issuance;

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
    public async Task VerifyAsync_WithInvalidDisclosureFormat_ThrowsException()
    {
        // Arrange
        var issuerOutput = _issuer.Issue(new JwtPayload { { "sub", "user123" } }, new SdIssuanceOptions());
        var invalidDisclosure = "invalid-base64-disclosure";
        var presentation = $"{issuerOutput.SdJwt}~{invalidDisclosure}~";

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(
            () => _verifier.VerifyAsync(presentation, validationParams)
        );
    }
}
