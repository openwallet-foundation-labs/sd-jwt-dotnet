using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.Vc.Issuer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Xunit;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Holder;
using SdJwt.Net.Serialization;
using SdJwt.Net.Vc.Models;

namespace SdJwt.Net.Vc.Tests;

public class SdJwtVcVerifierEnhancedTests : TestBase
{
    [Fact]
    public async Task VerifyJsonSerializationAsync_ShouldWork()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            AdditionalData = new Dictionary<string, object> { { "name", "John" } }
        };
        var options = new SdIssuanceOptions { DisclosureStructure = new { name = true } };

        var output = vcIssuer.Issue("https://example.com/vct", payload, options, HolderPublicJwk);

        // Holder creates JSON serialization
        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(
            disclosure => true,
            new JwtPayload { { "aud", "https://verifier.com" }, { "nonce", "123" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        var jsonObject = SdJwtJsonSerializer.ToFlattenedJsonSerialization(presentation);
        var json = JsonSerializer.Serialize(jsonObject, SdJwtConstants.DefaultJsonSerializerOptions);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false,
            ValidateLifetime = false
        };
        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "https://verifier.com",
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        var result = await verifier.VerifyJsonSerializationAsync(json, validationParams, kbValidationParams);

        Assert.NotNull(result);
        Assert.Equal("https://example.com/vct", result.VerifiableCredentialType);
        Assert.True(result.KeyBindingVerified);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenMissingVct()
    {
        // Issue a normal SD-JWT without vct claim
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "iss", TrustedIssuer },
            { "sub", "did:example:123" }
        };
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "dc+sd-jwt");

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Missing required 'vct' claim", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenVctMismatch()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            AdditionalData = new Dictionary<string, object> { { "name", "John" } }
        };
        var output = vcIssuer.Issue("https://example.com/vct", payload, new SdIssuanceOptions());

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams, expectedVctType: "https://expected.com/vct"));
        Assert.Contains("Expected VCT type", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_ShouldHandleInvalidComplexClaims()
    {
        // Manually create payload with invalid JSON for cnf and status
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer },
            { "cnf", "{ invalid_json" },
            { "status", "{ invalid_json" }
        };

        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "dc+sd-jwt");
        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        // Should not throw, just log warning and ignore invalid claims
        var result = await verifier.VerifyAsync(presentation, validationParams);

        Assert.NotNull(result);
        Assert.Null(result.SdJwtVcPayload.Confirmation);
        Assert.Null(result.SdJwtVcPayload.Status);
    }

    [Fact]
    public void GetNumericDateClaim_ShouldParseStringDates()
    {
        // This test uses reflection to verify the private GetNumericDateClaim method
        // which is hard to reach via public API due to strict JWT parsing in the base library.

        var methodInfo = typeof(SdJwtVcVerifier).GetMethod("GetNumericDateClaim", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(methodInfo);

        var now = DateTimeOffset.UtcNow;
        var nowSeconds = now.ToUnixTimeSeconds();

        // Case 1: Numeric claim (long)
        var identityNumeric = new ClaimsIdentity();
        identityNumeric.AddClaim(new Claim("iat", nowSeconds.ToString(), ClaimValueTypes.Integer64));
        var principalNumeric = new ClaimsPrincipal(identityNumeric);

        var resultNumeric = (long?)methodInfo.Invoke(null, new object[] { principalNumeric, "iat" });
        Assert.Equal(nowSeconds, resultNumeric);

        // Case 2: String claim (date string)
        var identityString = new ClaimsIdentity();
        identityString.AddClaim(new Claim("nbf", now.ToString("o"), ClaimValueTypes.String));
        var principalString = new ClaimsPrincipal(identityString);

        var resultString = (long?)methodInfo.Invoke(null, new object[] { principalString, "nbf" });
        // The parser converts to Unix time seconds, precision might be lost
        Assert.Equal(nowSeconds, resultString);

        // Case 3: Missing claim
        var principalEmpty = new ClaimsPrincipal(new ClaimsIdentity());
        var resultEmpty = (long?)methodInfo.Invoke(null, new object[] { principalEmpty, "exp" });
        Assert.Null(resultEmpty);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenVctIsNotCollisionResistant()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var jwtPayload = new JwtPayload
        {
            { "vct", "simple_name" }, // Invalid
            { "iss", TrustedIssuer },
            { "sub", "did:example:123" }
        };
        var output = issuer.Issue(jwtPayload, new SdIssuanceOptions(), null, "dc+sd-jwt");

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

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
    public async Task VerifyAsync_ShouldThrow_WhenNbfAfterExp()
    {
        var now = DateTime.UtcNow;
        var nbf = (long)(now.AddHours(2) - DateTime.UnixEpoch).TotalSeconds;
        var exp = (long)(now.AddHours(1) - DateTime.UnixEpoch).TotalSeconds;

        var header = new { alg = "ES256", typ = "dc+sd-jwt" };
        var payload = new
        {
            vct = "https://example.com/vct",
            iss = TrustedIssuer,
            nbf = nbf,
            exp = exp,
            _sd_alg = "sha-256"
        };

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);
        var headerBase64 = Base64UrlEncoder.Encode(headerJson);
        var payloadBase64 = Base64UrlEncoder.Encode(payloadJson);
        var contentToSign = $"{headerBase64}.{payloadBase64}";

        // Use a local key for signing to ensure we have the private key
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
            ValidateLifetime = false // Disable standard lifetime check to reach our custom check
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Not-before time must be before expiration time", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenIatAfterExp()
    {
        var now = DateTime.UtcNow;
        var iat = (long)(now.AddHours(2) - DateTime.UnixEpoch).TotalSeconds;
        var exp = (long)(now.AddHours(1) - DateTime.UnixEpoch).TotalSeconds;

        var header = new { alg = "ES256", typ = "dc+sd-jwt" };
        var payload = new
        {
            vct = "https://example.com/vct",
            iss = TrustedIssuer,
            iat = iat,
            exp = exp,
            _sd_alg = "sha-256"
        };

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);
        var headerBase64 = Base64UrlEncoder.Encode(headerJson);
        var payloadBase64 = Base64UrlEncoder.Encode(payloadJson);
        var contentToSign = $"{headerBase64}.{payloadBase64}";

        // Use a local key for signing to ensure we have the private key
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
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Issued-at time must be before expiration time", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrow_WhenVctIntegrityPresentButNoResolver()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var jwtPayload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "vct#integrity", "some-hash" },
            { "iss", TrustedIssuer }
        };
        var output = issuer.Issue(jwtPayload, new SdIssuanceOptions(), null, "dc+sd-jwt");

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("type metadata resolver", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
