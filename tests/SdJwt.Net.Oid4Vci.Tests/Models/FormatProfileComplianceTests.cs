using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.Oid4Vci.Models.Formats;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests.Models;

public class FormatProfileComplianceTests
{
    [Fact]
    public void JwtVcJsonLdCredentialConfiguration_SerializesFinalFormatIdentifier()
    {
        var configuration = new JwtVcJsonLdCredentialConfiguration
        {
            CredentialDefinition = new JwtVcJsonLdCredentialDefinition
            {
                Context = ["https://www.w3.org/ns/credentials/v2"],
                Type = ["VerifiableCredential", "UniversityDegreeCredential"]
            }
        };

        var json = JsonSerializer.Serialize(configuration);

        Assert.Contains("\"format\":\"jwt_vc_json-ld\"", json);
        Assert.Contains("\"@context\"", json);
        Assert.Contains("\"type\"", json);
    }

    [Fact]
    public void CredentialIssuerMetadata_SerializesCredentialResponseEncryption()
    {
        var metadata = new CredentialIssuerMetadata
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialEndpoint = "https://issuer.example.com/credential",
            CredentialConfigurationsSupported = new Dictionary<string, CredentialConfiguration>
            {
                ["pid"] = new SdJwtVcCredentialConfiguration
                {
                    Vct = "https://credentials.example.com/identity_credential"
                }
            },
            CredentialResponseEncryption = new CredentialResponseEncryption
            {
                AlgValuesSupported = ["ECDH-ES"],
                EncValuesSupported = ["A256GCM"],
                EncryptionRequired = true
            }
        };

        var json = JsonSerializer.Serialize(metadata);

        Assert.Contains("\"credential_response_encryption\"", json);
        Assert.Contains("\"alg_values_supported\"", json);
        Assert.Contains("\"encryption_required\":true", json);
    }

    [Fact]
    public void CredentialProofs_Validate_WithMultipleJwtProofs_Succeeds()
    {
        var proofs = new CredentialProofs
        {
            Jwt = ["proof-1", "proof-2"]
        };

        var ex = Record.Exception(() => proofs.Validate());

        Assert.Null(ex);
    }

    [Fact]
    public void CredentialProofs_Validate_WithAttestationAndJwt_Throws()
    {
        var proofs = new CredentialProofs
        {
            Jwt = ["proof-1"],
            Attestation = ["attestation"]
        };

        Assert.Throws<InvalidOperationException>(() => proofs.Validate());
    }
}
