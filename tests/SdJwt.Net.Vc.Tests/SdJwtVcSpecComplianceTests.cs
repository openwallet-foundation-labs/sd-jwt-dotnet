using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

public class SdJwtVcSpecComplianceTests : TestBase
{
    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenTypHeaderIsInvalid()
    {
        // 1. Create a valid SD-JWT VC but with wrong typ header
        var header = new { alg = "ES256", typ = "JWT" }; // Wrong typ, should be dc+sd-jwt
        var payload = new 
        { 
            vct = "https://example.com/vct",
            iss = TrustedIssuer,
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            _sd_alg = "sha-256"
        };

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);
        var headerBase64 = Base64UrlEncoder.Encode(headerJson);
        var payloadBase64 = Base64UrlEncoder.Encode(payloadJson);
        var contentToSign = $"{headerBase64}.{payloadBase64}";

        using var ecdsa = ECDsa.Create();
        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = "test-key" };
        var input = Encoding.UTF8.GetBytes(contentToSign);
        var signature = ecdsa.SignData(input, HashAlgorithmName.SHA256);
        var signatureBase64 = Base64UrlEncoder.Encode(signature);

        var presentation = $"{headerBase64}.{payloadBase64}.{signatureBase64}~";
        
        var verifier = new SdJwtVcVerifier(_ => Task.FromResult<SecurityKey>(securityKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = securityKey
        };

        // 2. Verify - Expect failure
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("typ", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenIssClaimIsMissing()
    {
        // 1. Create a valid SD-JWT VC but missing iss claim
        var header = new { alg = "ES256", typ = "dc+sd-jwt" };
        var payload = new 
        { 
            vct = "https://example.com/vct",
            // iss missing
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            _sd_alg = "sha-256"
        };

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);
        var headerBase64 = Base64UrlEncoder.Encode(headerJson);
        var payloadBase64 = Base64UrlEncoder.Encode(payloadJson);
        var contentToSign = $"{headerBase64}.{payloadBase64}";

        using var ecdsa = ECDsa.Create();
        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = "test-key" };
        var input = Encoding.UTF8.GetBytes(contentToSign);
        var signature = ecdsa.SignData(input, HashAlgorithmName.SHA256);
        var signatureBase64 = Base64UrlEncoder.Encode(signature);

        var presentation = $"{headerBase64}.{payloadBase64}.{signatureBase64}~";
        
        var verifier = new SdJwtVcVerifier(_ => Task.FromResult<SecurityKey>(securityKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false, // We disable framework validation to check our manual check
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = securityKey
        };

        // 2. Verify - Expect failure
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("iss", ex.Message);
    }
}
