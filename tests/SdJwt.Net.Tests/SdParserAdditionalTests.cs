using SdJwt.Net.Utils;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Additional tests for SdJwtParser to improve coverage
/// </summary>
public class SdParserAdditionalTests
{
    [Fact]
    public void ParseIssuance_WithNullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParseIssuance(null!));
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParseIssuance(""));
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParseIssuance("   "));
    }

    [Fact]
    public void ParsePresentation_WithNullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParsePresentation(null!));
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParsePresentation(""));
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParsePresentation("   "));
    }

    [Fact]
    public void ParsePresentation_WithOnlySdJwtAndTrailingSeparator_ParsesCorrectly()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";

        // Act
        var parsed = SdJwtParser.ParsePresentation($"{sdJwt}~");

        // Assert
        Assert.Equal(sdJwt, parsed.RawSdJwt);
        Assert.Empty(parsed.Disclosures);
        Assert.Null(parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void ParsePresentation_WithInvalidJwtFormat_ThrowsException()
    {
        // Arrange
        const string invalidJwt = "not.a.valid.jwt.format";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParsePresentation(invalidJwt));
    }

    [Fact]
    public void ParseIssuance_WithNoDisclosures_ParsesCorrectly()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";

        // Act
        var parsed = SdJwtParser.ParseIssuance($"{sdJwt}~");

        // Assert
        Assert.Equal(sdJwt, parsed.RawSdJwt);
        Assert.Empty(parsed.Disclosures);
    }

    [Fact]
    public void ParseJsonFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        const string nonExistentPath = "C:\\non\\existent\\file.json";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => SdJwtParser.ParseJsonFile<object>(nonExistentPath));
    }

    [Fact]
    public void ParseJson_WithValidJson_ReturnsDeserializedObject()
    {
        // Arrange
        const string json = "{\"name\":\"test\",\"value\":123}";

        // Act
        var result = SdJwtParser.ParseJson<Dictionary<string, JsonElement>>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result["name"].GetString());
        Assert.Equal(123, result["value"].GetInt32());
    }

    [Fact]
    public void ParseJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string invalidJson = "{invalid json}";

        // Act & Assert
        Assert.Throws<JsonException>(() => SdJwtParser.ParseJson<object>(invalidJson));
    }

}
