using FluentAssertions;
using SdJwt.Net.Oid4Vci.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests.Models;

public class CredentialRequestProofsTests
{
    [Fact]
    public void Validate_WithProofsJwtOnly_Succeeds()
    {
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegree",
            Proofs = new CredentialProofs
            {
                Jwt = new[] { "proof-jwt-1", "proof-jwt-2" }
            }
        };

        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithBothConfigurationIdAndIdentifier_Throws()
    {
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegree",
            CredentialIdentifier = "some-identifier",
            Proofs = new CredentialProofs { Jwt = new[] { "proof-jwt-1" } }
        };

        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot specify both*");
    }

    [Fact]
    public void Validate_WithEmptyProofs_Throws()
    {
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegree",
            Proofs = new CredentialProofs()
        };

        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*At least one proof entry is required*");
    }
}
