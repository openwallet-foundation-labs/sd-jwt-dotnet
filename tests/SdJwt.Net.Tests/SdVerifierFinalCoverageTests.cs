using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SdJwt.Net.Tests;

public class SdVerifierFinalCoverageTests : TestBase
{
    private string CreateSignedToken(JwtPayload payload)
    {
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256));
        var token = new JwtSecurityToken(header, payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task VerifyAsync_WithDuplicateDisclosures_ThrowsSecurityTokenException()
    {
        // Arrange
        var disclosure = new Disclosure(SdJwtUtils.GenerateSalt(), "claim", "value");
        var digest = SdJwtUtils.CreateDigest("sha-256", disclosure.EncodedValue);

        // Create SD-JWT with this digest
        var claims = new JwtPayload
        {
            { "_sd", new[] { digest } }
        };
        var sdJwt = CreateSignedToken(claims);

        // Presentation has the SAME disclosure twice
        var presentation = $"{sdJwt}~{disclosure.EncodedValue}~{disclosure.EncodedValue}~";

        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams));

        Assert.Contains("Duplicate disclosure detected", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_KeyBindingJwtMissingSdHash_ThrowsSecurityTokenException()
    {
        // Arrange
        var holderJwk = new Dictionary<string, object?>
        {
            ["kty"] = HolderPublicJwk.Kty,
            ["crv"] = HolderPublicJwk.Crv,
            ["x"] = HolderPublicJwk.X,
            ["y"] = HolderPublicJwk.Y,
            ["kid"] = HolderPublicJwk.Kid
        };

        var sdJwt = CreateSignedToken(new JwtPayload
        {
            { "cnf", new Dictionary<string, object?> { { "jwk", holderJwk } } }
        });

        // Create KB-JWT without sd_hash
        var kbJwtPayload = new JwtPayload
        {
            { "nonce", "123" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };
        var kbJwtHeader = new JwtHeader(new SigningCredentials(HolderPrivateKey, SecurityAlgorithms.EcdsaSha256));
        kbJwtHeader["typ"] = "kb+jwt";
        var kbJwt = new JwtSecurityToken(kbJwtHeader, kbJwtPayload);
        var kbJwtString = new JwtSecurityTokenHandler().WriteToken(kbJwt);

        var presentation = $"{sdJwt}~{kbJwtString}";

        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };
        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey,
            ValidateIssuerSigningKey = true
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams, kbValidationParams));

        Assert.Contains("Key Binding JWT is missing the required 'sd_hash' claim", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_WithSingleStringSdClaim_ThrowsSecurityTokenException()
    {
        // Arrange
        var disclosure = new Disclosure(SdJwtUtils.GenerateSalt(), "claim", "value");
        var digest = SdJwtUtils.CreateDigest("sha-256", disclosure.EncodedValue);

        // Create SD-JWT with _sd as a SINGLE STRING (not array)
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256));
        var payloadJson = $"{{\"_sd\":\"{digest}\",\"iss\":\"test\"}}";
        var payload = JwtPayload.Deserialize(payloadJson);
        var token = new JwtSecurityToken(header, payload);
        var sdJwt = new JwtSecurityTokenHandler().WriteToken(token);

        var presentation = $"{sdJwt}~{disclosure.EncodedValue}~";

        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams));
    }

    [Fact]
    public async Task VerifyJsonSerializationAsync_WithNullOrEmpty_ThrowsArgumentException()
    {
        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters();

        await Assert.ThrowsAsync<ArgumentException>(() => verifier.VerifyJsonSerializationAsync(null!, validationParams));
        await Assert.ThrowsAsync<ArgumentException>(() => verifier.VerifyJsonSerializationAsync("", validationParams));
    }

    [Fact]
    public async Task VerifyJsonSerializationAsync_WithInvalidJson_ThrowsArgumentException()
    {
        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters();

        await Assert.ThrowsAsync<ArgumentException>(() => verifier.VerifyJsonSerializationAsync("invalid-json", validationParams));
    }

    [Fact]
    public async Task VerifyJsonSerializationAsync_WithUnparseableJson_ThrowsArgumentException()
    {
        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters();

        // Valid JSON but not matching flattened or general structure
        var json = "{}";
        await Assert.ThrowsAsync<ArgumentException>(() => verifier.VerifyJsonSerializationAsync(json, validationParams));
    }

    [Fact]
    public async Task VerifyAsync_WithArrayItemDigestNotFound_RemovesPlaceholder()
    {
        // Arrange
        // Create SD-JWT with array item having "..." digest, but DO NOT provide the disclosure
        var digest = "digest-not-found";
        var claims = new JwtPayload
        {
            { "my_array", new[] { new Dictionary<string, object> { { "...", digest } } } }
        };
        var sdJwt = CreateSignedToken(claims);

        var presentation = $"{sdJwt}~"; // No disclosures

        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };

        // Act
        var result = await verifier.VerifyAsync(presentation, validationParams);

        // Assert
        Assert.True(result.ClaimsPrincipal.HasClaim(c => c.Type == "my_array"));
        var claimValue = result.ClaimsPrincipal.FindFirst("my_array")!.Value;
        Assert.DoesNotContain("...", claimValue);
        Assert.DoesNotContain(digest, claimValue);
    }
}
