using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Validators;

public class HaipValidatorTests {
    [Fact]
    public async Task ValidateRequestAsync_Level1_WithProof_ShouldBeCompliant()
    {
        var validator = new HaipProtocolValidator(HaipLevel.Level1_High, NullLogger<HaipProtocolValidator>.Instance);
        var request = new Dictionary<string, object>
        {
            ["proof"] = "proof-jwt"
        };

        var result = await validator.ValidateRequestAsync(request, HaipLevel.Level1_High);

        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateRequestAsync_Level2_WithoutWalletAttestation_ShouldFail()
    {
        var validator = new HaipProtocolValidator(HaipLevel.Level2_VeryHigh, NullLogger<HaipProtocolValidator>.Instance);
        var request = new Dictionary<string, object>
        {
            ["proof"] = "proof-jwt",
            ["dpop"] = "dpop-proof"
        };

        var result = await validator.ValidateRequestAsync(request, HaipLevel.Level2_VeryHigh);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.InsecureClientAuthentication);
    }

    [Fact]
    public async Task ValidateRequestAsync_Level2_WithWalletAttestationAndDpop_ShouldBeCompliant()
    {
        var validator = new HaipProtocolValidator(HaipLevel.Level2_VeryHigh, NullLogger<HaipProtocolValidator>.Instance);
        var request = new Dictionary<string, object>
        {
            ["proof"] = "proof-jwt",
            ["wallet_attestation"] = "attestation-token",
            ["dpop"] = "dpop-proof"
        };

        var result = await validator.ValidateRequestAsync(request, HaipLevel.Level2_VeryHigh);

        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateRequestAsync_Level3_WithoutHsmFlag_ShouldFail()
    {
        var validator = new HaipProtocolValidator(HaipLevel.Level3_Sovereign, NullLogger<HaipProtocolValidator>.Instance);
        var request = new Dictionary<string, object>
        {
            ["proof"] = "proof-jwt",
            ["wallet_attestation"] = "attestation-token",
            ["dpop"] = "dpop-proof"
        };

        var result = await validator.ValidateRequestAsync(request, HaipLevel.Level3_Sovereign);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Description.Contains("Hardware Security Module", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateTransportSecurity_WithHttp_ShouldFail()
    {
        var validator = new HaipProtocolValidator(HaipLevel.Level1_High, NullLogger<HaipProtocolValidator>.Instance);

        var result = validator.ValidateTransportSecurity("http://example.com", HaipLevel.Level1_High);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.InsecureTransport);
    }

    [Fact]
    public void ValidateClientAuthentication_Level2_WithClientSecretJwt_ShouldFail()
    {
        var validator = new HaipProtocolValidator(HaipLevel.Level2_VeryHigh, NullLogger<HaipProtocolValidator>.Instance);

        var result = validator.ValidateClientAuthentication("client_secret_jwt", HaipLevel.Level2_VeryHigh);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.InsecureClientAuthentication);
    }
}
