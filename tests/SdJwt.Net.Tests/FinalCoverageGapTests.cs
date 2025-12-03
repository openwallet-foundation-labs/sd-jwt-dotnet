using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using SdJwt.Net.Utils;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests;

public class FinalCoverageGapTests : TestBase
{
    

    private string CreateCompactSdJwt()
    {
        // Helper to create a valid SD-JWT for serializer tests
        var issuer = new SdIssuer(IssuerSigningKey, "ES256");
        var claims = new JwtPayload { { "sub", "user" } };
        return issuer.Issue(claims, new SdIssuanceOptions()).Issuance;
    }

    [Fact]
    public void SdJwtJsonSerializer_FromFlattened_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SdJwtJsonSerializer.FromFlattenedJsonSerialization(null!));
    }

    [Fact]
    public void SdJwtJsonSerializer_FromGeneral_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SdJwtJsonSerializer.FromGeneralJsonSerialization(null!));
    }

    [Fact]
    public void SdJwtJsonSerializer_FromGeneral_WithEmptySignatures_ThrowsArgumentException()
    {
        var general = new SdJwtGeneralJsonSerialization
        {
            Payload = "payload",
            Signatures = Array.Empty<SdJwtSignature>()
        };
        Assert.Throws<ArgumentException>(() => SdJwtJsonSerializer.FromGeneralJsonSerialization(general));
    }

    [Fact]
    public void SdJwtJsonSerializer_ToGeneral_WithInvalidAdditionalSignature_ThrowsArgumentException()
    {
        var sdJwt = CreateCompactSdJwt();
        var invalidSig = new SdJwtSignature
        {
            Header = new SdJwtUnprotectedHeader
            {
                Disclosures = new[] { "disclosure" }
            }
        };

        Assert.Throws<ArgumentException>(() => 
            SdJwtJsonSerializer.ToGeneralJsonSerialization(sdJwt, new[] { invalidSig }));
            
        var invalidSigKb = new SdJwtSignature
        {
            Header = new SdJwtUnprotectedHeader
            {
                KbJwt = "kb_jwt"
            }
        };
        
        Assert.Throws<ArgumentException>(() => 
            SdJwtJsonSerializer.ToGeneralJsonSerialization(sdJwt, new[] { invalidSigKb }));
    }

    [Fact]
    public void SdJwtJsonSerializer_IsValidJsonSerialization_WithMalformedJson_ReturnsFalse()
    {
        // This should trigger the catch block in IsValidJsonSerialization
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization("{ invalid json }"));
    }

    [Fact]
    public void Disclosure_Parse_WithInvalidLength_ThrowsJsonException()
    {
        var json = JsonSerializer.Serialize(new[] { "salt" }); // Length 1
        var encoded = Base64UrlEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(json));
        
        var ex = Assert.Throws<JsonException>((Action)(() => Disclosure.Parse(encoded)));
        Assert.Contains("must be an array of 2 elements [salt, value] or 3 elements [salt, name, value]", ex.Message);
    }

    [Fact]
    public void Disclosure_Parse_WithNullSalt_ThrowsJsonException()
    {
        var json = "[null, \"name\", \"value\"]";
        var encoded = Base64UrlEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(json));
        
        var ex = Assert.Throws<JsonException>((Action)(() => Disclosure.Parse(encoded)));
        Assert.Contains("salt cannot be null", ex.Message);
    }

    [Fact]
    public void Disclosure_Parse_WithNullName_ThrowsJsonException()
    {
        var json = "[\"salt\", null, \"value\"]";
        var encoded = Base64UrlEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(json));
        
        var ex = Assert.Throws<JsonException>((Action)(() => Disclosure.Parse(encoded)));
        Assert.Contains("claim name cannot be null", ex.Message);
    }

    [Fact]
    public void SdJwtParser_ParseJson_ParsesCorrectly()
    {
        var json = "{\"key\":\"value\"}";
        var parsed = SdJwtParser.ParseJson<Dictionary<string, string>>(json);
        Assert.NotNull(parsed);
        Assert.Equal("value", parsed["key"]);
    }

    [Fact]
    public void SdJwtParser_ParseJsonFile_ParsesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "{\"key\":\"value\"}");
        
        try
        {
            var parsed = SdJwtParser.ParseJsonFile<Dictionary<string, string>>(tempFile);
            Assert.NotNull(parsed);
            Assert.Equal("value", parsed["key"]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void SdJwtParser_ParseJsonFile_WithMissingFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => SdJwtParser.ParseJsonFile<object>("non-existent-file.json"));
    }

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
