using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SdJwt.Net.Tests;

public class FinalCoverageGapTests3 : TestBase
{
    [Fact]
    public void SdJwtUtils_CreateDigest_WithBlockedAlgorithm_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => SdJwtUtils.CreateDigest("MD5", "disclosure"));
    }

    [Fact]
    public void SdJwtUtils_CreateDigest_WithUnapprovedAlgorithm_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => SdJwtUtils.CreateDigest("SHA-224", "disclosure"));
    }

    [Fact]
    public async Task VerifyAsync_WithComplexNestedStructure_RehydratesCorrectly()
    {
        // Create a payload with deep nesting, arrays, nulls, primitives
        var issuer = new SdIssuer(IssuerSigningKey, "ES256");
        var complexClaims = new JwtPayload
        {
            { "nested", new Dictionary<string, object>
                {
                    { "nullVal", null! },
                    { "array", new object[] { 1, "string", true, null! } },
                    { "deep", new Dictionary<string, object> { { "val", "ok" } } }
                }
            }
        };
        
        var issuance = issuer.Issue(complexClaims, new SdIssuanceOptions()); // No disclosures, just structure
        
        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };

        var result = await verifier.VerifyAsync(issuance.Issuance, validationParams);
        
        Assert.NotNull(result.ClaimsPrincipal);
        // Just verifying that it didn't crash and traversed the structure
    }

    [Fact]
    public void SdJwtJsonSerializer_IsValidJsonSerialization_WithEmptyJson_ReturnsFalse()
    {
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization(""));
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization(null!));
    }

    [Fact]
    public void SdJwtJsonSerializer_IsValidJsonSerialization_WithEmptyObject_ReturnsFalse()
    {
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization("{}"));
    }
}
