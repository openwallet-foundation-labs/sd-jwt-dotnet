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
        // Create a JWT string manually with nbf > exp by crafting the JWT parts directly
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Create header
        var header = new { alg = IssuerSigningAlgorithm, typ = "dc+sd-jwt" };
        var headerJson = System.Text.Json.JsonSerializer.Serialize(header);
        var headerBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(headerJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Create payload with problematic timing claims
        var payload = new
        {
            vct = "https://example.com/vct",
            iss = TrustedIssuer,
            iat = now,
            nbf = now + 100,  // Set nbf after exp
            exp = now + 50,   // Set exp before nbf
            _sd_alg = "sha-256"
        };

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Create signature using the signing key
        var signingInput = $"{headerBase64}.{payloadBase64}";
        var signingCredentials = new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm);
        var cryptoProviderFactory = signingCredentials.Key.CryptoProviderFactory ?? CryptoProviderFactory.Default;
        var signatureProvider = cryptoProviderFactory.CreateForSigning(signingCredentials.Key, signingCredentials.Algorithm);

        var bytesToSign = System.Text.Encoding.UTF8.GetBytes(signingInput);
        var signature = signatureProvider.Sign(bytesToSign);
        var signatureBase64 = Convert.ToBase64String(signature)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var jwt = $"{headerBase64}.{payloadBase64}.{signatureBase64}";

        var holder = new SdJwtHolder($"{jwt}~");
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
        // Create a JWT string manually with iat > exp by crafting the JWT parts directly
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Create header
        var header = new { alg = IssuerSigningAlgorithm, typ = "dc+sd-jwt" };
        var headerJson = System.Text.Json.JsonSerializer.Serialize(header);
        var headerBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(headerJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Create payload with problematic timing claims
        var payload = new
        {
            vct = "https://example.com/vct",
            iss = TrustedIssuer,
            iat = now + 100, // Set iat after exp
            exp = now + 50,  // Set exp before iat
            _sd_alg = "sha-256"
        };

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Create signature using the signing key
        var signingInput = $"{headerBase64}.{payloadBase64}";
        var signingCredentials = new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm);
        var cryptoProviderFactory = signingCredentials.Key.CryptoProviderFactory ?? CryptoProviderFactory.Default;
        var signatureProvider = cryptoProviderFactory.CreateForSigning(signingCredentials.Key, signingCredentials.Algorithm);

        var bytesToSign = System.Text.Encoding.UTF8.GetBytes(signingInput);
        var signature = signatureProvider.Sign(bytesToSign);
        var signatureBase64 = Convert.ToBase64String(signature)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var jwt = $"{headerBase64}.{payloadBase64}.{signatureBase64}";

        var holder = new SdJwtHolder($"{jwt}~");
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
