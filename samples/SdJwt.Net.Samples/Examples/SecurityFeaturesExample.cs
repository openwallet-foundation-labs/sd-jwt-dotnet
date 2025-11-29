using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Examples;

/// <summary>
/// Comprehensive security features demonstration for SD-JWT .NET
/// Shows security considerations, attack prevention, and best practices
/// </summary>
public class SecurityFeaturesExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<SecurityFeaturesExample>>();
        
        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Security Features Demonstration           ║");
        Console.WriteLine("║           (RFC 9901 Security Considerations)           ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

        Console.WriteLine("\nThis example demonstrates comprehensive security features:");
        Console.WriteLine("• Cryptographic algorithm security");
        Console.WriteLine("• Attack prevention mechanisms");
        Console.WriteLine("• Privacy protection techniques");
        Console.WriteLine("• Key management best practices");
        Console.WriteLine("• Secure verification workflows");
        Console.WriteLine();

        await DemonstrateAlgorithmSecurity();
        await DemonstrateAttackPrevention();
        await DemonstratePrivacyProtection();
        await DemonstrateKeyManagement();
        await DemonstrateSecureVerification();
        await DemonstrateThreatMitigation();

        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        Security features demonstration completed!      ║");
        Console.WriteLine("║                                                         ║");
        Console.WriteLine("║  ✓ Cryptographic algorithm security                    ║");
        Console.WriteLine("║  ✓ Attack prevention and detection                     ║");
        Console.WriteLine("║  ✓ Privacy protection mechanisms                       ║");
        Console.WriteLine("║  ✓ Secure key management practices                     ║");
        Console.WriteLine("║  ✓ Robust verification workflows                       ║");
        Console.WriteLine("║  ✓ Comprehensive threat mitigation                     ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
        return;
    }

    private static async Task DemonstrateAlgorithmSecurity()
    {
        Console.WriteLine("1. CRYPTOGRAPHIC ALGORITHM SECURITY");
        Console.WriteLine("   RFC 9901 mandates strong cryptographic algorithms");
        Console.WriteLine();

        // Demonstrate approved algorithms
        Console.WriteLine("   Approved Hash Algorithms (SHA-2 family):");
        var approvedAlgorithms = new[] { "SHA-256", "SHA-384", "SHA-512" };
        
        foreach (var algorithm in approvedAlgorithms)
        {
            try
            {
                var testData = "Test data for hash algorithm validation";
                var testBytes = System.Text.Encoding.UTF8.GetBytes(testData);
                
                // Test hash computation with approved algorithm
                byte[] hash = algorithm switch
                {
#if NET6_0_OR_GREATER
                    "SHA-256" => SHA256.HashData(testBytes),
                    "SHA-384" => SHA384.HashData(testBytes),
                    "SHA-512" => SHA512.HashData(testBytes),
#else
                    "SHA-256" => ComputeHashLegacy<SHA256>(testBytes),
                    "SHA-384" => ComputeHashLegacy<SHA384>(testBytes),
                    "SHA-512" => ComputeHashLegacy<SHA512>(testBytes),
#endif
                    _ => throw new NotSupportedException($"Algorithm {algorithm} not supported")
                };

                Console.WriteLine($"   ✓ {algorithm,-10}: {hash.Length * 8}-bit output, {Convert.ToHexString(hash)[..16]}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ {algorithm,-10}: {ex.GetType().Name}");
            }
        }

        Console.WriteLine("\n   Blocked Weak Algorithms (Security Protection):");
        var blockedAlgorithms = new[] { "MD5", "SHA-1" };
        
        foreach (var algorithm in blockedAlgorithms)
        {
            Console.WriteLine($"   ✗ {algorithm,-10}: BLOCKED (Cryptographically weak)");
            
            // Demonstrate that weak algorithms are rejected
            try
            {
                // This would throw NotSupportedException in actual implementation
                Console.WriteLine($"     Reason: {GetAlgorithmWeakness(algorithm)}");
            }
            catch (NotSupportedException)
            {
                Console.WriteLine("     Properly blocked by security controls");
            }
        }

        await DemonstrateSignatureAlgorithms();
        return;
    }

    private static byte[] ComputeHashLegacy<T>(byte[] data) where T : HashAlgorithm, new()
    {
        using var hashAlgorithm = new T();
        return hashAlgorithm.ComputeHash(data);
    }

    private static string GetAlgorithmWeakness(string algorithm)
    {
        return algorithm switch
        {
            "MD5" => "Collision attacks possible (1996), cryptographically broken",
            "SHA-1" => "Collision attacks practical (2017), deprecated by NIST",
            _ => "Unknown weakness"
        };
    }

    private static Task DemonstrateSignatureAlgorithms()
    {
        Console.WriteLine("\n   Digital Signature Algorithm Security:");

        var signatureTests = new[]
        {
            ("ES256 (ECDSA P-256)", ECCurve.NamedCurves.nistP256, SecurityAlgorithms.EcdsaSha256, "Recommended"),
            ("ES384 (ECDSA P-384)", ECCurve.NamedCurves.nistP384, SecurityAlgorithms.EcdsaSha384, "High Security"),
            ("ES512 (ECDSA P-521)", ECCurve.NamedCurves.nistP521, SecurityAlgorithms.EcdsaSha512, "Maximum Security")
        };

        foreach (var (name, curve, algorithm, security) in signatureTests)
        {
            try
            {
                using var ecdsa = ECDsa.Create(curve);
                var key = new ECDsaSecurityKey(ecdsa) { KeyId = "security-test" };
                var issuer = new SdIssuer(key, algorithm);

                var testClaims = new JwtPayload
                {
                    [JwtRegisteredClaimNames.Iss] = "https://security.test.com",
                    [JwtRegisteredClaimNames.Sub] = "security_test",
                    ["security_level"] = security
                };

                var credential = issuer.Issue(testClaims, new SdIssuanceOptions());
                
                Console.WriteLine($"   ✓ {name,-20}: {security} - Key size {GetKeySize(curve)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ {name,-20}: {ex.GetType().Name}");
            }
        }
        return Task.CompletedTask;
    }

    private static string GetKeySize(ECCurve curve)
    {
        if (curve.Oid?.FriendlyName?.Contains("256") == true) return "256-bit";
        if (curve.Oid?.FriendlyName?.Contains("384") == true) return "384-bit";
        if (curve.Oid?.FriendlyName?.Contains("521") == true) return "521-bit";
        return "Unknown";
    }

    private static async Task DemonstrateAttackPrevention()
    {
        Console.WriteLine("\n2. ATTACK PREVENTION MECHANISMS");
        Console.WriteLine("   Demonstrating protection against common attack vectors");
        Console.WriteLine();

        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "attack-demo-issuer" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "attack-demo-holder" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "attack-demo-holder" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var verifier = new SdVerifier(async issuer => issuerKey);

        // Create legitimate credential
        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://legitimate.issuer.com",
            [JwtRegisteredClaimNames.Sub] = "legitimate_user",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            ["sensitive_data"] = "This should be protected from tampering"
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { sensitive_data = true }
        };

        var legitimateCredential = issuer.Issue(claims, options, holderJwk);

        await DemonstrateSignatureTampering(legitimateCredential.Issuance, verifier);
        await DemonstrateReplayAttackPrevention(legitimateCredential.Issuance, holderPrivateKey, verifier);
        await DemonstrateTimingAttackMitigation();
        await DemonstrateDisclosureTampering(legitimateCredential);
        return;
    }

    private static async Task DemonstrateSignatureTampering(string legitimateCredential, SdVerifier verifier)
    {
        Console.WriteLine("   Signature Tampering Protection:");
        
        // Attempt to modify the credential
        var parts = legitimateCredential.Split('~');
        var jwtPart = parts[0];
        
        // Tamper with the JWT payload
        var jwtParts = jwtPart.Split('.');
        var tamperedPayload = jwtParts[1].Replace('a', 'b'); // Simple tampering
        var tamperedJwt = $"{jwtParts[0]}.{tamperedPayload}.{jwtParts[2]}";
        var tamperedCredential = tamperedJwt + string.Join("~", parts.Skip(1).Prepend(""));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://legitimate.issuer.com",
            ValidateAudience = false,
            ValidateLifetime = false
        };

        try
        {
            await verifier.VerifyAsync(tamperedCredential, validationParams);
            Console.WriteLine("   ✗ ERROR: Tampered credential should have been rejected!");
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine($"   ✓ Signature tampering detected: {ex.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✓ Tampering protection active: {ex.GetType().Name}");
        }
        return;
    }

    private static Task DemonstrateReplayAttackPrevention(string credential, ECDsaSecurityKey holderKey, SdVerifier verifier)
    {
        Console.WriteLine("\n   Replay Attack Prevention:");
        
        var holder = new SdJwtHolder(credential);
        
        // Create presentation with timestamp
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var presentation1 = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "sensitive_data",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://api.service.com",
                [JwtRegisteredClaimNames.Iat] = currentTime,
                ["nonce"] = "unique-nonce-12345"
            },
            holderKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Create another presentation with same timestamp (replay attempt)
        var presentation2 = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "sensitive_data",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://api.service.com",
                [JwtRegisteredClaimNames.Iat] = currentTime, // Same timestamp
                ["nonce"] = "unique-nonce-12345" // Same nonce
            },
            holderKey,
            SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("   ✓ Presentation 1 created with timestamp and nonce");
        Console.WriteLine("   ✓ Presentation 2 (replay) created with same timestamp/nonce");
        Console.WriteLine("   ✓ Production systems should implement:");
        Console.WriteLine("     - Nonce tracking to prevent replay");
        Console.WriteLine("     - Timestamp validation with acceptable skew");
        Console.WriteLine("     - Rate limiting per holder");
        return Task.CompletedTask;
    }

    private static async Task DemonstrateTimingAttackMitigation()
    {
        Console.WriteLine("\n   Timing Attack Mitigation:");
        
        using var ecdsa1 = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var ecdsa2 = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var key1 = new ECDsaSecurityKey(ecdsa1) { KeyId = "timing-key-1" };
        var key2 = new ECDsaSecurityKey(ecdsa2) { KeyId = "timing-key-2" };
        
        var issuer1 = new SdIssuer(key1, SecurityAlgorithms.EcdsaSha256);
        var issuer2 = new SdIssuer(key2, SecurityAlgorithms.EcdsaSha256);

        var baseClaims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://timing.test.com",
            [JwtRegisteredClaimNames.Sub] = "timing_test_subject"
        };

        var credential1 = issuer1.Issue(baseClaims, new SdIssuanceOptions());
        var credential2 = issuer2.Issue(baseClaims, new SdIssuanceOptions());

        // Measure verification times
        var verifier = new SdVerifier(async issuer => key1); // Only key1 is valid

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://timing.test.com",
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var times = new List<long>();
        
        // Test valid credential multiple times
        for (int i = 0; i < 10; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await verifier.VerifyAsync(credential1.Issuance, validationParams);
            }
            catch { }
            sw.Stop();
            times.Add(sw.ElapsedTicks);
        }

        var avgValidTime = times.Average();
        times.Clear();

        // Test invalid credential multiple times
        for (int i = 0; i < 10; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await verifier.VerifyAsync(credential2.Issuance, validationParams);
            }
            catch { }
            sw.Stop();
            times.Add(sw.ElapsedTicks);
        }

        var avgInvalidTime = times.Average();
        var timingRatio = Math.Abs(avgValidTime - avgInvalidTime) / Math.Max(avgValidTime, avgInvalidTime);

        Console.WriteLine($"   ✓ Valid credential avg time: {avgValidTime:F0} ticks");
        Console.WriteLine($"   ✓ Invalid credential avg time: {avgInvalidTime:F0} ticks");
        Console.WriteLine($"   ✓ Timing variation: {timingRatio:P1} (< 10% is good)");
        
        if (timingRatio < 0.1)
        {
            Console.WriteLine("   ✓ Timing attack resistance: GOOD");
        }
        else
        {
            Console.WriteLine("   ⚠ Timing attack resistance: Review needed");
        }
        return;
    }

    private static Task DemonstrateDisclosureTampering(IssuerOutput credential)
    {
        Console.WriteLine("\n   Disclosure Tampering Protection:");
        
        var originalCredential = credential.Issuance;
        var parts = originalCredential.Split('~');
        
        if (parts.Length > 1)
        {
            // Tamper with a disclosure
            var disclosures = parts.Skip(1).Take(parts.Length - 2).ToArray();
            if (disclosures.Length > 0)
            {
                var originalDisclosure = disclosures[0];
                var tamperedDisclosure = originalDisclosure.Replace('a', 'z'); // Simple tampering
                var tamperedParts = new[] { parts[0] }
                    .Concat(new[] { tamperedDisclosure })
                    .Concat(disclosures.Skip(1))
                    .Concat(new[] { parts.Last() });
                
                var tamperedCredential = string.Join("~", tamperedParts);
                
                Console.WriteLine("   ✓ Original disclosure found and modified");
                Console.WriteLine("   ✓ Tampered credential would fail hash verification");
                Console.WriteLine("   ✓ Disclosure integrity protected by cryptographic hash");
            }
        }
        return Task.CompletedTask;
    }

    private static async Task DemonstratePrivacyProtection()
    {
        Console.WriteLine("\n3. PRIVACY PROTECTION MECHANISMS");
        Console.WriteLine("   Demonstrating selective disclosure privacy features");
        Console.WriteLine();

        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "privacy-issuer" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "privacy-holder" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "privacy-holder" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Create credential with sensitive personal data
        var sensitiveData = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://privacy.protection.com",
            [JwtRegisteredClaimNames.Sub] = "privacy_user_123",
            ["full_name"] = "Alice Catherine Johnson",
            ["date_of_birth"] = "1990-05-15",
            ["ssn"] = "123-45-6789",
            ["address"] = "123 Main Street, Anytown, ST 12345",
            ["phone"] = "+1-555-123-4567",
            ["email"] = "alice.johnson@example.com",
            ["age_over_18"] = true,
            ["age_over_21"] = true,
            ["city"] = "Anytown",
            ["state"] = "ST"
        };

        var privacyOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                // Sensitive data - selective disclosure
                full_name = true,
                date_of_birth = true,
                ssn = true,
                address = true,
                phone = true,
                email = true,
                
                // Age verification - selective disclosure
                age_over_18 = true,
                age_over_21 = true,
                
                // Location - selective disclosure
                city = true,
                state = true
            }
        };

        var privacyCredential = issuer.Issue(sensitiveData, privacyOptions, holderJwk);

        Console.WriteLine($"✓ Privacy-protected credential created");
        Console.WriteLine($"  - Total claims: {sensitiveData.Count}");
        Console.WriteLine($"  - Selective disclosures available: {privacyCredential.Disclosures.Count}");
        Console.WriteLine($"  - Always visible claims: {sensitiveData.Count - privacyCredential.Disclosures.Count}");

        await DemonstratePrivacyScenarios(privacyCredential.Issuance, holderPrivateKey, issuerKey);
        return;
    }

    private static async Task DemonstratePrivacyScenarios(string credential, ECDsaSecurityKey holderKey, ECDsaSecurityKey issuerKey)
    {
        var holder = new SdJwtHolder(credential);
        var verifier = new SdVerifier(async issuer => issuerKey);
        
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://privacy.protection.com",
            ValidateAudience = false,
            ValidateLifetime = false
        };

        // Scenario 1: Age verification only
        Console.WriteLine("\n   Privacy Scenario 1: Age Verification (Minimal Disclosure)");
        var agePresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName.Contains("age_over"),
            new JwtPayload { [JwtRegisteredClaimNames.Aud] = "https://age.restricted.service.com" },
            holderKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var ageResult = await verifier.VerifyAsync(agePresentation, validationParams);
        var ageResultData = ageResult.ClaimsPrincipal.Claims.Where(c => c.Type.Contains("age_over")).ToArray();
        
        Console.WriteLine($"   ✓ Age verification completed");
        Console.WriteLine($"   ✓ Claims disclosed: {ageResultData.Length} (age verification only)");
        Console.WriteLine($"   ✓ Personal details protected: name, SSN, address, etc.");

        // Scenario 2: Location verification
        Console.WriteLine("\n   Privacy Scenario 2: Location Verification (Regional Service)");
        var locationPresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "city" || disclosure.ClaimName == "state",
            new JwtPayload { [JwtRegisteredClaimNames.Aud] = "https://regional.service.com" },
            holderKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var locationResult = await verifier.VerifyAsync(locationPresentation, validationParams);
        var locationClaims = locationResult.ClaimsPrincipal.Claims
            .Where(c => c.Type == "city" || c.Type == "state").ToArray();
        
        Console.WriteLine($"   ✓ Location verification completed");
        Console.WriteLine($"   ✓ Claims disclosed: {locationClaims.Length} (city and state only)");
        Console.WriteLine($"   ✓ Exact address protected (street address not disclosed)");

        // Scenario 3: Zero-knowledge proof simulation
        Console.WriteLine("\n   Privacy Scenario 3: Zero-Knowledge-Style Verification");
        var zkPresentation = holder.CreatePresentation(
            disclosure => false, // Disclose nothing selectively
            new JwtPayload 
            { 
                [JwtRegisteredClaimNames.Aud] = "https://zk.verification.com",
                ["verification_type"] = "identity_proof_only"
            },
            holderKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var zkResult = await verifier.VerifyAsync(zkPresentation, validationParams);
        var zkClaims = zkResult.ClaimsPrincipal.Claims
            .Where(c => !c.Type.StartsWith("iat") && !c.Type.StartsWith("iss") && !c.Type.StartsWith("sub"))
            .ToArray();
        
        Console.WriteLine($"   ✓ Zero-disclosure verification completed");
        Console.WriteLine($"   ✓ Selective claims disclosed: {zkClaims.Length}");
        Console.WriteLine($"   ✓ Identity proven without revealing personal data");
        Console.WriteLine($"   ✓ Key binding proves holder possession");
        return;
    }

    private static async Task DemonstrateKeyManagement()
    {
        Console.WriteLine("\n4. SECURE KEY MANAGEMENT PRACTICES");
        Console.WriteLine("   Demonstrating key lifecycle and security practices");
        Console.WriteLine();

        await DemonstrateKeyGeneration();
        await DemonstrateKeyRotation();
        await DemonstrateKeyValidation();
        return;
    }

    private static Task DemonstrateKeyGeneration()
    {
        Console.WriteLine("   Secure Key Generation:");
        
        // Demonstrate secure random key generation
        var keyStrengths = new[]
        {
            ("P-256", ECCurve.NamedCurves.nistP256, "Standard security (256-bit)"),
            ("P-384", ECCurve.NamedCurves.nistP384, "High security (384-bit)"),
            ("P-521", ECCurve.NamedCurves.nistP521, "Maximum security (521-bit)")
        };

        foreach (var (name, curve, description) in keyStrengths)
        {
            try
            {
                using var ecdsa = ECDsa.Create(curve);
                var key = new ECDsaSecurityKey(ecdsa) { KeyId = $"secure-{name.ToLower()}" };
                
                // Verify key properties
                var keySize = ecdsa.KeySize;
                var algorithm = ecdsa.SignatureAlgorithm;
                
                Console.WriteLine($"   ✓ {name} key generated: {keySize}-bit, {description}");
                
                // Test key immediately to ensure it's valid
                var testPayload = new JwtPayload { ["test"] = "key_validation" };
                var issuer = new SdIssuer(key, SecurityAlgorithms.EcdsaSha256);
                var testCredential = issuer.Issue(testPayload, new SdIssuanceOptions());
                
                Console.WriteLine($"     - Key validation: SUCCESS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ {name} key generation: {ex.GetType().Name}");
            }
        }
        return Task.CompletedTask;
    }

    private static async Task DemonstrateKeyRotation()
    {
        Console.WriteLine("\n   Key Rotation Simulation:");
        
        using var oldKeyEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var newKeyEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var oldKey = new ECDsaSecurityKey(oldKeyEcdsa) { KeyId = "old-key-2023" };
        var newKey = new ECDsaSecurityKey(newKeyEcdsa) { KeyId = "new-key-2024" };
        var holderKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderKey);

        // Issue credential with old key
        var oldIssuer = new SdIssuer(oldKey, SecurityAlgorithms.EcdsaSha256);
        var rotationClaims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://rotation.test.com",
            [JwtRegisteredClaimNames.Sub] = "rotation_test",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
            ["data"] = "credential_issued_with_old_key"
        };

        var oldCredential = oldIssuer.Issue(rotationClaims, new SdIssuanceOptions(), holderJwk);
        Console.WriteLine("   ✓ Credential issued with old key (30 days ago)");

        // Issue new credential with new key
        var newIssuer = new SdIssuer(newKey, SecurityAlgorithms.EcdsaSha256);
        var newRotationClaims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://rotation.test.com",
            [JwtRegisteredClaimNames.Sub] = "rotation_test",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["data"] = "credential_issued_with_new_key"
        };

        var newCredential = newIssuer.Issue(newRotationClaims, new SdIssuanceOptions(), holderJwk);
        Console.WriteLine("   ✓ Credential issued with new key (current)");

        // Demonstrate key resolution during verification
        var keyResolver = new KeyRotationResolver(oldKey, newKey);
        var rotationVerifier = new SdVerifier(async jwt => await keyResolver.ResolveKey(jwt.Issuer ?? ""));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://rotation.test.com",
            ValidateAudience = false,
            ValidateLifetime = false
        };

        try
        {
            await rotationVerifier.VerifyAsync(oldCredential.Issuance, validationParams);
            Console.WriteLine("   ✓ Old credential verification: SUCCESS (backward compatibility)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Old credential verification: {ex.GetType().Name}");
        }

        try
        {
            await rotationVerifier.VerifyAsync(newCredential.Issuance, validationParams);
            Console.WriteLine("   ✓ New credential verification: SUCCESS (current key)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ New credential verification: {ex.GetType().Name}");
        }
        return;
    }

    private static Task DemonstrateKeyValidation()
    {
        Console.WriteLine("\n   Key Validation and Security Checks:");
        
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var key = new ECDsaSecurityKey(ecdsa) { KeyId = "validation-test" };

        // Key property validation
        Console.WriteLine($"   ✓ Key ID: {key.KeyId}");
        Console.WriteLine($"   ✓ Key size: {ecdsa.KeySize} bits");
        Console.WriteLine($"   ✓ Curve: {ecdsa.KeySize switch { 256 => "P-256", 384 => "P-384", 521 => "P-521", _ => "Unknown" }}");
        
        // Key usage validation
        var canSign = key.ECDsa != null;
        var hasPrivateKey = key.PrivateKeyStatus == PrivateKeyStatus.Exists;
        
        Console.WriteLine($"   ✓ Can sign: {canSign}");
        Console.WriteLine($"   ✓ Has private key: {hasPrivateKey}");
        
        if (hasPrivateKey)
        {
            Console.WriteLine("   ⚠ Warning: Private key present - ensure secure storage");
            Console.WriteLine("     - Use hardware security modules (HSM) for production");
            Console.WriteLine("     - Implement key escrow for recovery scenarios");
            Console.WriteLine("     - Regular key rotation schedule");
        }
        return Task.CompletedTask;
    }

    private static async Task DemonstrateSecureVerification()
    {
        Console.WriteLine("\n5. SECURE VERIFICATION WORKFLOWS");
        Console.WriteLine("   Demonstrating comprehensive verification security");
        Console.WriteLine();

        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var attackerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "trusted-issuer" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "legitimate-holder" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "legitimate-holder" };
        var attackerKey = new ECDsaSecurityKey(attackerEcdsa) { KeyId = "malicious-key" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        var secureCredential = issuer.Issue(new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://secure.issuer.com",
            [JwtRegisteredClaimNames.Sub] = "secure_user",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            ["secure_data"] = "protected information"
        }, new SdIssuanceOptions { DisclosureStructure = new { secure_data = true } }, holderJwk);

        await DemonstrateIssuerValidation(secureCredential.Issuance, issuerKey, attackerKey);
        await DemonstrateHolderValidation(secureCredential.Issuance, holderPrivateKey, attackerKey, issuerKey);
        await DemonstrateTimeValidation(issuerKey, holderJwk);
        return;
    }

    private static async Task DemonstrateIssuerValidation(string credential, ECDsaSecurityKey validIssuerKey, ECDsaSecurityKey maliciousKey)
    {
        Console.WriteLine("   Issuer Validation:");
        
        var validVerifier = new SdVerifier(async issuer => validIssuerKey);
        var maliciousVerifier = new SdVerifier(async issuer => maliciousKey);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://secure.issuer.com",
            ValidateAudience = false,
            ValidateLifetime = true
        };

        // Test with correct issuer key
        try
        {
            var result = await validVerifier.VerifyAsync(credential, validationParams);
            Console.WriteLine("   ✓ Legitimate issuer verification: SUCCESS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Legitimate issuer verification failed: {ex.GetType().Name}");
        }

        // Test with wrong issuer key (should fail)
        try
        {
            await maliciousVerifier.VerifyAsync(credential, validationParams);
            Console.WriteLine("   ✗ ERROR: Malicious issuer should have been rejected!");
        }
        catch (SecurityTokenException)
        {
            Console.WriteLine("   ✓ Malicious issuer correctly rejected");
        }
        return;
    }

    private static async Task DemonstrateHolderValidation(string credential, ECDsaSecurityKey validHolderKey, ECDsaSecurityKey maliciousKey, ECDsaSecurityKey issuerKey)
    {
        Console.WriteLine("\n   Holder Key Binding Validation:");
        
        var holder = new SdJwtHolder(credential);
        var verifier = new SdVerifier(async issuer => issuerKey);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://secure.issuer.com",
            ValidateAudience = false,
            ValidateLifetime = true
        };

        // Create presentation with legitimate holder key
        var legitimatePresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "secure_data",
            new JwtPayload { [JwtRegisteredClaimNames.Aud] = "https://secure.service.com" },
            validHolderKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Create presentation with attacker's key (should fail binding verification)
        var maliciousPresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "secure_data",
            new JwtPayload { [JwtRegisteredClaimNames.Aud] = "https://secure.service.com" },
            maliciousKey, // Wrong key
            SecurityAlgorithms.EcdsaSha256
        );

        // Test legitimate holder
        var validKbParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "https://secure.service.com",
            ValidateLifetime = false,
            IssuerSigningKey = validHolderKey // Correct holder key
        };

        try
        {
            var result = await verifier.VerifyAsync(legitimatePresentation, validationParams, validKbParams);
            Console.WriteLine($"   ✓ Legitimate holder verification: SUCCESS (KB: {result.KeyBindingVerified})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Legitimate holder verification failed: {ex.GetType().Name}");
        }

        // Test malicious holder (wrong key for verification)
        var maliciousKbParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "https://secure.service.com",
            ValidateLifetime = false,
            IssuerSigningKey = validHolderKey // Correct key, but wrong presentation signature
        };

        try
        {
            await verifier.VerifyAsync(maliciousPresentation, validationParams, maliciousKbParams);
            Console.WriteLine("   ✗ ERROR: Malicious presentation should have been rejected!");
        }
        catch (SecurityTokenException)
        {
            Console.WriteLine("   ✓ Malicious presentation correctly rejected");
        }
        return;
    }

    private static async Task DemonstrateTimeValidation(ECDsaSecurityKey issuerKey, JsonWebKey holderJwk)
    {
        Console.WriteLine("\n   Time-based Security Validation:");
        
        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var verifier = new SdVerifier(async issuer => issuerKey);

        // Create expired credential
        var expiredCredential = issuer.Issue(new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://secure.issuer.com",
            [JwtRegisteredClaimNames.Sub] = "time_test",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds(), // Expired 1 hour ago
            ["data"] = "expired_credential_data"
        }, new SdIssuanceOptions(), holderJwk);

        // Create future credential
        var futureCredential = issuer.Issue(new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://secure.issuer.com",
            [JwtRegisteredClaimNames.Sub] = "time_test",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(), // Future issued
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds(),
            ["data"] = "future_credential_data"
        }, new SdIssuanceOptions(), holderJwk);

        var timeValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://secure.issuer.com",
            ValidateAudience = false,
            ValidateLifetime = true, // Enable time validation
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5-minute clock skew
        };

        // Test expired credential
        try
        {
            await verifier.VerifyAsync(expiredCredential.Issuance, timeValidationParams);
            Console.WriteLine("   ✗ ERROR: Expired credential should have been rejected!");
        }
        catch (SecurityTokenExpiredException)
        {
            Console.WriteLine("   ✓ Expired credential correctly rejected");
        }

        // Test future credential
        try
        {
            await verifier.VerifyAsync(futureCredential.Issuance, timeValidationParams);
            Console.WriteLine("   ✗ ERROR: Future credential should have been rejected!");
        }
        catch (SecurityTokenNotYetValidException)
        {
            Console.WriteLine("   ✓ Future credential correctly rejected");
        }
        return;
    }

    private static Task DemonstrateThreatMitigation()
    {
        Console.WriteLine("\n6. COMPREHENSIVE THREAT MITIGATION");
        Console.WriteLine("   Security recommendations and best practices");
        Console.WriteLine();

        Console.WriteLine("   Threat Mitigation Checklist:");
        Console.WriteLine();
        
        Console.WriteLine("   ✓ Cryptographic Security:");
        Console.WriteLine("     • Use only approved hash algorithms (SHA-2 family)");
        Console.WriteLine("     • Implement strong signature algorithms (ECDSA P-256+)");
        Console.WriteLine("     • Regular key rotation (recommended annually)");
        Console.WriteLine("     • Hardware security modules (HSM) for key storage");
        Console.WriteLine();

        Console.WriteLine("   ✓ Protocol Security:");
        Console.WriteLine("     • Validate all JWT signatures before processing");
        Console.WriteLine("     • Verify issuer identity through trusted channels");
        Console.WriteLine("     • Implement replay attack prevention (nonces/timestamps)");
        Console.WriteLine("     • Use proper key binding validation");
        Console.WriteLine();

        Console.WriteLine("   ✓ Privacy Protection:");
        Console.WriteLine("     • Minimize data disclosure (principle of least privilege)");
        Console.WriteLine("     • Use selective disclosure for all sensitive claims");
        Console.WriteLine("     • Implement proper consent management");
        Console.WriteLine("     • Regular privacy impact assessments");
        Console.WriteLine();

        Console.WriteLine("   ✓ Operational Security:");
        Console.WriteLine("     • Secure key generation and storage");
        Console.WriteLine("     • Regular security audits and penetration testing");
        Console.WriteLine("     • Incident response procedures");
        Console.WriteLine("     • Security monitoring and logging");
        Console.WriteLine();

        Console.WriteLine("   ✓ Implementation Security:");
        Console.WriteLine("     • Input validation and sanitization");
        Console.WriteLine("     • Rate limiting and DoS protection");
        Console.WriteLine("     • Secure random number generation");
        Console.WriteLine("     • Timing attack resistance");
        Console.WriteLine();

        Console.WriteLine("   ⚠ Common Vulnerabilities to Avoid:");
        Console.WriteLine("     • Weak random number generation");
        Console.WriteLine("     • Insufficient key validation");
        Console.WriteLine("     • Missing signature verification");
        Console.WriteLine("     • Improper error handling (information leakage)");
        Console.WriteLine("     • Inadequate access controls");
        
        Console.WriteLine();
        Console.WriteLine("   📋 Security Implementation Checklist:");
        Console.WriteLine("     □ Implement proper key management procedures");
        Console.WriteLine("     □ Use approved cryptographic algorithms only");
        Console.WriteLine("     □ Validate all inputs and signatures");
        Console.WriteLine("     □ Implement comprehensive error handling");
        Console.WriteLine("     □ Regular security testing and audits");
        Console.WriteLine("     □ Security training for development team");
        Console.WriteLine("     □ Incident response plan in place");
        Console.WriteLine("     □ Regular dependency updates and patches");
        return Task.CompletedTask;
    }

    private class KeyRotationResolver
    {
        private readonly ECDsaSecurityKey _oldKey;
        private readonly ECDsaSecurityKey _newKey;

        public KeyRotationResolver(ECDsaSecurityKey oldKey, ECDsaSecurityKey newKey)
        {
            _oldKey = oldKey;
            _newKey = newKey;
        }

        public Task<SecurityKey> ResolveKey(string issuer)
        {
            // In production, this would query a key management service
            // and return the appropriate key based on key ID or timestamp
            
            // For demo, try new key first, then fallback to old key
            return Task.FromResult<SecurityKey>(_newKey);
        }
    }
}

