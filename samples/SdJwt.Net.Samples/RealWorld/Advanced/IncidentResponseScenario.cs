using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.RealWorld.Advanced;

/// <summary>
/// Demonstrates executable incident containment for issuer key compromise:
/// trust revocation plus status-list revocation with verifier enforcement.
/// </summary>
public static class IncidentResponseScenario
{
    public static async Task RunScenario()
    {
        Console.WriteLine("\n=======================================================");
        Console.WriteLine("   INCIDENT RESPONSE: AUTOMATED TRUST CONTAINMENT       ");
        Console.WriteLine("=======================================================\n");

        using var compromisedIssuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var statusAuthorityEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var compromisedIssuerKey = new ECDsaSecurityKey(compromisedIssuerEcdsa) { KeyId = "regional-bank-compromised-k1" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "wallet-key-incident-1" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "wallet-key-incident-1" };
        var statusAuthorityKey = new ECDsaSecurityKey(statusAuthorityEcdsa) { KeyId = "status-authority-k1" };

        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);
        var vcIssuer = new SdJwtVcIssuer(compromisedIssuerKey, SecurityAlgorithms.EcdsaSha256);
        var statusManager = new StatusListManager(statusAuthorityKey, SecurityAlgorithms.EcdsaSha256);

        const string compromisedIssuer = "https://regional-bank.example.com";
        const string statusListUri = "https://status.example.com/lists/regional-bank/1";
        const int credentialIndex = 8842;
        const string verifierAudience = "https://financial-verifier.example.com";
        const string nonce = "incident-check-2026-001";

        var statusBits = statusManager.CreateStatusBits(10000);
        var currentStatusListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(statusListUri, statusBits);
        var publishedStatusTokens = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [statusListUri] = currentStatusListToken
        };

        var credential = vcIssuer.Issue(
            "https://credentials.regional-bank.example.com/high-net-worth",
            new SdJwtVcPayload
            {
                Issuer = compromisedIssuer,
                Subject = "did:example:customer-771",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds(),
                Status = new
                {
                    status_list = new StatusListReference
                    {
                        Index = credentialIndex,
                        Uri = statusListUri
                    }
                },
                AdditionalData = new Dictionary<string, object>
                {
                    ["account_tier"] = "platinum",
                    ["high_net_worth"] = true
                }
            },
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    high_net_worth = true
                }
            },
            holderJwk);

        var holder = new SdJwtHolder(credential.Issuance);
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "high_net_worth",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = verifierAudience,
                [JwtRegisteredClaimNames.Nonce] = nonce,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256);

        var trustStore = new Dictionary<string, SecurityKey>(StringComparer.Ordinal)
        {
            [compromisedIssuer] = compromisedIssuerKey
        };

        Console.WriteLine("Pre-incident verification...");
        var preIncident = await VerifyWithTrustAndStatusAsync(
            presentation,
            holderPublicKey,
            trustStore,
            verifierAudience,
            nonce,
            publishedStatusTokens,
            statusManager);

        Console.WriteLine($"- Trust check: {(preIncident.IsTrustValid ? "PASS" : "FAIL")}");
        Console.WriteLine($"- Status check: {(preIncident.IsStatusValid ? "PASS" : "FAIL")} ({preIncident.StatusName})");
        Console.WriteLine($"- Final decision: {(preIncident.Accepted ? "ACCEPT" : "REJECT")}");

        Console.WriteLine("\nIncident detected: issuer private key compromise.");
        Console.WriteLine("Containment actions:");

        trustStore.Remove(compromisedIssuer);
        Console.WriteLine("- Trust containment: issuer removed from active trust set.");

        statusManager.SetCredentialStatus(statusBits, credentialIndex, StatusType.Invalid);
        currentStatusListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(statusListUri, statusBits);
        publishedStatusTokens[statusListUri] = currentStatusListToken;
        Console.WriteLine("- Lifecycle containment: status index marked revoked and republished.");

        Console.WriteLine("\nAttacker replays presentation after containment...");
        var postIncidentTrust = await VerifyWithTrustAndStatusAsync(
            presentation,
            holderPublicKey,
            trustStore,
            verifierAudience,
            nonce,
            publishedStatusTokens,
            statusManager);

        Console.WriteLine($"- Trust check: {(postIncidentTrust.IsTrustValid ? "PASS" : "FAIL")}");
        Console.WriteLine($"- Failure reason: {postIncidentTrust.FailureReason}");
        Console.WriteLine($"- Final decision: {(postIncidentTrust.Accepted ? "ACCEPT" : "REJECT")}");

        Console.WriteLine("\nIf a verifier had stale trust metadata, status containment still blocks it:");
        var staleTrustStore = new Dictionary<string, SecurityKey>(StringComparer.Ordinal)
        {
            [compromisedIssuer] = compromisedIssuerKey
        };

        var postIncidentStatus = await VerifyWithTrustAndStatusAsync(
            presentation,
            holderPublicKey,
            staleTrustStore,
            verifierAudience,
            nonce,
            publishedStatusTokens,
            statusManager);

        Console.WriteLine($"- Trust check: {(postIncidentStatus.IsTrustValid ? "PASS" : "FAIL")}");
        Console.WriteLine($"- Status check: {(postIncidentStatus.IsStatusValid ? "PASS" : "FAIL")} ({postIncidentStatus.StatusName})");
        Console.WriteLine($"- Final decision: {(postIncidentStatus.Accepted ? "ACCEPT" : "REJECT")}");

        Console.WriteLine("\nResult: containment is enforced by executable trust + status verification, not narrative-only logging.");
    }

    private static async Task<ContainmentResult> VerifyWithTrustAndStatusAsync(
        string presentation,
        SecurityKey holderPublicKey,
        IReadOnlyDictionary<string, SecurityKey> trustStore,
        string expectedAudience,
        string expectedNonce,
        IReadOnlyDictionary<string, string> publishedStatusTokens,
        StatusListManager statusManager)
    {
        try
        {
            var sdVerifier = new SdVerifier(jwt =>
            {
                var issuer = jwt.Payload.Iss;
                if (issuer == null || !trustStore.TryGetValue(issuer, out var issuerKey))
                {
                    throw new SecurityTokenException($"Issuer '{issuer ?? "unknown"}' is not trusted.");
                }

                return Task.FromResult(issuerKey);
            });

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = trustStore.Keys,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var kbValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = expectedAudience,
                ValidateLifetime = false,
                IssuerSigningKey = holderPublicKey,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var sdResult = await sdVerifier.VerifyAsync(presentation, validationParams, kbValidationParams, expectedNonce);

            var statusClaimJson = sdResult.ClaimsPrincipal.FindFirst("status")?.Value
                ?? throw new SecurityTokenException("Credential missing required status claim.");

            using var statusDocument = JsonDocument.Parse(statusClaimJson);
            if (!statusDocument.RootElement.TryGetProperty("status_list", out var statusListElement))
            {
                throw new SecurityTokenException("Status claim does not contain status_list.");
            }

            var credentialIndex = ReadInt(statusListElement, "idx") ?? ReadInt(statusListElement, "index");
            var statusUri = ReadString(statusListElement, "uri");

            if (credentialIndex is null || string.IsNullOrWhiteSpace(statusUri))
            {
                throw new SecurityTokenException("Status claim is missing idx/index or uri.");
            }

            var statusClaim = new StatusClaim
            {
                StatusList = new StatusListReference
                {
                    Index = credentialIndex.Value,
                    Uri = statusUri
                }
            };

            if (!publishedStatusTokens.TryGetValue(statusClaim.StatusList.Uri, out var statusListToken))
            {
                throw new SecurityTokenException("Status list token not found for URI.");
            }

            var statusBits = StatusListManager.GetBitsFromToken(statusListToken);
            var credentialStatus = statusManager.GetCredentialStatus(statusBits, statusClaim.StatusList.Index);
            var isStatusValid = credentialStatus == StatusType.Valid;
            var statusName = credentialStatus.GetName();

            return new ContainmentResult(
                IsTrustValid: true,
                IsStatusValid: isStatusValid,
                StatusName: statusName,
                Accepted: isStatusValid,
                FailureReason: isStatusValid ? string.Empty : "Credential status is not valid.");
        }
        catch (Exception ex)
        {
            return new ContainmentResult(
                IsTrustValid: false,
                IsStatusValid: false,
                StatusName: "unknown",
                Accepted: false,
                FailureReason: ex.Message);
        }
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.TryGetInt32(out var numericValue) ? numericValue : null,
            JsonValueKind.String => int.TryParse(property.GetString(), out var stringValue) ? stringValue : null,
            _ => null
        };
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return property.ValueKind == JsonValueKind.Number ? property.GetRawText() : null;
    }

    private sealed record ContainmentResult(
        bool IsTrustValid,
        bool IsStatusValid,
        string StatusName,
        bool Accepted,
        string FailureReason);
}
