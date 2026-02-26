using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Tests to achieve 100% coverage for SdJwtHolder
/// </summary>
public class SdJwtHolderFullCoverageTests : TestBase
{
    [Fact]
    public void Constructor_WithNullIssuance_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SdJwtHolder(null!));
    }

    [Fact]
    public void Constructor_WithEmptyIssuance_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SdJwtHolder(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceIssuance_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SdJwtHolder("   "));
    }

    [Fact]
    public void Constructor_WithInvalidJwtFormat_ThrowsArgumentException()
    {
        // Arrange
        var invalidJwt = "not.a.valid.jwt.format";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => new SdJwtHolder(invalidJwt));
    }

    [Fact]
    public void Constructor_WithMalformedJwt_ThrowsArgumentException()
    {
        // Arrange
        var malformedJwt = "eyJhbGciOiJIUzI1NiJ9.invalid-payload.sig";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => new SdJwtHolder(malformedJwt));
    }

    [Fact]
    public void CreatePresentation_WithNullDisclosureSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var issuerOutput = issuer.Issue(new() { { "sub", "user123" } }, new SdIssuanceOptions());
        var holder = new SdJwtHolder(issuerOutput.Issuance);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => holder.CreatePresentation(null!));
    }

    [Fact]
    public void CreatePresentation_WithSigningKeyButNoAlgorithm_ThrowsArgumentException()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var issuerOutput = issuer.Issue(new() { { "sub", "user123" } }, new SdIssuanceOptions());
        var holder = new SdJwtHolder(issuerOutput.Issuance);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            holder.CreatePresentation(_ => false, new(), HolderPrivateKey, null));
    }

    [Fact]
    public void CreatePresentation_WithSigningKeyButEmptyAlgorithm_ThrowsArgumentException()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var issuerOutput = issuer.Issue(new() { { "sub", "user123" } }, new SdIssuanceOptions());
        var holder = new SdJwtHolder(issuerOutput.Issuance);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            holder.CreatePresentation(_ => false, new(), HolderPrivateKey, "   "));
    }

    [Fact]
    public void Constructor_WithEmptyParts_ThrowsArgumentException()
    {
        // Arrange - only separators, no actual JWT
        var invalidIssuance = "~~~";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SdJwtHolder(invalidIssuance));
    }
}
