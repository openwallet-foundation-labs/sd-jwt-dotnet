using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.Core.Tests;

/// <summary>
/// Tests for enterprise model types: RequestBinding, DelegationBinding,
/// ToolRegistration, PolicyDecisionBinding, and SignedAuditReceipt.
/// </summary>
public class EnterpriseModelTests
{
    [Fact]
    public void RequestBinding_ComputeHash_WithPayload_ShouldReturnBase64Url()
    {
        var payload = Encoding.UTF8.GetBytes("hello world");
        var hash = RequestBinding.ComputeHash(payload);

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotContain("+");
        hash.Should().NotContain("/");
        hash.Should().NotEndWith("=");
    }

    [Fact]
    public void RequestBinding_ComputeHash_WithNull_ShouldThrow()
    {
        var act = () => RequestBinding.ComputeHash(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RequestBinding_ComputeJsonHash_WithObject_ShouldReturnHash()
    {
        var obj = new
        {
            name = "test",
            value = 42
        };
        var hash = RequestBinding.ComputeJsonHash(obj);

        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RequestBinding_ComputeJsonHash_WithNull_ShouldThrow()
    {
        var act = () => RequestBinding.ComputeJsonHash(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RequestBinding_Properties_ShouldRoundTrip()
    {
        var binding = new RequestBinding
        {
            Method = "POST",
            Uri = "https://api.example.com/tools/call",
            BodyHash = "abc123",
            McpToolId = "crm.contacts",
            McpToolSchemaHash = "schema-hash",
            McpArgumentsHash = "args-hash",
            ContentType = "application/json",
            IdempotencyKey = "idem-key"
        };

        binding.Method.Should().Be("POST");
        binding.Uri.Should().Be("https://api.example.com/tools/call");
        binding.BodyHash.Should().Be("abc123");
        binding.McpToolId.Should().Be("crm.contacts");
        binding.ContentType.Should().Be("application/json");
        binding.IdempotencyKey.Should().Be("idem-key");
    }

    [Fact]
    public void DelegationBinding_Defaults_ShouldHaveMaxDepth3()
    {
        var binding = new DelegationBinding();

        binding.MaxDepth.Should().Be(3);
        binding.Depth.Should().Be(0);
        binding.ParentTokenId.Should().BeEmpty();
        binding.ParentTokenHash.Should().BeEmpty();
    }

    [Fact]
    public void DelegationBinding_Properties_ShouldRoundTrip()
    {
        var binding = new DelegationBinding
        {
            ParentTokenId = "parent-123",
            ParentTokenHash = "hash-abc",
            Depth = 1,
            MaxDepth = 5,
            AllowedDownstreamAudiences = new[] { "tool://a", "tool://b" },
            RootIssuer = "agent://root"
        };

        binding.ParentTokenId.Should().Be("parent-123");
        binding.ParentTokenHash.Should().Be("hash-abc");
        binding.Depth.Should().Be(1);
        binding.MaxDepth.Should().Be(5);
        binding.AllowedDownstreamAudiences.Should().HaveCount(2);
        binding.RootIssuer.Should().Be("agent://root");
    }

    [Fact]
    public void ToolRegistration_Defaults_ShouldBeReasonable()
    {
        var reg = new ToolRegistration
        {
            ToolId = "test-tool",
            Version = "1.0.0",
            SchemaHash = "test-schema-hash",
            ManifestHash = "test-manifest-hash",
            Audience = "tool://test"
        };

        reg.ToolId.Should().Be("test-tool");
        reg.AllowedActions.Should().BeEmpty();
        reg.RequiresProofOfPossession.Should().BeFalse();
        reg.RequiresRequestBinding.Should().BeFalse();
    }

    [Fact]
    public void ToolRegistration_Properties_ShouldRoundTrip()
    {
        var reg = new ToolRegistration
        {
            ToolId = "crm.contacts",
            DisplayName = "CRM Contacts",
            Version = "1.2.0",
            SchemaHash = "schema-hash",
            ManifestHash = "manifest-hash",
            McpServerUri = "https://mcp.example.com",
            Audience = "tool://crm",
            AllowedActions = new[] { "Read", "Write" },
            MaxTokenLifetime = TimeSpan.FromMinutes(10),
            RequiresProofOfPossession = true,
            RequiresRequestBinding = true,
            DataClassification = "Confidential"
        };

        reg.ToolId.Should().Be("crm.contacts");
        reg.DisplayName.Should().Be("CRM Contacts");
        reg.Version.Should().Be("1.2.0");
        reg.AllowedActions.Should().Contain("Write");
        reg.RequiresProofOfPossession.Should().BeTrue();
        reg.DataClassification.Should().Be("Confidential");
    }

    [Fact]
    public void PolicyDecisionBinding_Properties_ShouldRoundTrip()
    {
        var binding = new PolicyDecisionBinding
        {
            DecisionId = "dec-1",
            PolicyId = "policy-A",
            PolicyVersion = "2.0",
            PolicyHash = "policy-hash",
            EvaluatedAt = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero)
        };

        binding.DecisionId.Should().Be("dec-1");
        binding.PolicyId.Should().Be("policy-A");
        binding.PolicyVersion.Should().Be("2.0");
        binding.PolicyHash.Should().Be("policy-hash");
        binding.EvaluatedAt.Year.Should().Be(2025);
    }

    [Fact]
    public void SignedAuditReceipt_Properties_ShouldRoundTrip()
    {
        var receipt = new SignedAuditReceipt
        {
            Receipt = new AuditReceipt { TokenId = "tok-1", Decision = ReceiptDecision.Allow },
            Signature = "jws-sig",
            KeyId = "key-1",
            Algorithm = "ES256",
            PreviousReceiptHash = "prev-hash"
        };

        receipt.Receipt.TokenId.Should().Be("tok-1");
        receipt.Signature.Should().Be("jws-sig");
        receipt.KeyId.Should().Be("key-1");
        receipt.Algorithm.Should().Be("ES256");
        receipt.PreviousReceiptHash.Should().Be("prev-hash");
    }

    [Fact]
    public void ProofValidationResult_Success_ShouldSetProperties()
    {
        var result = ProofValidationResult.Success("dpop", "thumb-123");

        result.IsValid.Should().BeTrue();
        result.ProofType.Should().Be("dpop");
        result.JwkThumbprint.Should().Be("thumb-123");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ProofValidationResult_Failure_ShouldSetProperties()
    {
        var result = ProofValidationResult.Failure("bad proof", "invalid_proof", "mtls");

        result.IsValid.Should().BeFalse();
        result.ProofType.Should().Be("mtls");
        result.Error.Should().Be("bad proof");
        result.ErrorCode.Should().Be("invalid_proof");
    }

    [Fact]
    public void AgentTrustSecurityMode_ShouldHaveThreeModes()
    {
        Enum.GetValues(typeof(AgentTrustSecurityMode)).Length.Should().Be(4);
    }

    [Fact]
    public void AgentTrustTokenTypes_ShouldHaveExpectedValues()
    {
        AgentTrustTokenTypes.CapabilitySdJwt.Should().Be("agent-cap+sd-jwt");
        AgentTrustTokenTypes.CapabilityContentType.Should().Be("application/agent-capability+json");
        AgentTrustTokenTypes.LegacySdJwtVc.Should().Be("vc+sd-jwt");
    }

    [Fact]
    public void CapabilityClaim_EnterpriseProperties_ShouldRoundTrip()
    {
        var claim = new CapabilityClaim
        {
            Tool = "Weather",
            Action = "Read",
            ToolId = "weather-v2",
            ToolVersion = "2.0.0",
            ToolManifestHash = "manifest-hash-abc",
            ResourceType = "api",
            DataClassification = "Public"
        };

        claim.ToolId.Should().Be("weather-v2");
        claim.ToolVersion.Should().Be("2.0.0");
        claim.ToolManifestHash.Should().Be("manifest-hash-abc");
        claim.ResourceType.Should().Be("api");
        claim.DataClassification.Should().Be("Public");
    }

    [Fact]
    public void AuditReceipt_EnterpriseProperties_ShouldRoundTrip()
    {
        var receipt = new AuditReceipt
        {
            TokenId = "tok-1",
            Decision = ReceiptDecision.Allow,
            RequestHash = "req-hash-123",
            ProofType = "dpop"
        };

        receipt.RequestHash.Should().Be("req-hash-123");
        receipt.ProofType.Should().Be("dpop");
    }

    [Fact]
    public void CapabilityTokenOptions_RequestBinding_ShouldRoundTrip()
    {
        var options = new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            RequestBinding = new RequestBinding { Method = "POST", Uri = "https://api.example.com" },
            Delegation = new DelegationBinding { ParentTokenId = "parent-1", ParentTokenHash = "hash-1" }
        };

        options.RequestBinding.Should().NotBeNull();
        options.RequestBinding!.Method.Should().Be("POST");
        options.Delegation.Should().NotBeNull();
        options.Delegation!.ParentTokenId.Should().Be("parent-1");
    }

    [Fact]
    public void AgentTrustVerificationContext_Defaults_ShouldBeSecure()
    {
        var ctx = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://test",
            TrustedIssuers = new Dictionary<string, SecurityKey>()
        };

        ctx.MaxTokenLifetime.Should().Be(TimeSpan.FromMinutes(5));
        ctx.MaxIssuedAtAge.Should().Be(TimeSpan.FromMinutes(5));
        ctx.ClockSkewTolerance.Should().Be(TimeSpan.FromSeconds(30));
        ctx.EnforceReplayPrevention.Should().BeTrue();
        ctx.SecurityMode.Should().Be(AgentTrustSecurityMode.Pilot);
        ctx.AllowedAlgorithms.Should().NotBeNull();
        ctx.AcceptedTokenTypes.Should().NotBeNull();
        ctx.AcceptedTokenTypes.Should().Contain(AgentTrustTokenTypes.CapabilitySdJwt);
        ctx.AcceptedTokenTypes.Should().Contain(AgentTrustTokenTypes.LegacySdJwtVc);
    }

    [Fact]
    public void CapabilityMintRequest_Defaults_ShouldBeReasonable()
    {
        var req = new CapabilityMintRequest
        {
            SubjectAgent = "agent://test",
            Audience = "tool://test",
            Caller = new WorkloadIdentity { SubjectId = "test-client" }
        };

        req.RequestedLifetime.Should().Be(TimeSpan.FromSeconds(60));
        req.SubjectAgent.Should().Be("agent://test");
        req.Audience.Should().Be("tool://test");
    }

    [Fact]
    public void CapabilityMintResult_Properties_ShouldRoundTrip()
    {
        var result = new CapabilityMintResult
        {
            Token = "jwt-token",
            TokenId = "tid-1",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            DecisionId = "dec-1",
            DecisionReceipt = new SignedAuditReceipt { Signature = "sig" }
        };

        result.Token.Should().Be("jwt-token");
        result.TokenId.Should().Be("tid-1");
        result.DecisionId.Should().Be("dec-1");
        result.DecisionReceipt.Should().NotBeNull();
    }
}
