using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.SiopV2;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.SiopV2.Tests;

public class SelfIssuedIdTokenTests
{
    [Fact]
    public async Task IssueAsync_WithJwkThumbprintSubject_CreatesValidSelfIssuedIdToken()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa) { KeyId = "holder-key-1" };
        var publicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey);
        var issuer = new SelfIssuedIdTokenIssuer(signingKey, SecurityAlgorithms.EcdsaSha256, publicJwk);

        var token = issuer.Issue(new SelfIssuedIdTokenOptions
        {
            Audience = "https://rp.example.com",
            Nonce = "nonce-123",
            Lifetime = TimeSpan.FromMinutes(10)
        });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var expectedSubject = SiopSubject.CreateJwkThumbprintSubject(publicJwk);

        jwt.Issuer.Should().Be(expectedSubject);
        jwt.Payload.Sub.Should().Be(expectedSubject);
        jwt.Payload[SiopConstants.Claims.SubJwk].Should().NotBeNull();

        var validator = new SelfIssuedIdTokenValidator();
        var result = await validator.ValidateAsync(token, new SelfIssuedIdTokenValidationParameters
        {
            ExpectedAudience = "https://rp.example.com",
            ExpectedNonce = "nonce-123"
        });

        result.Subject.Should().Be(expectedSubject);
        result.Payload.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithWrongNonce_ThrowsSecurityTokenException()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa);
        var publicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey);
        var issuer = new SelfIssuedIdTokenIssuer(signingKey, SecurityAlgorithms.EcdsaSha256, publicJwk);
        var token = issuer.Issue(new SelfIssuedIdTokenOptions
        {
            Audience = "rp",
            Nonce = "expected"
        });

        var validator = new SelfIssuedIdTokenValidator();
        var action = () => validator.ValidateAsync(token, new SelfIssuedIdTokenValidationParameters
        {
            ExpectedAudience = "rp",
            ExpectedNonce = "other"
        });

        await action.Should().ThrowAsync<SecurityTokenException>().WithMessage("*nonce*");
    }

    [Fact]
    public async Task ValidateAsync_WithTamperedSubject_ThrowsSecurityTokenException()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa);
        var publicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey);
        var handler = new JwtSecurityTokenHandler();
        var payload = new JwtPayload
        {
            { JwtRegisteredClaimNames.Iss, "not-sub" },
            { JwtRegisteredClaimNames.Sub, SiopSubject.CreateJwkThumbprintSubject(publicJwk) },
            { JwtRegisteredClaimNames.Aud, "rp" },
            { JwtRegisteredClaimNames.Nonce, "nonce" },
            { JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() },
            { SiopConstants.Claims.SubJwk, SiopSubject.CreatePublicJwk(publicJwk) }
        };
        var token = handler.WriteToken(new JwtSecurityToken(
            new JwtHeader(new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256)),
            payload));

        var validator = new SelfIssuedIdTokenValidator();
        var action = () => validator.ValidateAsync(token, new SelfIssuedIdTokenValidationParameters
        {
            ExpectedAudience = "rp",
            ExpectedNonce = "nonce"
        });

        await action.Should().ThrowAsync<SecurityTokenException>().WithMessage("*iss*sub*");
    }

    [Fact]
    public async Task ValidateAsync_WithBareJwkThumbprintSubject_ReturnsValidResult()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa);
        var publicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey);
        var bareSubject = SiopSubject.CreateJwkThumbprint(publicJwk);
        var handler = new JwtSecurityTokenHandler();
        var payload = new JwtPayload
        {
            { JwtRegisteredClaimNames.Iss, bareSubject },
            { JwtRegisteredClaimNames.Sub, bareSubject },
            { JwtRegisteredClaimNames.Aud, "rp" },
            { JwtRegisteredClaimNames.Nonce, "nonce" },
            { JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() },
            { SiopConstants.Claims.SubJwk, SiopSubject.CreatePublicJwk(publicJwk) }
        };
        var token = handler.WriteToken(new JwtSecurityToken(
            new JwtHeader(new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256)),
            payload));

        var validator = new SelfIssuedIdTokenValidator();
        var result = await validator.ValidateAsync(token, new SelfIssuedIdTokenValidationParameters
        {
            ExpectedAudience = "rp",
            ExpectedNonce = "nonce"
        });

        result.Subject.Should().Be(bareSubject);
    }
}
