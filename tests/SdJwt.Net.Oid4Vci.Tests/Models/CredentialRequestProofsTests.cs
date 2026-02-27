using FluentAssertions;
using SdJwt.Net.Oid4Vci.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests.Models;

public class CredentialRequestProofsTests {
    [Fact]
    public void Validate_WithProofsJwtOnly_Succeeds()
    {
        var request = new CredentialRequest
        {
            Format = Oid4VciConstants.SdJwtVcFormat,
            Vct = "https://example.com/UniversityDegree",
            Proofs = new CredentialProofs
            {
                Jwt = new[] { "proof-jwt-1", "proof-jwt-2" }
            }
        };

        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithProofAndProofs_Throws()
    {
        var request = new CredentialRequest
        {
            Format = Oid4VciConstants.SdJwtVcFormat,
            Vct = "https://example.com/UniversityDegree",
            Proof = new CredentialProof { ProofType = "jwt", Jwt = "proof-jwt-1" },
            Proofs = new CredentialProofs { Jwt = new[] { "proof-jwt-2" } }
        };

        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*both proof and proofs*");
    }

    [Fact]
    public void Validate_WithEmptyProofs_Throws()
    {
        var request = new CredentialRequest
        {
            Format = Oid4VciConstants.SdJwtVcFormat,
            Vct = "https://example.com/UniversityDegree",
            Proofs = new CredentialProofs()
        };

        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*At least one proof entry is required*");
    }
}
