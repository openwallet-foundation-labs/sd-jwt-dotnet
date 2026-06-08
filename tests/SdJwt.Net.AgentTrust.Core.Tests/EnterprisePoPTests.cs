using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.Core.Tests;

/// <summary>
/// Tests for proof-of-possession (DPoP) and request-binding enforcement in
/// <see cref="CapabilityTokenVerifier"/> strict mode.
/// </summary>
public class EnterprisePoPTests
{
    private static readonly SymmetricSecurityKey IssuerKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    private static (CapabilityTokenIssuer Issuer, CapabilityTokenVerifier Verifier) CreatePair()
    {
        var nonceStore = new MemoryNonceStore();
        return (new CapabilityTokenIssuer(IssuerKey, SecurityAlgorithms.HmacSha256, nonceStore),
                new CapabilityTokenVerifier(nonceStore));
    }

    private static AgentTrustVerificationContext BaseContext(
        ProofMaterial? proof = null,
        HttpRequestBinding? actualRequest = null,
        bool requirePop = false,
        bool requireBinding = false)
    {
        return new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = IssuerKey },
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            EnforceReplayPrevention = false,
            RequireProofOfPossession = requirePop,
            RequireRequestBinding = requireBinding,
            ProofMaterial = proof,
            ActualRequest = actualRequest
        };
    }

    private static (ECDsa Private, Dictionary<string, object> PublicJwk, string Jkt) CreateHolderKey()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var pubParams = ecdsa.ExportParameters(false);
        var pubJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(
            new ECDsaSecurityKey(ECDsa.Create(pubParams)));
        var jkt = Base64UrlEncoder.Encode(pubJwk.ComputeJwkThumbprint());
        var jwkDict = new Dictionary<string, object>
        {
            ["kty"] = "EC",
            ["crv"] = "P-256",
            ["x"] = pubJwk.X,
            ["y"] = pubJwk.Y
        };
        return (ecdsa, jwkDict, jkt);
    }

    private static string BuildDpopProof(
        ECDsa holderPrivate,
        Dictionary<string, object> publicJwk,
        string htm,
        string htu,
        string? ath = null,
        string typ = "dpop+jwt",
        long? iatOverride = null)
    {
        var signingKey = new ECDsaSecurityKey(holderPrivate);
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256);
        var header = new JwtHeader(creds);
        header["typ"] = typ;
        header["jwk"] = publicJwk;

        var payload = new JwtPayload
        {
            ["htm"] = htm,
            ["htu"] = htu,
            ["jti"] = Guid.NewGuid().ToString("N"),
            ["iat"] = iatOverride ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        if (ath != null)
        {
            payload["ath"] = ath;
        }

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header, payload));
    }

    [Fact]
    public async Task VerifyAsync_WithValidDpopProof_ShouldSucceed()
    {
        var (issuer, verifier) = CreatePair();
        var (holderPrivate, publicJwk, jkt) = CreateHolderKey();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-pop-1" },
            SenderConstraint = SenderConstraint.Dpop(jkt)
        });

        var actualRequest = new HttpRequestBinding { Method = "POST", Uri = "https://api.example.com/weather" };
        var dpop = BuildDpopProof(holderPrivate, publicJwk, "POST", "https://api.example.com/weather");

        var context = BaseContext(
            proof: new ProofMaterial { DpopProof = dpop },
            actualRequest: actualRequest,
            requirePop: true);

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeTrue(result.Error);
        result.ProofType.Should().Be("dpop");
    }

    [Fact]
    public async Task VerifyAsync_WithDpopProofFromDifferentKey_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();
        var (_, _, boundJkt) = CreateHolderKey();
        var (attackerPrivate, attackerJwk, _) = CreateHolderKey();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-pop-2" },
            SenderConstraint = SenderConstraint.Dpop(boundJkt)
        });

        // Proof signed by a different key than the one the token was bound to.
        var dpop = BuildDpopProof(attackerPrivate, attackerJwk, "POST", "https://api.example.com/weather");

        var context = BaseContext(
            proof: new ProofMaterial { DpopProof = dpop },
            actualRequest: new HttpRequestBinding { Method = "POST", Uri = "https://api.example.com/weather" },
            requirePop: true);

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("dpop_jkt_mismatch");
    }

    [Fact]
    public async Task VerifyAsync_WithDpopMethodMismatch_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();
        var (holderPrivate, publicJwk, jkt) = CreateHolderKey();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-pop-3" },
            SenderConstraint = SenderConstraint.Dpop(jkt)
        });

        var dpop = BuildDpopProof(holderPrivate, publicJwk, "GET", "https://api.example.com/weather");

        var context = BaseContext(
            proof: new ProofMaterial { DpopProof = dpop },
            actualRequest: new HttpRequestBinding { Method = "POST", Uri = "https://api.example.com/weather" },
            requirePop: true);

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("dpop_htm_mismatch");
    }

    [Fact]
    public async Task VerifyAsync_RequirePoP_WithoutProofMaterial_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();
        var (_, _, jkt) = CreateHolderKey();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-pop-4" },
            SenderConstraint = SenderConstraint.Dpop(jkt)
        });

        var context = BaseContext(requirePop: true); // no ProofMaterial

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("missing_proof");
    }

    [Fact]
    public async Task VerifyAsync_RequirePoP_TokenWithoutCnf_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();
        var (holderPrivate, publicJwk, _) = CreateHolderKey();

        // Token minted WITHOUT a sender constraint (no cnf).
        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-pop-5" }
        });

        var dpop = BuildDpopProof(holderPrivate, publicJwk, "POST", "https://api.example.com/weather");
        var context = BaseContext(
            proof: new ProofMaterial { DpopProof = dpop },
            actualRequest: new HttpRequestBinding { Method = "POST", Uri = "https://api.example.com/weather" },
            requirePop: true);

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("missing_cnf");
    }

    [Fact]
    public async Task VerifyAsync_WithMatchingRequestBinding_ShouldSucceed()
    {
        var (issuer, verifier) = CreatePair();
        var body = Encoding.UTF8.GetBytes("{\"city\":\"Sydney\"}");

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-rb-1" },
            RequestBinding = new RequestBinding
            {
                Method = "POST",
                Uri = "https://api.example.com/weather",
                BodyHash = RequestBinding.ComputeHash(body)
            }
        });

        var context = BaseContext(
            actualRequest: new HttpRequestBinding
            {
                Method = "POST",
                Uri = "https://api.example.com/weather",
                Body = body
            },
            requireBinding: true);

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeTrue(result.Error);
    }

    [Fact]
    public async Task VerifyAsync_WithTamperedRequestBody_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();
        var body = Encoding.UTF8.GetBytes("{\"city\":\"Sydney\"}");
        var tampered = Encoding.UTF8.GetBytes("{\"city\":\"Melbourne\"}");

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-rb-2" },
            RequestBinding = new RequestBinding
            {
                Method = "POST",
                Uri = "https://api.example.com/weather",
                BodyHash = RequestBinding.ComputeHash(body)
            }
        });

        var context = BaseContext(
            actualRequest: new HttpRequestBinding
            {
                Method = "POST",
                Uri = "https://api.example.com/weather",
                Body = tampered
            },
            requireBinding: true);

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("request_binding_mismatch");
    }

    [Fact]
    public async Task VerifyAsync_RequireBinding_TokenWithoutReqBind_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-rb-3" }
        });

        var context = BaseContext(
            actualRequest: new HttpRequestBinding { Method = "POST", Uri = "https://api.example.com/weather" },
            requireBinding: true);

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("missing_request_binding");
    }
}
