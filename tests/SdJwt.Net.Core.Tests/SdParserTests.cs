using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Utils;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Core.Tests;

public class SdParserTests
{
    [Fact]
    public void ParsePresentation_WithKeyBinding_ParsesCorrectly()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string disclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";
        const string kbJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJub25jZSI6IjEyMyJ9.sig";
        var presentation = $"{sdJwt}~{disclosure}~{kbJwt}";

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.Equal(sdJwt, parsed.RawSdJwt);
        Assert.Single(parsed.Disclosures);
        Assert.Equal(kbJwt, parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void ParsePresentation_WithoutKeyBinding_ParsesCorrectly()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string disclosure1 = "WyJzYWx0MSIsIm5hbWUxIiwidmFsdWUxIl0";
        const string disclosure2 = "WyJzYWx0MiIsIm5hbWUyIiwidmFsdWUyIl0";
        var presentation = $"{sdJwt}~{disclosure1}~{disclosure2}";

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.Equal(sdJwt, parsed.RawSdJwt);
        Assert.Equal(2, parsed.Disclosures.Count);
        Assert.Null(parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void ParsePresentation_WithExtraSeparators_IsRobust()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string disclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";
        var presentation = $"{sdJwt}~~{disclosure}~"; // Extra and trailing separators

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.Single(parsed.Disclosures);
        Assert.Null(parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void ParseIssuance_WithInvalidDisclosure_ThrowsJsonException()
    {
        // Arrange
        var invalidDisclosure = Base64UrlEncoder.Encode("this is not a json array");
        var issuance = $"eyJhbGciOiJIUzI1NiJ9.e30.sig~{invalidDisclosure}";

        // Act & Assert
        Assert.Throws<JsonException>(() => SdJwtParser.ParseIssuance(issuance));
    }
}