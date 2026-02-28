using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Metadata;
using SdJwt.Net.Vc.Verifier;
using Xunit;
using VcModels = SdJwt.Net.Vc.Models;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Unit tests for SdJwtVcVerifier.
/// </summary>
public class SdJwtVcVerifierCoverageTests : TestBase
{
    [Fact]
    public void SdJwtVcVerifier_ThrowsOnNullIssuerKeyResolver()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SdJwtVcVerifier((IJwtVcIssuerSigningKeyResolver)null!));
    }

    [Fact]
    public void SdJwtVcVerifier_ThrowsOnNullMetadataResolver()
    {
        using var httpClient = new HttpClient();
        Assert.Throws<ArgumentNullException>(() =>
            new SdJwtVcVerifier((IJwtVcIssuerMetadataResolver)null!, httpClient));
    }

    [Fact]
    public void SdJwtVcVerifier_ThrowsOnNullHttpClient()
    {
        var mockMetadataResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver();
        Assert.Throws<ArgumentNullException>(() =>
            new SdJwtVcVerifier(mockMetadataResolver, null!));
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnMissingTypHeader()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer }
        };
        // Issue without typ header
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, null);

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
            verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("typ", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnInvalidTypHeader()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer }
        };
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "invalid-typ");

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
            verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("typ", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_AcceptsLegacyTypWhenAllowed()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new VcModels.SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123"
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
        var policy = new SdJwtVcVerificationPolicy { AcceptLegacyTyp = true };

        var result = await verifier.VerifyAsync(presentation, validationParams, verificationPolicy: policy);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SdJwtVcVerifier_RejectsLegacyTypWhenNotAllowed()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer }
        };
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "vc+sd-jwt");

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };
        var policy = new SdJwtVcVerificationPolicy { AcceptLegacyTyp = false };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams, verificationPolicy: policy));
        Assert.Contains("typ", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnMissingIss()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" }
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

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("iss", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnMissingVct()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "iss", TrustedIssuer }
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

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("vct", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnInvalidVct()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "invalid" },
            { "iss", TrustedIssuer }
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

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Collision-Resistant Name", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ValidatesVctFromPolicy()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new VcModels.SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123"
        };
        var output = vcIssuer.Issue("https://example.com/actual-vct", payload, new SdIssuanceOptions());

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };
        var policy = new SdJwtVcVerificationPolicy
        {
            ExpectedVctType = "https://example.com/expected-vct"
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams, verificationPolicy: policy));
        Assert.Contains("Expected VCT type", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnStatusValidationRequired()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new VcModels.SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            Status = new { status_list = new { uri = "https://status.example.com" } }
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
        var policy = new SdJwtVcVerificationPolicy
        {
            RequireStatusCheck = true
            // No status validator configured
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams, verificationPolicy: policy));
        Assert.Contains("status validator", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnTypeMetadataRequired()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new VcModels.SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            VctIntegrity = "sha-256-test"
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
        var policy = new SdJwtVcVerificationPolicy
        {
            RequireTypeMetadata = true
            // No type metadata resolver configured
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams, verificationPolicy: policy));
        Assert.Contains("type metadata resolver", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_VerifyForOid4Vp_ThrowsOnMissingNonce()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new VcModels.SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123"
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
            verifier.VerifyForOid4VpAsync(presentation, validationParams, "", "https://verifier.example.com"));
        Assert.Contains("Nonce", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_VerifyForOid4Vp_ThrowsOnMissingKbJwt()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new VcModels.SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123"
        };
        var output = vcIssuer.Issue("https://example.com/vct", payload, new SdIssuanceOptions());

        // Create presentation without KB-JWT
        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyForOid4VpAsync(presentation, validationParams, "nonce123", "https://verifier.example.com"));
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnIatAfterExp()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer },
            { "iat", now + 1000 },
            { "exp", now + 500 }
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

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Issued-at time must be before expiration time", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ThrowsOnNbfAfterExp()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" },
            { "iss", TrustedIssuer },
            { "iat", now },
            { "nbf", now + 1000 },
            { "exp", now + 500 }
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

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyAsync(presentation, validationParams));
        Assert.Contains("Not-before time must be before expiration time", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_ParsesAdditionalClaims()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new VcModels.SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            AdditionalData = new Dictionary<string, object>
            {
                ["given_name"] = "John",
                ["family_name"] = "Doe",
                ["age"] = 30
            }
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

        var result = await verifier.VerifyAsync(presentation, validationParams);

        Assert.NotNull(result.SdJwtVcPayload.AdditionalData);
        Assert.True(result.SdJwtVcPayload.AdditionalData!.ContainsKey("given_name"));
    }

    [Fact]
    public async Task SdJwtVcVerifier_VerifyJsonSerializationAsync_ThrowsOnMissingVct()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "iss", TrustedIssuer }
        };
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "dc+sd-jwt");

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var jsonObject = SdJwt.Net.Serialization.SdJwtJsonSerializer.ToFlattenedJsonSerialization(presentation);
        var json = JsonSerializer.Serialize(jsonObject, SdJwtConstants.DefaultJsonSerializerOptions);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyJsonSerializationAsync(json, validationParams));
        Assert.Contains("vct", ex.Message);
    }

    [Fact]
    public async Task SdJwtVcVerifier_VerifyJsonSerializationAsync_ThrowsOnMissingIss()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new JwtPayload
        {
            { "vct", "https://example.com/vct" }
        };
        var output = issuer.Issue(payload, new SdIssuanceOptions(), null, "dc+sd-jwt");

        var holder = new SdJwtHolder(output.Issuance);
        var presentation = holder.CreatePresentation(_ => true, null, null, null);

        var jsonObject = SdJwt.Net.Serialization.SdJwtJsonSerializer.ToFlattenedJsonSerialization(presentation);
        var json = JsonSerializer.Serialize(jsonObject, SdJwtConstants.DefaultJsonSerializerOptions);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() =>
            verifier.VerifyJsonSerializationAsync(json, validationParams));
        Assert.Contains("iss", ex.Message);
    }
}
