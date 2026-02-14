using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests.Serialization;

/// <summary>
/// Comprehensive tests for SdJwtJsonSerializer to reach 95% coverage
/// </summary>
public class SdJwtJsonSerializerComprehensiveTests
{
    private const string TestSdJwt = "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9.eyJfc2QiOlsiQ3JRZTdTNWtxQkFIdC1uTVlYZ2M2YmR0MlNINWFUWTFzVV9NLVBna2pQSSJdLCJpc3MiOiJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsInN1YiI6InVzZXJfNDIiLCJfc2RfYWxnIjoic2hhLTI1NiJ9.signature~WyJzYWx0IiwibmFtZSIsInZhbHVlIl0~";

    [Fact]
    public void ToFlattenedJsonSerialization_WithMultipleDisclosures_HandlesCorrectly()
    {
        // Arrange
        var sdJwt = "eyJhbGciOiJFUzI1NiJ9.eyJfc2QiOlsiYSIsImIiXX0.sig~WyJzMSIsImMxIiwidjEiXQ~WyJzMiIsImMyIiwidjIiXQ~";

        // Act
        var result = SdJwtJsonSerializer.ToFlattenedJsonSerialization(sdJwt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Header.Disclosures.Length);
    }

    [Fact]
    public void ToFlattenedJsonSerialization_WithTrailingSeparator_HandlesCorrectly()
    {
        // Act
        var result = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwt);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Header.KbJwt);
    }

    [Fact]
    public void FromFlattenedJsonSerialization_WithNoDisclosures_CreatesCorrectString()
    {
        // Arrange
        var jsonSerialization = new SdJwtJsonSerialization
        {
            Protected = "eyJhbGciOiJFUzI1NiJ9",
            Payload = "eyJfc2QiOlsiYSJdfQ",
            Signature = "sig",
            Header = new SdJwtUnprotectedHeader { Disclosures = Array.Empty<string>() }
        };

        // Act
        var result = SdJwtJsonSerializer.FromFlattenedJsonSerialization(jsonSerialization);

        // Assert
        Assert.EndsWith("~", result);
    }

    [Fact]
    public void ToGeneralJsonSerialization_WithEmptyAdditionalSignatures_OnlyIncludesMainSignature()
    {
        // Act
        var result = SdJwtJsonSerializer.ToGeneralJsonSerialization(TestSdJwt, Array.Empty<SdJwtSignature>());

        // Assert
        Assert.Single(result.Signatures);
    }

    [Fact]
    public void FromGeneralJsonSerialization_WithMultipleSignatures_UsesFirstSignature()
    {
        // Arrange
        var generalSerialization = new SdJwtGeneralJsonSerialization
        {
            Payload = "eyJfc2QiOlsiYSJdfQ",
            Signatures = new[]
            {
                new SdJwtSignature
                {
                    Protected = "eyJhbGciOiJFUzI1NiJ9",
                    Signature = "sig1",
                    Header = new SdJwtUnprotectedHeader { Disclosures = new[] { "WyJzMSIsImMxIiwidjEiXQ" } }
                },
                new SdJwtSignature
                {
                    Protected = "eyJhbGciOiJFUzI1NiJ9",
                    Signature = "sig2",
                    Header = new SdJwtUnprotectedHeader()
                }
            }
        };

        // Act
        var result = SdJwtJsonSerializer.FromGeneralJsonSerialization(generalSerialization);

        // Assert
        Assert.Contains("sig1", result);
    }

    [Fact]
    public void CalculateSdHashForJsonSerialization_WithDifferentInputs_ProducesDifferentHashes()
    {
        // Arrange
        var json1 = SdJwtJsonSerializer.ToFlattenedJsonSerialization(TestSdJwt);
        var json2 = SdJwtJsonSerializer.ToFlattenedJsonSerialization("eyJhbGciOiJFUzI1NiJ9.eyJfc2QiOlsieCJdfQ.sig~");

        // Act
        var hash1 = SdJwtJsonSerializer.CalculateSdHashForJsonSerialization(json1, "sha-256");
        var hash2 = SdJwtJsonSerializer.CalculateSdHashForJsonSerialization(json2, "sha-256");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void IsValidJsonSerialization_WithValidFlattenedStructure_ReturnsTrue()
    {
        // Arrange
        var json = "{\"protected\":\"abc\",\"payload\":\"def\",\"signature\":\"ghi\",\"header\":{\"sd_disclosures\":[]}}";

        // Act
        var result = SdJwtJsonSerializer.IsValidJsonSerialization(json);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidJsonSerialization_WithValidGeneralStructure_ReturnsTrue()
    {
        // Arrange
        var json = "{\"payload\":\"abc\",\"signatures\":[{\"protected\":\"def\",\"signature\":\"ghi\",\"header\":{}}]}";

        // Act
        var result = SdJwtJsonSerializer.IsValidJsonSerialization(json);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidJsonSerialization_WithMissingRequiredFields_ReturnsFalse()
    {
        // Arrange
        var json = "{\"protected\":\"abc\"}"; // Missing payload and signature

        // Act
        var result = SdJwtJsonSerializer.IsValidJsonSerialization(json);

        // Assert
        Assert.False(result);
    }

}
