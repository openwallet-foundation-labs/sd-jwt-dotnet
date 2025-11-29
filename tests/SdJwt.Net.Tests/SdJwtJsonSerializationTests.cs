using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests.Serialization;

public class SdJwtJsonSerializationTests
{
    private const string TestSdJwt = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9.eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9.signature~WyJzYWx0IiwibmFtZSIsInZhbHVlIl0~";
    private const string TestSdJwtKb = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9.eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9.signature~WyJzYWx0IiwibmFtZSIsInZhbHVlIl0~eyJhbGciOiJFUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJhdWQiOiJ2ZXJpZmllciIsIm5vbmNlIjoiMTIzNCIsImlhdCI6MTYwMDAwMDAwMCwic2RfaGFzaCI6InRlc3RoYXNoIn0.kbsignature";

    [Fact]
    public void ToFlattenedJsonSerialization_ValidSdJwt_ReturnsCorrectFormat()
    {
        // Act
        var result = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9", result.Protected);
        Assert.Equal("eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9", result.Payload);
        Assert.Equal("signature", result.Signature);
        Assert.Single(result.Header.Disclosures);
        Assert.Equal("WyJzYWx0IiwibmFtZSIsInZhbHVlIl0", result.Header.Disclosures[0]);
        Assert.Null(result.Header.KbJwt);
    }

    [Fact]
    public void ToFlattenedJsonSerialization_ValidSdJwtKb_ReturnsCorrectFormat()
    {
        // Act
        var result = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwtKb);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9", result.Protected);
        Assert.Equal("eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9", result.Payload);
        Assert.Equal("signature", result.Signature);
        Assert.Single(result.Header.Disclosures);
        Assert.Equal("WyJzYWx0IiwibmFtZSIsInZhbHVlIl0", result.Header.Disclosures[0]);
        Assert.Equal("eyJhbGciOiJFUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJhdWQiOiJ2ZXJpZmllciIsIm5vbmNlIjoiMTIzNCIsImlhdCI6MTYwMDAwMDAwMCwic2RfaGFzaCI6InRlc3RoYXNoIn0.kbsignature", result.Header.KbJwt);
    }

    [Fact]
    public void FromFlattenedJsonSerialization_ValidJson_ReturnsCorrectSdJwt()
    {
        // Arrange
        var jsonSerialization = new SdJwtJsonSerialization
        {
            Protected = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9",
            Payload = "eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9",
            Signature = "signature",
            Header = new SdJwtUnprotectedHeader
            {
                Disclosures = new[] { "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0" }
            }
        };

        // Act
        var result = SdJwtJsonSerializer.FromFlattenedJsonSerialization(jsonSerialization);

        // Assert
        Assert.Equal(TestSdJwt, result);
    }

    [Fact]
    public void FromFlattenedJsonSerialization_ValidJsonWithKbJwt_ReturnsCorrectSdJwtKb()
    {
        // Arrange
        var jsonSerialization = new SdJwtJsonSerialization
        {
            Protected = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9",
            Payload = "eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9",
            Signature = "signature",
            Header = new SdJwtUnprotectedHeader
            {
                Disclosures = new[] { "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0" },
                KbJwt = "eyJhbGciOiJFUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJhdWQiOiJ2ZXJpZmllciIsIm5vbmNlIjoiMTIzNCIsImlhdCI6MTYwMDAwMDAwMCwic2RfaGFzaCI6InRlc3RoYXNoIn0.kbsignature"
            }
        };

        // Act
        var result = SdJwtJsonSerializer.FromFlattenedJsonSerialization(jsonSerialization);

        // Assert
        Assert.Equal(TestSdJwtKb, result);
    }

    [Fact]
    public void ToGeneralJsonSerialization_ValidSdJwt_ReturnsCorrectFormat()
    {
        // Act
        var result = SdJwtJsonSerializer.ToGeneralJsonSerialization(TestSdJwt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9", result.Payload);
        Assert.Single(result.Signatures);
        Assert.Equal("eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9", result.Signatures[0].Protected);
        Assert.Equal("signature", result.Signatures[0].Signature);
        Assert.Single(result.Signatures[0].Header.Disclosures);
    }

    [Fact]
    public void FromGeneralJsonSerialization_ValidJson_ReturnsCorrectSdJwt()
    {
        // Arrange
        var generalSerialization = new SdJwtGeneralJsonSerialization
        {
            Payload = "eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9",
            Signatures = new[]
            {
                new SdJwtSignature
                {
                    Protected = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9",
                    Signature = "signature",
                    Header = new SdJwtUnprotectedHeader
                    {
                        Disclosures = new[] { "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0" }
                    }
                }
            }
        };

        // Act
        var result = SdJwtJsonSerializer.FromGeneralJsonSerialization(generalSerialization);

        // Assert
        Assert.Equal(TestSdJwt, result);
    }

    [Fact]
    public void RoundTrip_FlattenedJsonSerialization_PreservesOriginal()
    {
        // Act
        var json = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwt);
        var result = SdJwtJsonSerializer.FromFlattenedJsonSerialization(json);

        // Assert
        Assert.Equal(TestSdJwt, result);
    }

    [Fact]
    public void RoundTrip_GeneralJsonSerialization_PreservesOriginal()
    {
        // Act
        var json = SdJwtJsonSerializer.ToGeneralJsonSerialization(TestSdJwt);
        var result = SdJwtJsonSerializer.FromGeneralJsonSerialization(json);

        // Assert
        Assert.Equal(TestSdJwt, result);
    }

    [Fact]
    public void JsonSerialization_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var original = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwt);

        // Act
        var json = JsonSerializer.Serialize(original, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<SdJwtJsonSerialization>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Protected, deserialized.Protected);
        Assert.Equal(original.Payload, deserialized.Payload);
        Assert.Equal(original.Signature, deserialized.Signature);
        Assert.Equal(original.Header.Disclosures, deserialized.Header.Disclosures);
        Assert.Equal(original.Header.KbJwt, deserialized.Header.KbJwt);
    }

    [Fact]
    public void IsValidJsonSerialization_ValidFlattenedJson_ReturnsTrue()
    {
        // Arrange
        var jsonSerialization = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwt);
        var json = JsonSerializer.Serialize(jsonSerialization, SdJwtConstants.DefaultJsonSerializerOptions);

        // Act
        var result = SdJwtJsonSerializer.IsValidJsonSerialization(json);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidJsonSerialization_ValidGeneralJson_ReturnsTrue()
    {
        // Arrange
        var generalSerialization = SdJwtJsonSerializer.ToGeneralJsonSerialization(TestSdJwt);
        var json = JsonSerializer.Serialize(generalSerialization, SdJwtConstants.DefaultJsonSerializerOptions);

        // Act
        var result = SdJwtJsonSerializer.IsValidJsonSerialization(json);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidJsonSerialization_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var invalidJson = "{\"invalid\": \"format\"}";

        // Act
        var result = SdJwtJsonSerializer.IsValidJsonSerialization(invalidJson);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToFlattenedJsonSerialization_NullOrEmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SdJwtJsonSerializer.ToFlattenedJsonSerialization(null!));
        Assert.Throws<ArgumentException>(() => SdJwtJsonSerializer.ToFlattenedJsonSerialization(""));
    }

    [Fact]
    public void FromFlattenedJsonSerialization_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SdJwtJsonSerializer.FromFlattenedJsonSerialization(null!));
    }
}
