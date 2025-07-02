using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SdJwt.Net.Tests;

/// <summary>
/// This class contains an end-to-end test for a standard, non-VC SD-JWT flow.
/// It verifies the core functionality of the SdIssuer, SdHolder, and SdVerifier.
/// </summary>
public class EndToEndTests : TestBase
{
    [Fact]
    public async Task FullFlow_WithRecursionAndKeyBinding_VerifiesSuccessfully()
    {
        // 1. ISSUER: Create an SD-JWT with a mix of always-visible, selectively disclosable,
        // and nested selectively disclosable claims.
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload
        {
            // Always-visible claims that will remain in the final JWT payload.
            { "iss", TrustedIssuer },
            { "sub", "user123" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            
            // A complex claim with some parts being disclosable.
            { "address", new
                {
                    street_address = "123 Main St",
                    locality = "Anytown",
                    country = "US"
                }
            },
            // A simple claim that will be made selectively disclosable.
            { "email", "jane.doe@example.com" }
        };

        var options = new SdIssuanceOptions
        {
            // Define which claims can be selectively disclosed.
            DisclosureStructure = new
            {
                address = new
                {
                    locality = true, // only 'locality' and 'country' will be SD
                    country = true
                },
                email = true
            }
        };

        var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);

        var jwt = new JwtSecurityToken(issuerOutput.SdJwt);
        var payloadJson = JsonSerializer.Serialize(jwt.Payload, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("SD-JWT Payload:");
        Console.WriteLine(payloadJson);

        // 2. Log the disclosures (decoded)
        Console.WriteLine("Disclosures:");
        foreach (var disclosure in issuerOutput.Disclosures)
        {
            // If Disclosure has a property for the decoded value, print it; otherwise, print EncodedValue
            Console.WriteLine($"  - Encoded: {disclosure.EncodedValue}");
            Console.WriteLine($"    Name: {disclosure.ClaimName}, Value: {JsonSerializer.Serialize(disclosure.ClaimValue)}");
        }

        // 2. HOLDER: Create a presentation, choosing to disclose only the address's country and the email.
        var holder = new SdJwtHolder(issuerOutput.Issuance);
        var presentation = holder.CreatePresentation(
            // The selector function determines which disclosures to include.
            disclosure => disclosure.ClaimName == "country" || disclosure.ClaimName == "email",
            new JwtPayload { { "aud", "verifier" }, { "nonce", "xyz-nonce" } },
            HolderPrivateKey,
            HolderSigningAlgorithm);

        // 3. VERIFIER: Set up precise validation parameters to check the presentation.
        var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));

        // Parameters for the main SD-JWT. We are testing that the issuer is validated correctly.
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TrustedIssuer,
            ValidateAudience = false, // No top-level audience in this JWT
            ValidateLifetime = true
        };

        // Parameters for the Key Binding JWT.
        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false, // KB-JWTs don't have an issuer
            ValidateAudience = true,
            ValidAudience = "verifier",
            ValidateLifetime = false, // KB-JWTs use 'iat' and 'nonce', not 'exp'
            IssuerSigningKey = HolderPublicKey
        };

        // Act
        var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.KeyBindingVerified);

        var verifiedPrincipal = result.ClaimsPrincipal;

        // Check always-visible claims from the main JWT that were never disclosed.
        Assert.Equal(TrustedIssuer, verifiedPrincipal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value);
        Assert.Equal("user123", verifiedPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        Assert.NotNull(verifiedPrincipal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value);

        // Check the rehydrated 'address' claim, which is a complex object.
        var addressClaimJson = verifiedPrincipal.FindFirst("address")?.Value;
        Assert.NotNull(addressClaimJson);
        var addressNode = JsonNode.Parse(addressClaimJson!)!.AsObject();

        // 'street_address' was never disclosable, so it should always be present.
        Assert.True(addressNode.ContainsKey("street_address"));
        Assert.Equal("123 Main St", addressNode["street_address"]!.GetValue<string>());

        // 'locality' was disclosable but the Holder chose NOT to disclose it.
        Assert.False(addressNode.ContainsKey("locality"));

        // 'country' was disclosable AND the Holder chose to disclose it.
        Assert.True(addressNode.ContainsKey("country"));
        Assert.Equal("US", addressNode["country"]!.GetValue<string>());

        // Check the simple 'email' claim, which was disclosed.
        Assert.Equal("jane.doe@example.com", verifiedPrincipal.FindFirst("email")?.Value);
    }
}