using SdJwt.Net.Holder;
using SdJwt.Net.Internal;
using SdJwt.Net.Issuer;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace SdJwt.Net.Tests;

public class SdHolderTests : TestBase
{
    [Fact]
    public void Constructor_WithInvalidIssuance_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SdJwtHolder(""));
        Assert.Throws<ArgumentException>(() => new SdJwtHolder("~disclosure1"));
    }

    [Fact]
    public void CreatePresentation_WithNullSelector_ThrowsArgumentNullException()
    {
        var holder = new SdJwtHolder("eyJhbGciOiJIUzI1NiJ9.eyJfc2RfYWxnIjoic2hhLTI1NiJ9.sig~");
        Assert.Throws<ArgumentNullException>(() => holder.CreatePresentation(null!));
    }

    [Fact]
    public void CreatePresentation_WithKeyBinding_GeneratesCorrectSdHash()
    {
        // Arrange    
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm, "sha-512");
        var claims = new JwtPayload { { "sub", "user123" } };
        var options = new SdIssuanceOptions();
        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);
        var holder = new SdJwtHolder(issuerOutput.Issuance);

        // Act    
        var presentation = holder.CreatePresentation(
            d => false,
            new JwtPayload { { "aud", "verifier" } },
            HolderPrivateKey,
            HolderSigningAlgorithm
        );

        // Assert    
        var kbJwtString = presentation.Split('~')[^1];
        var kbJwt = new JwtSecurityToken(kbJwtString);
        var sdHashClaim = kbJwt.Payload["sd_hash"]; // Fix: Access the claim directly using the indexer  
        Assert.NotNull(sdHashClaim);

        var expectedHash = SdJwtUtils.CreateDigest("sha-512", $"{holder.SdJwt}~");
        Assert.Equal(expectedHash, sdHashClaim.ToString());
    }

    [Fact]
    public void CreatePresentation_WithKeyBindingButNoAlgorithm_ThrowsArgumentException()
    {
        // Arrange  
        var holder = new SdJwtHolder("eyJhbGciOiJIUzI1NiJ9.eyJfc2RfYWxnIjoic2hhLTI1NiJ9.sig~");

        // Act  
        var ex = Assert.Throws<ArgumentException>(() => holder.CreatePresentation(
            d => true,
            [],
            HolderPrivateKey,
            null // No algorithm provided  
        ));

        // Assert  
        Assert.Contains("must be provided when a signing key is present", ex.Message);
    }
}
