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
    public async Task VerifyAsync_WithWrongNonceInKbJwt_HowToDetect()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user" } };
        var options = new SdIssuanceOptions();
        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        // var expectedNonce = "correct-nonce"; // Unused
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
            // How to validate nonce here?
            // TokenValidationParameters doesn't have a direct "Nonce" property to validate against.
        };

        // Act
        var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);

        // Assert
        // This confirms the gap: The library verifies the KB-JWT signature and sd_hash,
        // but ignores the nonce mismatch because it doesn't check it.
        Assert.True(result.KeyBindingVerified);
        
        // To be compliant, we should probably expose the KB-JWT claims so the caller can verify nonce.
    }
}
