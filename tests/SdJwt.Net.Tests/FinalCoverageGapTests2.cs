using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests;

public class FinalCoverageGapTests2 : TestBase
{
    // ... tests ...

    private string CreateCompactSdJwt()
    {
        // Helper to create a valid SD-JWT
        var issuer = new SdIssuer(IssuerSigningKey, "ES256");
        var claims = new JwtPayload { { "sub", "user" } };
        return issuer.Issue(claims, new SdIssuanceOptions()).Issuance;
    }
    [Fact]
    public async Task VerifyJsonSerializationAsync_WithVerificationFailure_WrapsInSecurityTokenException()
    {
        // Create a valid JSON serialization but with a signature that will fail verification
        var sdJwt = CreateCompactSdJwt();
        var flattened = SdJwtJsonSerializer.ToFlattenedJsonSerialization(sdJwt);
        
        // Tamper with signature
        flattened.Signature = flattened.Signature + "tampered";
        var json = JsonSerializer.Serialize(flattened);
        
        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };

        // This should trigger the catch block in VerifyJsonSerializationAsync
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => 
            verifier.VerifyJsonSerializationAsync(json, validationParams));
            
        Assert.Contains("Failed to verify SD-JWT", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_WithSingleStringSdClaimAndMissingDisclosure_IgnoresIt()
    {
        // Arrange
        var digest = "digest-not-found";
        // Create SD-JWT with _sd as a SINGLE STRING (not array)
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256));
        var payloadJson = $"{{\"_sd\":\"{digest}\",\"iss\":\"test\"}}";
        var payload = JwtPayload.Deserialize(payloadJson);
        var token = new JwtSecurityToken(header, payload);
        var sdJwt = new JwtSecurityTokenHandler().WriteToken(token);
        
        var presentation = $"{sdJwt}~"; // No disclosures
        
        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };

        // Act
        var result = await verifier.VerifyAsync(presentation, validationParams);

        // Assert
        // The claim should NOT be rehydrated (it remains as _sd claim? No, _sd is removed at the end)
        // Wait, RehydrateNode handles _sd claim.
        // If digest not found, it does nothing.
        // Then _sd claim is removed at line 118.
        // So the result should NOT have the claim.
        Assert.False(result.ClaimsPrincipal.HasClaim(c => c.Type == "claim"));
    }

    [Fact]
    public void SdJwtUtils_CreateDigest_SupportsSha384AndSha512()
    {
        var disclosure = "disclosure";
        var digest384 = SdJwtUtils.CreateDigest("SHA-384", disclosure);
        Assert.NotNull(digest384);
        
        var digest512 = SdJwtUtils.CreateDigest("SHA-512", disclosure);
        Assert.NotNull(digest512);
    }

    [Fact]
    public void SdJwtUtils_ConvertJsonElement_HandlesAllTypes()
    {
        // Number
        var jsonNumber = "123.45";
        var docNumber = JsonDocument.Parse(jsonNumber);
        var resultNumber = SdJwtUtils.ConvertJsonElement(docNumber.RootElement);
        Assert.IsType<decimal>(resultNumber);
        Assert.Equal(123.45m, resultNumber);

        // True
        var jsonTrue = "true";
        var docTrue = JsonDocument.Parse(jsonTrue);
        var resultTrue = SdJwtUtils.ConvertJsonElement(docTrue.RootElement);
        Assert.IsType<bool>(resultTrue);
        Assert.True((bool)resultTrue);

        // False
        var jsonFalse = "false";
        var docFalse = JsonDocument.Parse(jsonFalse);
        var resultFalse = SdJwtUtils.ConvertJsonElement(docFalse.RootElement);
        Assert.IsType<bool>(resultFalse);
        Assert.False((bool)resultFalse);

        // Null
        var jsonNull = "null";
        var docNull = JsonDocument.Parse(jsonNull);
        var resultNull = SdJwtUtils.ConvertJsonElement(docNull.RootElement);
        Assert.Null(resultNull);

        // Object
        var jsonObject = "{\"key\":\"value\"}";
        var docObject = JsonDocument.Parse(jsonObject);
        var resultObject = SdJwtUtils.ConvertJsonElement(docObject.RootElement);
        Assert.IsType<Dictionary<string, object>>(resultObject);
        
        // Array
        var jsonArray = "[\"value\"]";
        var docArray = JsonDocument.Parse(jsonArray);
        var resultArray = SdJwtUtils.ConvertJsonElement(docArray.RootElement);
        Assert.IsType<List<object>>(resultArray);
    }

}
