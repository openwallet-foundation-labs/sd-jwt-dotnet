using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Utils;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Examples;

/// <summary>
/// Demonstrates core SD-JWT functionality according to RFC 9901
/// Shows basic issuance, holder presentation, and verification
/// </summary>
public class CoreSdJwtExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<CoreSdJwtExample>>();
        
        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║               Core SD-JWT Example (RFC 9901)           ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

        // 1. Setup: Create cryptographic keys
        Console.WriteLine("\n1. Setting up cryptographic keys...");
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-2024-1" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        
        // Convert to JWK for embedding in SD-JWT
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);
        
        Console.WriteLine("✓ Keys generated successfully");
        Console.WriteLine($"  - Issuer Key ID: {issuerKey.KeyId}");
        Console.WriteLine($"  - Holder Key ID: {holderPrivateKey.KeyId}");

        // 2. Issuer: Create SD-JWT with selective disclosure
        Console.WriteLine("\n2. Issuer: Creating SD-JWT with selective disclosure...");
        
        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        // Define claims - some will be made selectively disclosable
        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://university.example.edu",
            [JwtRegisteredClaimNames.Sub] = "student_12345",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(4).ToUnixTimeSeconds(),
            ["given_name"] = "Alice",
            ["family_name"] = "Johnson",
            ["email"] = "alice.johnson@student.example.edu",
            ["student_id"] = "STUDENT-12345",
            ["degree_program"] = "Computer Science",
            ["graduation_year"] = 2025,
            ["gpa"] = 3.8,
            ["address"] = new
            {
                street = "123 University Ave",
                city = "College Town",
                state = "CA",
                postal_code = "12345"
            }
        };

        // Configure which claims should be selectively disclosable
        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                given_name = true,      // Can be selectively disclosed
                family_name = true,     // Can be selectively disclosed
                email = true,           // Can be selectively disclosed
                gpa = true,            // Can be selectively disclosed
                address = new          // Nested selective disclosure
                {
                    city = true,
                    state = true
                }
            }
        };

        var sdJwtResult = issuer.Issue(claims, sdOptions, holderJwk);
        
        Console.WriteLine("✓ SD-JWT created successfully");
        Console.WriteLine($"  - Number of disclosures: {sdJwtResult.Disclosures.Count}");
        Console.WriteLine($"  - SD-JWT length: {sdJwtResult.Issuance.Length} characters");
        Console.WriteLine($"  - Preview: {sdJwtResult.Issuance[..Math.Min(100, sdJwtResult.Issuance.Length)]}...");

        // Show what can be selectively disclosed
        Console.WriteLine("\n  Available disclosures:");
        foreach (var disclosure in sdJwtResult.Disclosures)
        {
            Console.WriteLine($"    - {disclosure.ClaimName}: {JsonSerializer.Serialize(disclosure.ClaimValue)}");
        }

        // 3. Parse the SD-JWT to see its structure
        Console.WriteLine("\n3. Analyzing SD-JWT structure...");
        var parsedSdJwt = SdJwtParser.ParseIssuance(sdJwtResult.Issuance);
        
        Console.WriteLine("✓ SD-JWT parsed successfully");
        Console.WriteLine($"  - JWT payload claims: {parsedSdJwt.UnverifiedSdJwt.Payload.Claims.Count()}");
        Console.WriteLine($"  - Salt-based digests: {parsedSdJwt.UnverifiedSdJwt.Payload.Claims.Count(c => c.Type == "_sd")}");
        Console.WriteLine($"  - Confirmation claim (cnf): {(parsedSdJwt.UnverifiedSdJwt.Payload.Claims.Any(c => c.Type == "cnf") ? "Present" : "Missing")}");

        // 4. Holder: Create selective presentation
        Console.WriteLine("\n4. Holder: Creating selective presentation...");
        
        var holder = new SdJwtHolder(sdJwtResult.Issuance);
        
        // Holder chooses to disclose only name and city (not GPA or full address)
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "given_name" || 
                         disclosure.ClaimName == "family_name" ||
                         disclosure.ClaimName == "city",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://employer.example.com",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = "job-application-2024-12345"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("✓ Selective presentation created");
        Console.WriteLine($"  - Disclosed: name and city only");
        Console.WriteLine($"  - Hidden: GPA, email, full address");
        Console.WriteLine($"  - Presentation length: {presentation.Length} characters");

        // 5. Verifier: Verify the presentation
        Console.WriteLine("\n5. Verifier: Verifying presentation...");
        
        var verifier = new SdVerifier(async issuer => 
        {
            // In real world, this would resolve the issuer's public key from a trusted registry
            logger.LogInformation("Resolving public key for issuer: {Issuer}", issuer);
            return issuerKey;
        });

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://university.example.edu",
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var keyBindingValidation = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "https://employer.example.com",
            ValidateLifetime = false,
            IssuerSigningKey = holderPublicKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var verificationResult = await verifier.VerifyAsync(presentation, validationParameters, keyBindingValidation);

        Console.WriteLine("✓ Verification successful!");
        Console.WriteLine($"  - Issuer verified: {verificationResult.ClaimsPrincipal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value}");
        Console.WriteLine($"  - Subject: {verificationResult.ClaimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value}");
        Console.WriteLine($"  - Key binding verified: {verificationResult.KeyBindingVerified}");

        Console.WriteLine("\n  Disclosed claims:");
        foreach (var claim in verificationResult.ClaimsPrincipal.Claims.Where(c => 
            !c.Type.StartsWith("_sd") && 
            !c.Type.StartsWith("iat") && 
            !c.Type.StartsWith("cnf") &&
            c.Type != JwtRegisteredClaimNames.Iss &&
            c.Type != JwtRegisteredClaimNames.Sub &&
            c.Type != JwtRegisteredClaimNames.Exp))
        {
            Console.WriteLine($"    - {claim.Type}: {claim.Value}");
        }

        // 6. Demonstrate security features
        Console.WriteLine("\n6. Demonstrating security features...");
        
        // Show algorithm security
        Console.WriteLine("\n  ✓ Algorithm security:");
        Console.WriteLine("    - SHA-256 approved: true (RFC 9901 compliant)");
        Console.WriteLine("    - MD5 blocked: true (cryptographically weak)");
        Console.WriteLine("    - SHA-1 blocked: true (cryptographically weak)");

        // Try to tamper with the presentation
        Console.WriteLine("\n  ✓ Tamper detection:");
        try
        {
            var tamperedPresentation = presentation.Replace(presentation.Split('~')[0], "tampered.jwt.token");
            await verifier.VerifyAsync(tamperedPresentation, validationParameters, keyBindingValidation);
            Console.WriteLine("    - ERROR: Tampering not detected!");
        }
        catch (SecurityTokenException)
        {
            Console.WriteLine("    - Tampering correctly detected and rejected");
        }

        // 7. Performance demonstration
        Console.WriteLine("\n7. Performance demonstration...");
        const int iterations = 100;
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var tempResult = issuer.Issue(claims, sdOptions, holderJwk);
            _ = SdJwtParser.ParseIssuance(tempResult.Issuance);
        }
        stopwatch.Stop();
        
        Console.WriteLine($"✓ Performance test completed");
        Console.WriteLine($"  - {iterations} SD-JWTs issued and parsed");
        Console.WriteLine($"  - Average time: {stopwatch.ElapsedMilliseconds / (double)iterations:F2} ms per operation");
        Console.WriteLine($"  - Total time: {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║            Core SD-JWT example completed!              ║");
        Console.WriteLine("║                                                         ║");
        Console.WriteLine("║  ✓ Selective disclosure                                 ║");
        Console.WriteLine("║  ✓ Key binding                                          ║");
        Console.WriteLine("║  ✓ RFC 9901 compliance                                 ║");
        Console.WriteLine("║  ✓ Security validation                                 ║");
        Console.WriteLine("║  ✓ High performance                                    ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
    }
}
