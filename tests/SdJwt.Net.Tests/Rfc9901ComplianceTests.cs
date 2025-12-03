using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using SdJwt.Net.Verifier;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Comprehensive test suite for RFC 9901 compliance
/// Tests various scenarios from the RFC 9901 specification
/// </summary>
public class Rfc9901ComplianceTests : TestBase
{
    [Fact]
    public async Task RFC9901_Example_BasicDisclosure_ShouldWork()
    {
        // Based on RFC 9901 Example 1: Simple claims
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload
        {
            { "iss", TrustedIssuer },
            { "sub", "user_42" },
            { "given_name", "John" },
            { "family_name", "Doe" },
            { "email", "johndoe@example.com" },
            { "phone_number", "+1-202-555-0101" },
            { "birthdate", "1940-01-01" },
            { "address", new {
                street_address = "123 Main St",
                locality = "Anytown", 
                region = "Anystate",
                country = "US"
            }}
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                given_name = true,
                family_name = true,
                email = true,
                phone_number = true,
                birthdate = true,
                address = new
                {
                    region = true
                }
            }
        };

        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);
        
        // Verify the SD-JWT has the expected structure
        var jwt = new JwtSecurityToken(issuerOutput.SdJwt);
        Assert.Equal(TrustedIssuer, jwt.Payload.Iss);
        Assert.Equal("user_42", jwt.Payload.Sub);
        Assert.Contains(jwt.Payload.Claims, c => c.Type == "_sd_alg");
        Assert.Contains(jwt.Payload.Claims, c => c.Type == "_sd");

        // Holder creates a presentation disclosing only given_name and address.region
        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "given_name" || disclosure.ClaimName == "region",
            new JwtPayload { { "aud", "https://verifier.example.org" }, { "nonce", "1234567890" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        // Verifier checks the presentation
        var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "https://verifier.example.org",
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);
        
        Assert.NotNull(result);
        Assert.True(result.KeyBindingVerified);
        
        // Check that only disclosed claims are present
        Assert.Equal("John", result.ClaimsPrincipal.FindFirst("given_name")?.Value);
        Assert.Null(result.ClaimsPrincipal.FindFirst("family_name"));
        Assert.Null(result.ClaimsPrincipal.FindFirst("email"));
        Assert.Null(result.ClaimsPrincipal.FindFirst("phone_number"));
        Assert.Null(result.ClaimsPrincipal.FindFirst("birthdate"));

        // Check address is rehydrated with only region disclosed
        var addressJson = result.ClaimsPrincipal.FindFirst("address")?.Value;
        Assert.NotNull(addressJson);
        var address = JsonSerializer.Deserialize<Dictionary<string, object>>(addressJson);
        Assert.Contains("street_address", address!.Keys);
        Assert.Contains("locality", address.Keys);
        Assert.Contains("region", address.Keys);
        Assert.Contains("country", address.Keys);
    }

    [Fact]
    public async Task RFC9901_ArrayDisclosures_ShouldWork()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload
        {
            { "iss", TrustedIssuer },
            { "sub", "user_42" },
            { "nationalities", new[] { "US", "DE", "SG" } }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                nationalities = new[] { false, true, false } // Only "DE" is selectively disclosable
            }
        };

        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == null, // Array disclosure
            new JwtPayload { { "aud", "verifier" }, { "nonce", "abc" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "verifier",
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);
        Assert.NotNull(result);
        Assert.True(result.KeyBindingVerified);
    }

    [Fact]
    public async Task RFC9901_NoKeyBinding_ShouldWork()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload
        {
            { "iss", TrustedIssuer },
            { "sub", "user_42" },
            { "given_name", "John" },
            { "family_name", "Doe" }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                family_name = true
            }
        };

        // Issue without holder key (no key binding)
        var issuerOutput = issuer.Issue(claims, options);

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "family_name");

        var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);
        
        Assert.NotNull(result);
        Assert.False(result.KeyBindingVerified); // No key binding JWT provided
        Assert.Equal("John", result.ClaimsPrincipal.FindFirst("given_name")?.Value);
        Assert.Equal("Doe", result.ClaimsPrincipal.FindFirst("family_name")?.Value);
    }

    [Fact]
    public void RFC9901_HashAlgorithmCompliance()
    {
        // Test that SHA-256 is properly supported (required by RFC 9901)
        var salt = SdJwtUtils.GenerateSalt();
        var disclosure = new Disclosure(salt, "claim", "value");
        var digest = SdJwtUtils.CreateDigest("sha-256", disclosure.EncodedValue);
        
        Assert.NotNull(digest);
        Assert.NotEmpty(digest);
        
        // Test that SHA-512 is also supported
        var digest512 = SdJwtUtils.CreateDigest("sha-512", disclosure.EncodedValue);
        Assert.NotNull(digest512);
        Assert.NotEmpty(digest512);
        Assert.NotEqual(digest, digest512); // Different algorithms should produce different hashes
    }

    [Fact]
    public void RFC9901_MediaTypes_AreCompliant()
    {
        // Verify that the media types match RFC 9901 Section 11
        Assert.Equal("application/sd-jwt", SdJwtConstants.SdJwtMediaType);
        Assert.Equal("application/sd-jwt+json", SdJwtConstants.SdJwtJsonMediaType);
        Assert.Equal("application/kb+jwt", SdJwtConstants.KeyBindingJwtMediaType);
        Assert.Equal("+sd-jwt", SdJwtConstants.SdJwtSuffix);
        Assert.Equal("sd+jwt", SdJwtConstants.SdJwtTypeName);
        Assert.Equal("kb+jwt", SdJwtConstants.KbJwtHeaderType);
    }

    [Fact]
    public async Task RFC9901_EmptyPresentation_ShouldWork()
    {
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload
        {
            { "iss", TrustedIssuer },
            { "sub", "user_42" },
            { "given_name", "John" },
            { "family_name", "Doe" }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                family_name = true
            }
        };

        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);

        var holder = new SdJwtHolder(issuerOutput.Issuance);
        // Create presentation with no disclosures
        var presentation = holder.CreatePresentation(
            disclosure => false, // Don't disclose anything
            new JwtPayload { { "aud", "verifier" }, { "nonce", "123" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "verifier",
            ValidateLifetime = false,
            IssuerSigningKey = HolderPublicKey
        };

        var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);
        
        Assert.NotNull(result);
        Assert.True(result.KeyBindingVerified);
        Assert.Equal("John", result.ClaimsPrincipal.FindFirst("given_name")?.Value);
        Assert.Null(result.ClaimsPrincipal.FindFirst("family_name")); // Not disclosed
    }
}
