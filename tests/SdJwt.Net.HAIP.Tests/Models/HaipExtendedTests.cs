using FluentAssertions;
using SdJwt.Net.HAIP.Models;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Models;

/// <summary>
/// Extended test coverage for HAIP models to ensure comprehensive coverage.
/// </summary>
public class HaipExtendedTests
{
    [Fact]
    public void HaipRequest_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var request = new HaipRequest();

        // Assert
        request.Endpoint.Should().BeNull();
        request.Nonce.Should().BeNull();
        request.State.Should().BeNull();
        request.AuthorizationDetails.Should().BeNull();
        request.WalletData.Should().BeNull();
    }

    [Fact]
    public void HaipRequest_Properties_ShouldBeSettable()
    {
        // Arrange
        var request = new HaipRequest();
        var authDetails = new AuthorizationDetailsRequest();
        var walletData = new WalletData();

        // Act
        request.Endpoint = "https://wallet.example.com/haip";
        request.Nonce = "test-nonce-123";
        request.State = "test-state-456";
        request.AuthorizationDetails = authDetails;
        request.WalletData = walletData;

        // Assert
        request.Endpoint.Should().Be("https://wallet.example.com/haip");
        request.Nonce.Should().Be("test-nonce-123");
        request.State.Should().Be("test-state-456");
        request.AuthorizationDetails.Should().Be(authDetails);
        request.WalletData.Should().Be(walletData);
    }

    [Fact]
    public void HaipResponse_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var response = new HaipResponse();

        // Assert
        response.ResponseData.Should().BeNull();
        response.State.Should().BeNull();
        response.Error.Should().BeNull();
        response.ErrorDescription.Should().BeNull();
    }

    [Fact]
    public void HaipResponse_Properties_ShouldBeSettable()
    {
        // Arrange
        var response = new HaipResponse();
        var responseData = new Dictionary<string, object>
        {
            ["result"] = "success",
            ["credential"] = "credential-data"
        };

        // Act
        response.ResponseData = responseData;
        response.State = "test-state-456";
        response.Error = "invalid_request";
        response.ErrorDescription = "The request is malformed";

        // Assert
        response.ResponseData.Should().BeEquivalentTo(responseData);
        response.State.Should().Be("test-state-456");
        response.Error.Should().Be("invalid_request");
        response.ErrorDescription.Should().Be("The request is malformed");
    }

    [Fact]
    public void AuthorizationDetailsRequest_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var authDetails = new AuthorizationDetailsRequest();

        // Assert
        authDetails.Type.Should().BeNull();
        authDetails.Locations.Should().BeNull();
        authDetails.Actions.Should().BeNull();
        authDetails.DataTypes.Should().BeNull();
        authDetails.Identifier.Should().BeNull();
        authDetails.Privileges.Should().BeNull();
    }

    [Fact]
    public void AuthorizationDetailsRequest_Properties_ShouldBeSettable()
    {
        // Arrange
        var authDetails = new AuthorizationDetailsRequest();
        var locations = new[] { "https://api.example.com", "https://service.example.com" };
        var actions = new[] { "read", "write", "delete" };
        var dataTypes = new[] { "credential", "presentation" };
        var privileges = new Dictionary<string, object>
        {
            ["scope"] = "openid profile",
            ["claims"] = new[] { "sub", "name", "email" }
        };

        // Act
        authDetails.Type = "openid_credential";
        authDetails.Locations = locations;
        authDetails.Actions = actions;
        authDetails.DataTypes = dataTypes;
        authDetails.Identifier = "credential-123";
        authDetails.Privileges = privileges;

        // Assert
        authDetails.Type.Should().Be("openid_credential");
        authDetails.Locations.Should().BeEquivalentTo(locations);
        authDetails.Actions.Should().BeEquivalentTo(actions);
        authDetails.DataTypes.Should().BeEquivalentTo(dataTypes);
        authDetails.Identifier.Should().Be("credential-123");
        authDetails.Privileges.Should().BeEquivalentTo(privileges);
    }

    [Fact]
    public void WalletData_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var walletData = new WalletData();

        // Assert
        walletData.WalletId.Should().BeNull();
        walletData.WalletName.Should().BeNull();
        walletData.WalletVersion.Should().BeNull();
        walletData.SupportedFormats.Should().BeNull();
        walletData.SupportedCryptographicSuites.Should().BeNull();
        walletData.Capabilities.Should().BeNull();
        walletData.Metadata.Should().BeNull();
    }

    [Fact]
    public void WalletData_Properties_ShouldBeSettable()
    {
        // Arrange
        var walletData = new WalletData();
        var supportedFormats = new[] { "jwt_vc", "vc+sd-jwt", "mso_mdoc" };
        var supportedSuites = new[] { "ES256", "ES384", "RS256" };
        var capabilities = new Dictionary<string, object>
        {
            ["selective_disclosure"] = true,
            ["zero_knowledge_proofs"] = false,
            ["batch_issuance"] = true
        };
        var metadata = new Dictionary<string, object>
        {
            ["attestation_url"] = "https://wallet.example.com/attestation",
            ["privacy_policy"] = "https://wallet.example.com/privacy"
        };

        // Act
        walletData.WalletId = "wallet-123";
        walletData.WalletName = "Example Wallet";
        walletData.WalletVersion = "1.0.0";
        walletData.SupportedFormats = supportedFormats;
        walletData.SupportedCryptographicSuites = supportedSuites;
        walletData.Capabilities = capabilities;
        walletData.Metadata = metadata;

        // Assert
        walletData.WalletId.Should().Be("wallet-123");
        walletData.WalletName.Should().Be("Example Wallet");
        walletData.WalletVersion.Should().Be("1.0.0");
        walletData.SupportedFormats.Should().BeEquivalentTo(supportedFormats);
        walletData.SupportedCryptographicSuites.Should().BeEquivalentTo(supportedSuites);
        walletData.Capabilities.Should().BeEquivalentTo(capabilities);
        walletData.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void HaipTypes_HaipRequestType_ShouldHaveExpectedValue()
    {
        // Assert
        HaipTypes.HaipRequestType.Should().Be("haip_request");
    }

    [Fact]
    public void HaipTypes_HaipResponseType_ShouldHaveExpectedValue()
    {
        // Assert
        HaipTypes.HaipResponseType.Should().Be("haip_response");
    }

    [Fact]
    public void HaipTypes_AuthorizationDetailsType_ShouldHaveExpectedValue()
    {
        // Assert
        HaipTypes.AuthorizationDetailsType.Should().Be("openid_credential");
    }

    [Fact]
    public void HaipRequest_WithCompleteData_ShouldSetAllProperties()
    {
        // Arrange
        var authDetails = new AuthorizationDetailsRequest
        {
            Type = HaipTypes.AuthorizationDetailsType,
            Locations = new[] { "https://issuer.example.com" },
            Actions = new[] { "issue" },
            DataTypes = new[] { "UniversityDegreeCredential" },
            Identifier = "degree-123",
            Privileges = new Dictionary<string, object>
            {
                ["format"] = "vc+sd-jwt",
                ["claims"] = new[] { "given_name", "family_name", "degree_type" }
            }
        };

        var walletData = new WalletData
        {
            WalletId = "wallet-abc",
            WalletName = "University Wallet",
            WalletVersion = "2.1.0",
            SupportedFormats = new[] { "vc+sd-jwt", "jwt_vc", "ldp_vc" },
            SupportedCryptographicSuites = new[] { "ES256", "EdDSA", "ES384" },
            Capabilities = new Dictionary<string, object>
            {
                ["selective_disclosure"] = true,
                ["zero_knowledge_proofs"] = true,
                ["batch_credentials"] = false
            }
        };

        // Act
        var request = new HaipRequest
        {
            Endpoint = "https://wallet.university.edu/haip",
            Nonce = "secure-nonce-xyz789",
            State = "secure-state-abc123",
            AuthorizationDetails = authDetails,
            WalletData = walletData
        };

        // Assert
        request.Endpoint.Should().Be("https://wallet.university.edu/haip");
        request.Nonce.Should().Be("secure-nonce-xyz789");
        request.State.Should().Be("secure-state-abc123");
        request.AuthorizationDetails.Should().Be(authDetails);
        request.WalletData.Should().Be(walletData);

        // Verify authorization details
        request.AuthorizationDetails!.Type.Should().Be(HaipTypes.AuthorizationDetailsType);
        request.AuthorizationDetails.Locations.Should().Contain("https://issuer.example.com");
        request.AuthorizationDetails.Actions.Should().Contain("issue");
        request.AuthorizationDetails.DataTypes.Should().Contain("UniversityDegreeCredential");

        // Verify wallet data
        request.WalletData!.WalletId.Should().Be("wallet-abc");
        request.WalletData.SupportedFormats.Should().Contain("vc+sd-jwt");
        request.WalletData.SupportedCryptographicSuites.Should().Contain("ES256");
        request.WalletData.Capabilities.Should().ContainKey("selective_disclosure");
        request.WalletData.Capabilities!["selective_disclosure"].Should().Be(true);
    }

    [Fact]
    public void HaipResponse_SuccessResponse_ShouldContainValidData()
    {
        // Arrange
        var successData = new Dictionary<string, object>
        {
            ["status"] = "success",
            ["credential"] = new Dictionary<string, object>
            {
                ["format"] = "vc+sd-jwt",
                ["credential"] = "eyJhbGciOiJFUzI1NiJ9...",
                ["c_nonce"] = "credential-nonce-123",
                ["c_nonce_expires_in"] = 300
            },
            ["metadata"] = new Dictionary<string, object>
            {
                ["issued_at"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["issuer"] = "https://university.example.com"
            }
        };

        // Act
        var response = new HaipResponse
        {
            ResponseData = successData,
            State = "secure-state-abc123"
        };

        // Assert
        response.ResponseData.Should().NotBeNull();
        response.ResponseData.Should().ContainKey("status");
        response.ResponseData!["status"].Should().Be("success");
        response.ResponseData.Should().ContainKey("credential");
        response.ResponseData.Should().ContainKey("metadata");
        response.State.Should().Be("secure-state-abc123");
        response.Error.Should().BeNull();
        response.ErrorDescription.Should().BeNull();
    }

    [Fact]
    public void HaipResponse_ErrorResponse_ShouldContainErrorInfo()
    {
        // Act
        var response = new HaipResponse
        {
            State = "secure-state-abc123",
            Error = "unsupported_credential_type",
            ErrorDescription = "The requested credential type is not supported by this wallet"
        };

        // Assert
        response.Error.Should().Be("unsupported_credential_type");
        response.ErrorDescription.Should().Be("The requested credential type is not supported by this wallet");
        response.State.Should().Be("secure-state-abc123");
        response.ResponseData.Should().BeNull();
    }

    [Fact]
    public void AuthorizationDetailsRequest_WithComplexPrivileges_ShouldHandleNestedData()
    {
        // Arrange
        var complexPrivileges = new Dictionary<string, object>
        {
            ["credential_definition"] = new Dictionary<string, object>
            {
                ["type"] = new[] { "VerifiableCredential", "UniversityDegreeCredential" },
                ["credentialSubject"] = new Dictionary<string, object>
                {
                    ["degree"] = new Dictionary<string, object>
                    {
                        ["name"] = "Bachelor of Science",
                        ["type"] = "BachelorDegree"
                    }
                }
            },
            ["proof_types_supported"] = new[] { "jwt", "ldp" },
            ["cryptographic_binding_methods_supported"] = new[] { "did", "jwk" }
        };

        // Act
        var authDetails = new AuthorizationDetailsRequest
        {
            Type = "openid_credential",
            Identifier = "university-degree-001",
            Privileges = complexPrivileges
        };

        // Assert
        authDetails.Privileges.Should().NotBeNull();
        authDetails.Privileges.Should().ContainKey("credential_definition");
        authDetails.Privileges.Should().ContainKey("proof_types_supported");
        authDetails.Privileges.Should().ContainKey("cryptographic_binding_methods_supported");

        var credentialDef = authDetails.Privileges!["credential_definition"] as Dictionary<string, object>;
        credentialDef.Should().ContainKey("type");
        credentialDef.Should().ContainKey("credentialSubject");

        var proofTypes = authDetails.Privileges["proof_types_supported"] as string[];
        proofTypes.Should().Contain("jwt");
        proofTypes.Should().Contain("ldp");
    }

    [Fact]
    public void WalletData_WithAdvancedCapabilities_ShouldSupportComplexFeatures()
    {
        // Arrange
        var advancedCapabilities = new Dictionary<string, object>
        {
            ["selective_disclosure"] = new Dictionary<string, object>
            {
                ["supported"] = true,
                ["algorithms"] = new[] { "sha-256", "sha-384" }
            },
            ["zero_knowledge_proofs"] = new Dictionary<string, object>
            {
                ["supported"] = true,
                ["proof_types"] = new[] { "bbs+", "cl-signatures" }
            },
            ["batch_operations"] = new Dictionary<string, object>
            {
                ["issuance"] = true,
                ["presentation"] = false,
                ["max_batch_size"] = 10
            },
            ["credential_formats"] = new Dictionary<string, object>
            {
                ["jwt_vc_json"] = new { alg = new[] { "ES256", "RS256" } },
                ["vc+sd-jwt"] = new { alg = new[] { "ES256", "EdDSA" } },
                ["ldp_vc"] = new { proof_type = new[] { "Ed25519Signature2018", "EcdsaSecp256k1Signature2019" } }
            }
        };

        var extendedMetadata = new Dictionary<string, object>
        {
            ["attestation"] = new Dictionary<string, object>
            {
                ["url"] = "https://wallet.example.com/attestation",
                ["type"] = "wallet_attestation",
                ["validity_period"] = 86400
            },
            ["compliance"] = new Dictionary<string, object>
            {
                ["eidas_compliant"] = true,
                ["fido_certified"] = false,
                ["privacy_preserving"] = true
            }
        };

        // Act
        var walletData = new WalletData
        {
            WalletId = "advanced-wallet-v2",
            WalletName = "Enterprise Wallet Pro",
            WalletVersion = "2.5.1",
            SupportedFormats = new[] { "jwt_vc_json", "vc+sd-jwt", "ldp_vc", "mso_mdoc" },
            SupportedCryptographicSuites = new[] { "ES256", "ES384", "ES512", "EdDSA", "RS256", "PS256" },
            Capabilities = advancedCapabilities,
            Metadata = extendedMetadata
        };

        // Assert
        walletData.Capabilities.Should().NotBeNull();
        walletData.Capabilities.Should().ContainKey("selective_disclosure");
        walletData.Capabilities.Should().ContainKey("zero_knowledge_proofs");
        walletData.Capabilities.Should().ContainKey("batch_operations");
        walletData.Capabilities.Should().ContainKey("credential_formats");

        walletData.Metadata.Should().NotBeNull();
        walletData.Metadata.Should().ContainKey("attestation");
        walletData.Metadata.Should().ContainKey("compliance");

        // Verify specific capability details
        var selectiveDisclosure = walletData.Capabilities!["selective_disclosure"] as Dictionary<string, object>;
        selectiveDisclosure.Should().ContainKey("supported");
        selectiveDisclosure!["supported"].Should().Be(true);

        var attestation = walletData.Metadata!["attestation"] as Dictionary<string, object>;
        attestation.Should().ContainKey("url");
        attestation!["url"].Should().Be("https://wallet.example.com/attestation");
    }

    [Fact]
    public void HaipTypes_AllConstants_ShouldHaveExpectedValues()
    {
        // Assert - Verify all HAIP type constants
        HaipTypes.HaipRequestType.Should().NotBeNullOrEmpty();
        HaipTypes.HaipResponseType.Should().NotBeNullOrEmpty();
        HaipTypes.AuthorizationDetailsType.Should().NotBeNullOrEmpty();

        // Verify the values match expected HAIP specification values
        HaipTypes.HaipRequestType.Should().Be("haip_request");
        HaipTypes.HaipResponseType.Should().Be("haip_response");
        HaipTypes.AuthorizationDetailsType.Should().Be("openid_credential");
    }
}
