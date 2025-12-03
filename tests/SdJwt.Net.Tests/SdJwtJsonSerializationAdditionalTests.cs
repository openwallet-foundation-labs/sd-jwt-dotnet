using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests.Serialization;

/// <summary>
/// Additional tests for SdJwtJsonSerializer to improve coverage
/// </summary>
public class SdJwtJsonSerializationAdditionalTests
{
    private const string TestSdJwt = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9.eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9.signature~WyJzYWx0IiwibmFtZSIsInZhbHVlIl0~";

    [Fact]
    public void ToGeneralJsonSerialization_WithAdditionalSignatures_IncludesAll()
    {
        // Arrange
        var additionalSignature = new SdJwtSignature
        {
            Protected = "eyJhbGciOiJFUzI1NiJ9",
            Signature = "additional-sig",
            Header = new SdJwtUnprotectedHeader()
        };

        // Act
        var result = SdJwtJsonSerializer.ToGeneralJsonSerialization(TestSdJwt, new[] { additionalSignature });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Signatures.Length);
        Assert.Equal("additional-sig", result.Signatures[1].Signature);
    }

    [Fact]
    public void ToGeneralJsonSerialization_WithNullAdditionalSignatures_WorksCorrectly()
    {
        // Act
        var result = SdJwtJsonSerializer.ToGeneralJsonSerialization(TestSdJwt, null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Signatures);
    }

    [Fact]
    public void FromGeneralJsonSerialization_WithNullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SdJwtJsonSerializer.FromGeneralJsonSerialization(null!));
    }

    [Fact]
    public void ToGeneralJsonSerialization_WithInvalidSdJwt_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => SdJwtJsonSerializer.ToGeneralJsonSerialization(""));
        Assert.Throws<ArgumentException>(() => SdJwtJsonSerializer.ToGeneralJsonSerialization(null!));
    }

    [Fact]
    public void CalculateSdHashForJsonSerialization_WithValidInput_ReturnsHash()
    {
        // Arrange
        var jsonSerialization = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwt);

        // Act
        var hash = SdJwtJsonSerializer.CalculateSdHashForJsonSerialization(jsonSerialization, "sha-256");

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void CalculateSdHashForJsonSerialization_WithNullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SdJwtJsonSerializer.CalculateSdHashForJsonSerialization(null!, "sha-256"));
    }

    [Fact]
    public void IsValidJsonSerialization_WithNullOrEmpty_ReturnsFalse()
    {
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization(null!));
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization(""));
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization("   "));
    }

    [Fact]
    public void IsValidJsonSerialization_WithMalformedJson_ReturnsFalse()
    {
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization("{malformed}"));
        Assert.False(SdJwtJsonSerializer.IsValidJsonSerialization("not json at all"));
    }

    [Fact]
    public void GeneralJsonSerialization_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var original = SdJwtJsonSerializer.ToGeneralJsonSerialization(TestSdJwt);

        // Act
        var json = JsonSerializer.Serialize(original, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<SdJwtGeneralJsonSerialization>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Payload, deserialized.Payload);
        Assert.Equal(original.Signatures.Length, deserialized.Signatures.Length);
    }

    [Fact]
    public void FromFlattenedJsonSerialization_WithEmptyDisclosures_WorksCorrectly()
    {
        // Arrange
        var jsonSerialization = new SdJwtJsonSerialization
        {
            Protected = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9",
            Payload = "eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9",
            Signature = "signature",
            Header = new SdJwtUnprotectedHeader { Disclosures = Array.Empty<string>() }
        };

        // Act
        var result = SdJwtJsonSerializer.FromFlattenedJsonSerialization(jsonSerialization);

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith("~", result); // Should end with separator even with no disclosures
    }
}
