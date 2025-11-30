using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using SdJwt.Net.Verifier;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Core;

/// <summary>
/// Demonstrates JWS JSON Serialization features of SD-JWT
/// as defined in RFC 9901 Section 8
/// </summary>
public class JsonSerializationExample
{
    public static async Task RunExample()
    {
        Console.WriteLine("=== SD-JWT JWS JSON Serialization Example ===");
        
        // 1. Setup keys
        using var issuerKey = ECDsa.Create();
        using var holderKey = ECDsa.Create();
        
        var issuerECDsaKey = new ECDsaSecurityKey(issuerKey) { KeyId = "issuer-key" };
        var holderECDsaKey = new ECDsaSecurityKey(holderKey) { KeyId = "holder-key" };
        var holderJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(holderECDsaKey);
        
        // 2. Create SD-JWT
        var issuer = new SdIssuer(issuerECDsaKey, SecurityAlgorithms.EcdsaSha256);
        
        var payload = new JwtPayload
        {
            { JwtRegisteredClaimNames.Iss, "https://issuer.example.com" },
            { JwtRegisteredClaimNames.Sub, "user_42" },
            { JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() },
            { "given_name", "John" },
            { "family_name", "Doe" },
            { "email", "john.doe@example.com" },
            { "address", new
                {
                    street_address = "123 Main St",
                    locality = "Anytown",
                    region = "Anystate",
                    country = "US"
                }
            }
        };
        
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                given_name = true,
                family_name = true,
                email = true,
                address = true
            }
        };
        
        // 3. Issue as compact serialization
        Console.WriteLine("\n--- Compact Serialization ---");
        var compactResult = issuer.Issue(payload, options, holderJwk);
        Console.WriteLine($"Compact SD-JWT: {compactResult.Issuance[..100]}...");
        
        // 4. Convert to JWS Flattened JSON Serialization
        Console.WriteLine("\n--- Flattened JSON Serialization ---");
        var flattenedJson = SdJwtJsonSerializer.ToFlattenedJsonSerialization(compactResult.Issuance);
        var flattenedJsonString = JsonSerializer.Serialize(flattenedJson, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"Flattened JSON SD-JWT:\n{flattenedJsonString}");
        
        // 5. Convert to JWS General JSON Serialization
        Console.WriteLine("\n--- General JSON Serialization ---");
        var generalJson = SdJwtJsonSerializer.ToGeneralJsonSerialization(compactResult.Issuance);
        var generalJsonString = JsonSerializer.Serialize(generalJson, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"General JSON SD-JWT:\n{generalJsonString}");
        
        // 6. Issue directly as JSON serialization
        Console.WriteLine("\n--- Direct JSON Serialization Issuance ---");
        var directFlattenedJson = issuer.IssueAsJsonSerialization(payload, options, holderJwk);
        var directFlattenedJsonString = JsonSerializer.Serialize(directFlattenedJson, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"Direct Flattened JSON SD-JWT:\n{directFlattenedJsonString}");
        
        // 7. Verification
        Console.WriteLine("\n--- Verification ---");
        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(issuerECDsaKey));
        
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://issuer.example.com",
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = issuerECDsaKey
        };
        
        // Verify from JSON serialization
        var verificationResult = await verifier.VerifyJsonSerializationAsync(flattenedJsonString, validationParams);
        Console.WriteLine($"Verification successful: {verificationResult != null}");
        Console.WriteLine($"Number of claims: {verificationResult?.ClaimsPrincipal.Claims.Count()}");
        
        // 8. Round-trip verification
        Console.WriteLine("\n--- Round-trip Verification ---");
        var roundTripCompact = SdJwtJsonSerializer.FromFlattenedJsonSerialization(flattenedJson);
        var roundTripResult = await verifier.VerifyAsync(roundTripCompact, validationParams);
        Console.WriteLine($"Round-trip verification successful: {roundTripResult != null}");
        
        // 9. Format validation
        Console.WriteLine("\n--- Format Validation ---");
        Console.WriteLine($"Valid JSON serialization: {SdJwtJsonSerializer.IsValidJsonSerialization(flattenedJsonString)}");
        Console.WriteLine($"Valid JSON serialization (general): {SdJwtJsonSerializer.IsValidJsonSerialization(generalJsonString)}");
        Console.WriteLine($"Invalid JSON: {SdJwtJsonSerializer.IsValidJsonSerialization("{\"invalid\": \"format\"}")}");
        
        Console.WriteLine("\n=== Example Complete ===");
        return;
    }
}
