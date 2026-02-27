using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Xunit;

namespace SdJwt.Net.Tests;

public class SdParserFinalCoverageTests
{
    [Fact]
    public void ParseIssuance_WithEmptyParts_ThrowsFormatException()
    {
        // Arrange
        var sdJwt = "eyJhbGciOiJFUzI1NiJ9.e30.signature";
        var validDisclosure = "WyJzYWx0IiwgImNsYWltIiwgInZhbHVlIl0"; // ["salt", "claim", "value"]
        var issuance = $"{sdJwt}~~{validDisclosure}~"; // Double tilde and trailing tilde

        // Act & Assert
        Assert.Throws<FormatException>(() => SdJwtParser.ParseIssuance(issuance));
    }

    [Fact]
    public void ParsePresentation_WithOnlySeparators_ThrowsArgumentException()
    {
        // Arrange
        var presentation = "~~~";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParsePresentation(presentation));
    }

    [Fact]
    public void ParsePresentation_WithMalformedKbJwtHeader_TreatsAsDisclosure()
    {
        // Arrange
        var sdJwt = "eyJhbGciOiJFUzI1NiJ9.e30.signature";
        var malformedJwt = "not.a.jwt"; // Not 3 parts
        var presentation = $"{sdJwt}~{malformedJwt}";

        // Act
        // Should treat "not.a.jwt" as a disclosure.
        // Since it contains dots, it's not valid base64url, so Disclosure.Parse throws FormatException.
        Assert.Throws<FormatException>(() => SdJwtParser.ParsePresentation(presentation));
    }

    [Fact]
    public void ParsePresentation_WithKbJwtTypJWT_RecognizedAsKbJwt()
    {
        // Arrange
        var sdJwt = "eyJhbGciOiJFUzI1NiJ9.e30.signature";

        // Create a JWT with typ=JWT
        var header = "{\"typ\":\"kb+jwt\",\"alg\":\"none\"}";
        var payload = "{}";
        var signature = "";
        var kbJwt = $"{Base64UrlEncoder.Encode(header)}.{Base64UrlEncoder.Encode(payload)}.{signature}";

        var presentation = $"{sdJwt}~{kbJwt}";

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.NotNull(parsed.RawKeyBindingJwt);
        Assert.Equal(kbJwt, parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void ParsePresentation_WithKbJwtMissingTyp_TreatsAsDisclosure()
    {
        // Arrange
        var sdJwt = "eyJhbGciOiJFUzI1NiJ9.e30.signature";

        // Create a JWT without typ
        var header = "{\"alg\":\"none\"}";
        var payload = "{}";
        var signature = "";
        var fakeJwt = $"{Base64UrlEncoder.Encode(header)}.{Base64UrlEncoder.Encode(payload)}.{signature}";

        var presentation = $"{sdJwt}~{fakeJwt}";

        // Act
        // Should treat as disclosure.
        // Since it contains dots, it's not valid base64url, so Disclosure.Parse throws FormatException.
        Assert.Throws<FormatException>(() => SdJwtParser.ParsePresentation(presentation));
    }
}
