using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace SdJwt.Net.Tests;

public class KbJwtVerificationTests : TestBase
{
    [Fact]
    public async Task VerifyAsync_WithWrongNonceInKbJwt_ThrowsSecurityTokenException()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user" } };
        var options = new SdIssuanceOptions();
        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var expectedNonce = "correct-nonce";
        var wrongNonce = "wrong-nonce";
        
        // Create presentation with WRONG nonce in KB-JWT
        var presentation = holder.CreatePresentation(
            _ => false,
            new JwtPayload { { "aud", "verifier" }, { "nonce", wrongNonce } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
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
            ValidateAudience = true,
            ValidAudience = "verifier",
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => 
            verifier.VerifyAsync(presentation, validationParams, kbValidationParams, expectedNonce));
            
        Assert.Contains("Nonce in Key Binding JWT", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_WithCorrectNonceInKbJwt_VerifiesSuccessfully()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user" } };
        var options = new SdIssuanceOptions();
        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var expectedNonce = "correct-nonce";
        
        // Create presentation with CORRECT nonce in KB-JWT
        var presentation = holder.CreatePresentation(
            _ => false,
            new JwtPayload { { "aud", "verifier" }, { "nonce", expectedNonce } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
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
            ValidateAudience = true,
            ValidAudience = "verifier",
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        // Act
        var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams, expectedNonce);

        // Assert
        Assert.True(result.KeyBindingVerified);
        Assert.NotNull(result.KeyBindingJwtPayload);
        Assert.Equal(expectedNonce, result.KeyBindingJwtPayload["nonce"]);
    }
}
