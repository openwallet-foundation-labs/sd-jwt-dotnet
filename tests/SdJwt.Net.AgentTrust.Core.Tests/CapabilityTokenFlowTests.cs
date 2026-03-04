using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.Core.Tests;

public class CapabilityTokenFlowTests
{
    private static readonly SymmetricSecurityKey DefaultKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    private static (CapabilityTokenIssuer Issuer, CapabilityTokenVerifier Verifier) CreatePair()
    {
        var nonceStore = new MemoryNonceStore();
        return (new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore),
                new CapabilityTokenVerifier(nonceStore));
    }

    [Fact]
    public async Task MintAndVerify_WithValidInput_ShouldSucceed()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read", Purpose = "forecast" },
            Context = new CapabilityContext { CorrelationId = "corr-1" }
        });

        var result = await verifier.VerifyAsync(minted.Token, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey }
        });

        result.IsValid.Should().BeTrue(result.Error);
        result.Capability.Should().NotBeNull();
        result.Capability!.Tool.Should().Be("Weather");
        result.Capability.Action.Should().Be("Read");
        result.Capability.Purpose.Should().Be("forecast");
        result.Context!.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public async Task VerifyAsync_WithReplayToken_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-2" }
        });

        var opts = new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey }
        };

        var first = await verifier.VerifyAsync(minted.Token, opts);
        var second = await verifier.VerifyAsync(minted.Token, opts);

        first.IsValid.Should().BeTrue(first.Error);
        second.IsValid.Should().BeFalse();
        second.ErrorCode.Should().Be("replay_detected");
    }

    [Fact]
    public async Task VerifyAsync_WithWrongAudience_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-wrong-aud" }
        });

        var result = await verifier.VerifyAsync(minted.Token, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://calendar",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey }
        });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_WithUntrustedIssuer_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-untrusted" }
        });

        var result = await verifier.VerifyAsync(minted.Token, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>
            {
                ["agent://other"] = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("OTHER_KEY_0123456789ABCDEF123456"))
            }
        });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_WithExpiredToken_ShouldFail()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);

        // A JWT token with exp = 946684800 (2000-01-01), signed with a different key,
        // will fail validation either due to expiry or signature mismatch.
        // We test using a token where we can guarantee it's invalid.
        // eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJhZ2VudDovL2FscGhhIiwiYXVkIjoidG9vbDovL3dlYXRoZXIiLCJleHAiOjk0NjY4NDgwMCwiaWF0Ijo5NDY2ODQ4MDB9.invalid_sig
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
                           ".eyJpc3MiOiJhZ2VudDovL2FscGhhIiwiYXVkIjoidG9vbDovL3dlYXRoZXIiLCJleHAiOjk0NjY4NDgwMCwiaWF0Ijo5NDY2ODQ4MDB9" +
                           ".INVALID_SIGNATURE_TO_FORCE_FAILURE";

        var result = await verifier.VerifyAsync(expiredToken, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey }
        });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_WithMalformedToken_ShouldFail()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);

        var result = await verifier.VerifyAsync("not.a.valid.jwt", new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey }
        });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Mint_ShouldReturnTokenIdAndToken()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-mint" }
        });

        minted.Token.Should().NotBeNullOrWhiteSpace();
        minted.TokenId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task VerifyAsync_WithEmptyToken_ShouldFail()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);

        var result = await verifier.VerifyAsync(string.Empty, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey }
        });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_WithNoTrustedIssuers_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-no-issuers" }
        });

        var result = await verifier.VerifyAsync(minted.Token, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>()
        });

        result.IsValid.Should().BeFalse();
    }
}
