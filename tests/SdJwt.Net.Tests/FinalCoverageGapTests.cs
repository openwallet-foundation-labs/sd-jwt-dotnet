using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using SdJwt.Net.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests;

public class FinalCoverageGapTests : TestBase
{
    // ... tests ...

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
        var json = JsonSerializer.Serialize(new[] { "salt", "name" }); // Length 2
        var encoded = Base64UrlEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(json));
        
        var ex = Assert.Throws<JsonException>((Action)(() => Disclosure.Parse(encoded)));
        Assert.Contains("must be an array of 3 elements", ex.Message);
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

}
