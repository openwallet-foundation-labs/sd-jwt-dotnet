using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Basic tests for SD-JWT VC compliance with draft-ietf-oauth-sd-jwt-vc-13
/// </summary>
public class SdJwtVcComplianceTests : TestBase
{
    [Fact]
    public async Task SdJwtVc_BasicIssuanceAndVerification_ShouldWork()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);

        var vcPayload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123456789abcdefghi",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            NotBefore = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                { "given_name", "John" },
                { "family_name", "Doe" },
                { "email", "john.doe@example.com" },
                { "degree", "Bachelor of Science" },
                { "gpa", "3.8" }
            }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                given_name = true,
                family_name = true,
                email = true,
                gpa = true  // Keep degree always visible
            }
        };

        var issuerOutput = vcIssuer.Issue("https://university.example.edu/credentials/degree", vcPayload, options, HolderPublicJwk);

        // Verify the VC structure
        var jwt = new JwtSecurityToken(issuerOutput.SdJwt);
        Assert.Equal(TrustedIssuer, jwt.Payload.Iss);
        Assert.Equal("did:example:123456789abcdefghi", jwt.Payload.Sub);
        Assert.Contains(jwt.Payload.Claims, c => c.Type == "vct" && c.Value == "https://university.example.edu/credentials/degree");
        Assert.Contains(jwt.Payload.Claims, c => c.Type == "_sd_alg");

        // Holder creates selective disclosure
        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "given_name" || disclosure.ClaimName == "email",
            new JwtPayload { { "aud", "https://verifier.university.edu" }, { "nonce", "challenge-123" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        // Verifier validates
        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var kbJwtValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "https://verifier.university.edu",
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        var result = await verifier.VerifyAsync(presentation, validationParams, kbJwtValidationParams);

        Assert.NotNull(result);
        Assert.True(result.KeyBindingVerified);
        Assert.Equal(TrustedIssuer, result.SdJwtVcPayload.Issuer);
        Assert.Equal("https://university.example.edu/credentials/degree", result.VerifiableCredentialType);
        Assert.Equal("did:example:123456789abcdefghi", result.SdJwtVcPayload.Subject);

        // Check disclosed claims
        Assert.True(result.SdJwtVcPayload.AdditionalData?.ContainsKey("given_name") == true);
        Assert.True(result.SdJwtVcPayload.AdditionalData?.ContainsKey("email") == true);
        Assert.True(result.SdJwtVcPayload.AdditionalData?.ContainsKey("degree") == true); // Always visible
        Assert.False(result.SdJwtVcPayload.AdditionalData?.ContainsKey("family_name") == true); // Not disclosed
        Assert.False(result.SdJwtVcPayload.AdditionalData?.ContainsKey("gpa") == true); // Not disclosed
    }

    [Fact]
    public void SdJwtVc_PayloadValidation_ShouldEnforceRequiredFields()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);

        // Empty VCT should throw
        Assert.Throws<ArgumentException>(() =>
        {
            var invalidPayload = new SdJwtVcPayload { Issuer = TrustedIssuer };
            vcIssuer.Issue("", invalidPayload, new SdIssuanceOptions());
        });
    }

    [Fact]
    public void SdJwtVc_MultipleTypes_ShouldWork()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);

        var vcPayload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:holder",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                { "qualification", "Certified Developer" },
                { "level", "Senior" }
            }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { level = true }
        };

        var issuerOutput = vcIssuer.Issue("https://example.org/credentials/professional", vcPayload, options);

        var jwt = new JwtSecurityToken(issuerOutput.SdJwt);
        Assert.Contains(jwt.Payload.Claims, c => c.Type == "vct" && c.Value == "https://example.org/credentials/professional");
    }

    [Fact]
    public void Issue_ShouldSetCorrectTypHeader()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var vcPayload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:test",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object> { { "test", "value" } }
        };
        var options = new SdIssuanceOptions();

        var output = vcIssuer.Issue("https://example.com/vct", vcPayload, options);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(output.SdJwt);

        Assert.Equal("dc+sd-jwt", token.Header.Typ);
    }

    [Fact]
    public async Task SdJwtVc_InvalidVerification_ShouldFail()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);

        var vcPayload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:test",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object> { { "test", "value" } }
        };

        var issuerOutput = vcIssuer.Issue("https://example.com/test-credential", vcPayload, new SdIssuanceOptions(), HolderPublicJwk);

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            _ => false,
            new JwtPayload { { "aud", "wrong-audience" }, { "nonce", "test" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "correct-audience", // Different from presentation
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        await Assert.ThrowsAsync<SecurityTokenInvalidAudienceException>(
            () => verifier.VerifyAsync(presentation, validationParams, kbValidationParams));
    }
}
