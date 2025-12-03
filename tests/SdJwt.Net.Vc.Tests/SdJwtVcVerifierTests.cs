using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using System.Security.Claims;

namespace SdJwt.Net.Vc.Tests;

public class SdJwtVcVerifierTests : TestBase
{
    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenVctIsNotCollisionResistant()
    {
        // Use base SdIssuer to bypass SdJwtVcIssuer validation
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "invalid_vct" }, // Invalid VCT
            { "iss", TrustedIssuer },
            { "sub", "did:example:test" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };
        
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "dc+sd-jwt");
        
        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => false, null, null, null);
        
        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("not a valid Collision-Resistant Name", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenNbfIsAfterExp()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer },
            { "nbf", now + 100 },
            { "exp", now + 50 } // Exp before Nbf
        };
        
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "dc+sd-jwt");
        
        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => false, null, null, null);
        
        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Not-before time must be before expiration time", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenIatIsAfterExp()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer },
            { "iat", now + 100 },
            { "exp", now + 50 } // Exp before Iat
        };
        
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "dc+sd-jwt");
        
        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => false, null, null, null);
        
        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Issued-at time must be before expiration time", ex.Message);
    }
}
