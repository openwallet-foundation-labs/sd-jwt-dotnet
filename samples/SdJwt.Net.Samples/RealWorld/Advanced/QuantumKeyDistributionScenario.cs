using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace SdJwt.Net.Samples.RealWorld.Advanced;

/// <summary>
/// Demonstrates a runnable post-quantum readiness pattern:
/// signed algorithm policy, SD-JWT verification, and QKD-style encrypted transport.
/// </summary>
public static class QuantumKeyDistributionScenario
{
    public static async Task RunScenario()
    {
        Console.WriteLine("\n=======================================================");
        Console.WriteLine("    QUANTUM READINESS: AGILITY + QKD STYLE TRANSPORT    ");
        Console.WriteLine("=======================================================\n");
        Console.WriteLine("Implementation boundaries:");
        Console.WriteLine("- Native PQC path uses .NET 10 MLDsa/MLKem when provider support is available.");
        Console.WriteLine("- SD-JWT issuance in this sample remains JOSE ES256 for interoperability.");
        Console.WriteLine("- QKD hardware integration is represented as application-layer transport pattern.\n");

        using var trustAuthorityEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var trustAuthorityKey = new ECDsaSecurityKey(trustAuthorityEcdsa) { KeyId = "sovereign-trust-authority-k1" };
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "sovereign-issuer-k1" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-qkd-wallet-k1" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-qkd-wallet-k1" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        var federationPolicyJwt = CreateSignedAlgorithmPolicy(trustAuthorityKey);
        var validatedPolicy = ValidateAlgorithmPolicy(federationPolicyJwt, trustAuthorityKey);

        Console.WriteLine("Trust policy validated from signed metadata:");
        Console.WriteLine($"- Current signing suite: {validatedPolicy.CurrentSigningSuite}");
        Console.WriteLine($"- PQC target suite: {validatedPolicy.PqcTargetSuite}");
        Console.WriteLine($"- PQC required by: {validatedPolicy.PqcRequiredFrom:yyyy-MM-dd}");

        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var credential = vcIssuer.Issue(
            "https://credentials.sovereign.example/clearance",
            new SdJwtVcPayload
            {
                Issuer = "https://issuer.sovereign.example",
                Subject = "did:example:officer-22",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["clearance_level"] = "secret",
                    ["mission_code"] = "ARCTIC-7",
                    ["issuer_crypto_profile"] = validatedPolicy.CurrentSigningSuite
                }
            },
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    mission_code = true
                }
            },
            holderJwk);

        var holder = new SdJwtHolder(credential.Issuance);
        var verifierAudience = "https://verifier.sovereign.example";
        var nonce = "qkd-demo-2026-001";

        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "mission_code",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = verifierAudience,
                [JwtRegisteredClaimNames.Nonce] = nonce,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256);

        var pqcResult = TryRunNativePqcDemo(presentation);
        var transportKey = pqcResult.TransportKey ?? QkdNode.GenerateSharedSymmetricKey();
        var encryptedEnvelope = EncryptPresentation(presentation, transportKey);
        var decryptedPresentation = DecryptPresentation(encryptedEnvelope, transportKey);

        var ciphertextChanged = !CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(presentation),
            encryptedEnvelope.Ciphertext);

        Console.WriteLine("\nPQC execution status:");
        Console.WriteLine($"- Native ML-DSA signing path: {pqcResult.MlDsaStatus}");
        Console.WriteLine($"- Native ML-KEM key establishment: {pqcResult.MlKemStatus}");

        Console.WriteLine("\nTransport protection:");
        Console.WriteLine($"- Shared key size: {transportKey.Length * 8} bits");
        Console.WriteLine($"- Ciphertext differs from plaintext: {ciphertextChanged}");
        Console.WriteLine($"- Envelope nonce/tag present: {encryptedEnvelope.Nonce.Length > 0 && encryptedEnvelope.Tag.Length > 0}");

        var verifier = new SdVerifier(jwt => Task.FromResult<SecurityKey>(issuerKey));
        var verification = await verifier.VerifyAsync(
            decryptedPresentation,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://issuer.sovereign.example" },
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            },
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = verifierAudience,
                ValidateLifetime = false,
                IssuerSigningKey = holderPublicKey,
                ClockSkew = TimeSpan.FromMinutes(5)
            },
            nonce);

        Console.WriteLine("\nVerifier results:");
        Console.WriteLine($"- Signature + disclosure verification: PASS");
        Console.WriteLine($"- Key binding verification: {(verification.KeyBindingVerified ? "PASS" : "FAIL")}");
        Console.WriteLine($"- mission_code disclosed: {verification.ClaimsPrincipal.FindFirst("mission_code")?.Value}");

        Console.WriteLine("\nResult: scenario uses real PQC primitives when available on net10 platform crypto providers, with deterministic fallback.");
    }

    private static string CreateSignedAlgorithmPolicy(SecurityKey trustAuthorityKey)
    {
        var payload = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://trust.sovereign.example",
            [JwtRegisteredClaimNames.Sub] = "https://trust.sovereign.example/policy/algorithm-profile",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddHours(4).ToUnixTimeSeconds(),
            ["current_signing_suite"] = "ES256",
            ["pqc_target_suite"] = "ML-DSA-87",
            ["pqc_required_from"] = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)).ToString("yyyy-MM-dd")
        };

        var header = new JwtHeader(new SigningCredentials(trustAuthorityKey, SecurityAlgorithms.EcdsaSha256))
        {
            [JwtHeaderParameterNames.Typ] = "entity-statement+jwt"
        };

        var token = new JwtSecurityToken(header, payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AlgorithmPolicy ValidateAlgorithmPolicy(string policyJwt, SecurityKey trustAuthorityKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            policyJwt,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://trust.sovereign.example",
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = trustAuthorityKey,
                ClockSkew = TimeSpan.FromMinutes(2)
            },
            out _);

        var currentSuite = principal.FindFirst("current_signing_suite")?.Value ?? "unknown";
        var pqcTargetSuite = principal.FindFirst("pqc_target_suite")?.Value ?? "unknown";
        var pqcRequiredFromRaw = principal.FindFirst("pqc_required_from")?.Value ?? "1970-01-01";

        var pqcRequiredFrom = DateOnly.ParseExact(pqcRequiredFromRaw, "yyyy-MM-dd");
        return new AlgorithmPolicy(currentSuite, pqcTargetSuite, pqcRequiredFrom);
    }

    private static EncryptedEnvelope EncryptPresentation(string presentation, byte[] sharedKey)
    {
        var plaintext = Encoding.UTF8.GetBytes(presentation);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(DeriveAes256Key(sharedKey), tagSizeInBytes: 16);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        return new EncryptedEnvelope(nonce, ciphertext, tag);
    }

    private static string DecryptPresentation(EncryptedEnvelope envelope, byte[] sharedKey)
    {
        var plaintext = new byte[envelope.Ciphertext.Length];

        using var aes = new AesGcm(DeriveAes256Key(sharedKey), tagSizeInBytes: 16);
        aes.Decrypt(envelope.Nonce, envelope.Ciphertext, envelope.Tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] DeriveAes256Key(byte[] sourceKeyMaterial)
    {
        if (sourceKeyMaterial.Length is 16 or 24 or 32)
        {
            return sourceKeyMaterial;
        }

        return SHA256.HashData(sourceKeyMaterial);
    }

    private static PqcExecutionResult TryRunNativePqcDemo(string message)
    {
#if NET10_0_OR_GREATER
        try
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var context = Encoding.UTF8.GetBytes("sd-jwt-dotnet:pqc-demo");

            if (!MLDsa.IsSupported)
            {
                return new PqcExecutionResult(
                    MlDsaStatus: "UNAVAILABLE (platform provider does not support ML-DSA)",
                    MlKemStatus: "NOT ATTEMPTED",
                    TransportKey: null);
            }

            if (!MLKem.IsSupported)
            {
                using var signerOnly = MLDsa.GenerateKey(MLDsaAlgorithm.MLDsa65);
                var signatureOnly = signerOnly.SignData(messageBytes, context);
                var isSignatureValidOnly = signerOnly.VerifyData(messageBytes, signatureOnly, context);

                return new PqcExecutionResult(
                    MlDsaStatus: isSignatureValidOnly ? "PASS" : "FAIL",
                    MlKemStatus: "UNAVAILABLE (platform provider does not support ML-KEM)",
                    TransportKey: null);
            }

            using var signer = MLDsa.GenerateKey(MLDsaAlgorithm.MLDsa65);
            var signature = signer.SignData(messageBytes, context);
            var isSignatureValid = signer.VerifyData(messageBytes, signature, context);

            using var kemReceiver = MLKem.GenerateKey(MLKemAlgorithm.MLKem768);
            kemReceiver.Encapsulate(out var encapsulatedCiphertext, out var senderSharedSecret);
            var receiverSharedSecret = kemReceiver.Decapsulate(encapsulatedCiphertext);

            var keyAgreementValid = CryptographicOperations.FixedTimeEquals(senderSharedSecret, receiverSharedSecret);

            return new PqcExecutionResult(
                MlDsaStatus: isSignatureValid ? "PASS" : "FAIL",
                MlKemStatus: keyAgreementValid ? "PASS" : "FAIL",
                TransportKey: keyAgreementValid ? senderSharedSecret : null);
        }
        catch (Exception ex)
        {
            return new PqcExecutionResult(
                MlDsaStatus: $"ERROR ({ex.GetType().Name})",
                MlKemStatus: "ERROR",
                TransportKey: null);
        }
#else
        return new PqcExecutionResult(
            MlDsaStatus: "UNAVAILABLE (requires net10+)",
            MlKemStatus: "UNAVAILABLE (requires net10+)",
            TransportKey: null);
#endif
    }

    private static class QkdNode
    {
        public static byte[] GenerateSharedSymmetricKey() => RandomNumberGenerator.GetBytes(32);
    }

    private sealed record AlgorithmPolicy(string CurrentSigningSuite, string PqcTargetSuite, DateOnly PqcRequiredFrom);

    private sealed record EncryptedEnvelope(byte[] Nonce, byte[] Ciphertext, byte[] Tag);

    private sealed record PqcExecutionResult(string MlDsaStatus, string MlKemStatus, byte[]? TransportKey);
}
