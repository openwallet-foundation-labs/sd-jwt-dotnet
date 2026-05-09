using FluentAssertions;
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Serialization;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.VcDm.Tests;

public class VerifiablePresentationTests
{
    [Fact]
    public void Serialize_MinimalPresentation_ProducesCorrectJson()
    {
        var vp = new VerifiablePresentation();
        var json = JsonSerializer.Serialize(vp, VcDmSerializerOptions.Default);

        json.Should().Contain("\"@context\"");
        json.Should().Contain(VcDmContexts.V2);
        json.Should().Contain("\"VerifiablePresentation\"");
    }

    [Fact]
    public void Deserialize_Presentation_Roundtrips()
    {
        var vp = new VerifiablePresentation
        {
            Id = "https://example.com/presentations/1",
            VerifiableCredential = ["eyJhbGci...SD-JWT-KB-string"]
        };

        var json = JsonSerializer.Serialize(vp, VcDmSerializerOptions.Default);
        var restored = JsonSerializer.Deserialize<VerifiablePresentation>(json, VcDmSerializerOptions.Default);

        restored!.Id.Should().Be("https://example.com/presentations/1");
        restored.VerifiableCredential.Should().HaveCount(1);
    }

    [Fact]
    public void Serialize_PresentationWithProof_IncludesProofFields()
    {
        var vp = new VerifiablePresentation
        {
            Proof =
            [
                new DataIntegrityProof
                {
                    Cryptosuite = "ecdsa-rdfc-2019",
                    Challenge = "abc-nonce-123",
                    Domain = "https://verifier.example.com",
                    ProofPurpose = "authentication",
                    ProofValue = "z3FXQjecWufY46yg"
                }
            ]
        };

        var json = JsonSerializer.Serialize(vp, VcDmSerializerOptions.Default);

        json.Should().Contain("DataIntegrityProof");
        json.Should().Contain("ecdsa-rdfc-2019");
        json.Should().Contain("abc-nonce-123");
    }

    [Fact]
    public void IsVerifiablePresentation_ReturnsTrue_WhenTypeContainsVp()
    {
        var vp = new VerifiablePresentation();
        vp.IsVerifiablePresentation().Should().BeTrue();
    }

    [Fact]
    public void HasV2Context_ReturnsTrue_WhenV2ContextPresent()
    {
        var vp = new VerifiablePresentation();
        vp.HasV2Context().Should().BeTrue();
    }
}
