using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Coverage;

/// <summary>
/// Tests for <see cref="VerifierMetadata"/> class and <see cref="Oid4VpConstants"/>,
/// covering property access, factory methods, and JSON serialization.
/// </summary>
public class VerifierMetadataConstantsTests
{
    #region VerifierMetadata Tests

    /// <summary>
    /// Tests that CreateForSdJwtVc() returns instance with expected formats.
    /// </summary>
    [Fact]
    public void VerifierMetadata_CreateForSdJwtVc_ReturnsConfiguredInstance()
    {
        // Act
        var result = VerifierMetadata.CreateForSdJwtVc();

        // Assert
        result.Should().NotBeNull();
        result.VpFormatsSupported.Should().NotBeNull();
        result.VpFormatsSupported.Should().ContainKey("vc+sd-jwt");
    }

    /// <summary>
    /// Tests that CreateForSdJwtVc() contains proper algorithm values.
    /// </summary>
    [Fact]
    public void VerifierMetadata_CreateForSdJwtVc_ContainsAlgorithmValues()
    {
        // Act
        var result = VerifierMetadata.CreateForSdJwtVc();

        // Assert
        var vcFormat = result.VpFormatsSupported!["vc+sd-jwt"] as Dictionary<string, object>;
        vcFormat.Should().NotBeNull();
        vcFormat.Should().ContainKey("sd-jwt_alg_values");
        vcFormat.Should().ContainKey("kb-jwt_alg_values");
    }

    /// <summary>
    /// Tests setting VpFormatsSupported property.
    /// </summary>
    [Fact]
    public void VerifierMetadata_VpFormatsSupported_CanBeSetAndRetrieved()
    {
        // Arrange
        var metadata = new VerifierMetadata();
        var formats = new Dictionary<string, object>
        {
            ["jwt_vp"] = new { alg_values = new[] { "ES256" } }
        };

        // Act
        metadata.VpFormatsSupported = formats;

        // Assert
        metadata.VpFormatsSupported.Should().BeSameAs(formats);
    }

    /// <summary>
    /// Tests setting AuthorizationEncryptedResponseAlg property.
    /// </summary>
    [Fact]
    public void VerifierMetadata_AuthorizationEncryptedResponseAlg_CanBeSetAndRetrieved()
    {
        // Arrange
        var metadata = new VerifierMetadata();

        // Act
        metadata.AuthorizationEncryptedResponseAlg = "ECDH-ES";

        // Assert
        metadata.AuthorizationEncryptedResponseAlg.Should().Be("ECDH-ES");
    }

    /// <summary>
    /// Tests setting AuthorizationEncryptedResponseEnc property.
    /// </summary>
    [Fact]
    public void VerifierMetadata_AuthorizationEncryptedResponseEnc_CanBeSetAndRetrieved()
    {
        // Arrange
        var metadata = new VerifierMetadata();

        // Act
        metadata.AuthorizationEncryptedResponseEnc = "A256GCM";

        // Assert
        metadata.AuthorizationEncryptedResponseEnc.Should().Be("A256GCM");
    }

    /// <summary>
    /// Tests JSON serialization of VerifierMetadata.
    /// </summary>
    [Fact]
    public void VerifierMetadata_JsonSerialization_RoundTripsCorrectly()
    {
        // Arrange
        var metadata = VerifierMetadata.CreateForSdJwtVc();
        metadata.AuthorizationEncryptedResponseAlg = "ECDH-ES";
        metadata.AuthorizationEncryptedResponseEnc = "A256GCM";

        // Act
        var json = JsonSerializer.Serialize(metadata);
        var deserialized = JsonSerializer.Deserialize<VerifierMetadata>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.VpFormatsSupported.Should().ContainKey("vc+sd-jwt");
        deserialized.AuthorizationEncryptedResponseAlg.Should().Be("ECDH-ES");
        deserialized.AuthorizationEncryptedResponseEnc.Should().Be("A256GCM");
    }

    /// <summary>
    /// Tests JSON serialization omits null properties.
    /// </summary>
    [Fact]
    public void VerifierMetadata_JsonSerialization_OmitsNullProperties()
    {
        // Arrange
        var metadata = new VerifierMetadata();

        // Act
        var json = JsonSerializer.Serialize(metadata);

        // Assert - Optional properties should be omitted when null
        json.Should().NotContain("authorization_encrypted_response_alg");
        json.Should().NotContain("authorization_encrypted_response_enc");
    }

    /// <summary>
    /// Tests default constructor creates instance with null properties.
    /// </summary>
    [Fact]
    public void VerifierMetadata_DefaultConstructor_CreatesInstanceWithNullProperties()
    {
        // Act
        var metadata = new VerifierMetadata();

        // Assert
        metadata.VpFormatsSupported.Should().BeNull();
        metadata.AuthorizationEncryptedResponseAlg.Should().BeNull();
        metadata.AuthorizationEncryptedResponseEnc.Should().BeNull();
    }

    #endregion

    #region Oid4VpConstants Tests

    /// <summary>
    /// Tests that AuthorizationRequestScheme constant has expected value.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_AuthorizationRequestScheme_HasExpectedValue()
    {
        Oid4VpConstants.AuthorizationRequestScheme.Should().Be("openid4vp");
    }

    /// <summary>
    /// Tests that SdJwtVcFormat constant has expected value.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_SdJwtVcFormat_HasExpectedValue()
    {
        Oid4VpConstants.SdJwtVcFormat.Should().Be("dc+sd-jwt");
    }

    /// <summary>
    /// Tests that SdJwtVcLegacyFormat constant has expected value.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_SdJwtVcLegacyFormat_HasExpectedValue()
    {
        Oid4VpConstants.SdJwtVcLegacyFormat.Should().Be("vc+sd-jwt");
    }

    /// <summary>
    /// Tests that KeyBindingJwtType constant has expected value.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_KeyBindingJwtType_HasExpectedValue()
    {
        Oid4VpConstants.KeyBindingJwtType.Should().Be("kb+jwt");
    }

    /// <summary>
    /// Tests that AuthorizationRequestJwtType constant has expected value.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_AuthorizationRequestJwtType_HasExpectedValue()
    {
        Oid4VpConstants.AuthorizationRequestJwtType.Should().Be("oauth-authz-req+jwt");
    }

    /// <summary>
    /// Tests ResponseModes constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_ResponseModes_HaveExpectedValues()
    {
        Oid4VpConstants.ResponseModes.DirectPost.Should().Be("direct_post");
        Oid4VpConstants.ResponseModes.DirectPostJwt.Should().Be("direct_post.jwt");
        Oid4VpConstants.ResponseModes.Fragment.Should().Be("fragment");
        Oid4VpConstants.ResponseModes.Query.Should().Be("query");
    }

    /// <summary>
    /// Tests ResponseTypes constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_ResponseTypes_HaveExpectedValues()
    {
        Oid4VpConstants.ResponseTypes.VpToken.Should().Be("vp_token");
    }

    /// <summary>
    /// Tests ClientIdSchemes constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_ClientIdSchemes_HaveExpectedValues()
    {
        Oid4VpConstants.ClientIdSchemes.RedirectUri.Should().Be("redirect_uri");
        Oid4VpConstants.ClientIdSchemes.EntityId.Should().Be("entity_id");
        Oid4VpConstants.ClientIdSchemes.Did.Should().Be("did");
        Oid4VpConstants.ClientIdSchemes.Web.Should().Be("web");
        Oid4VpConstants.ClientIdSchemes.X509SanDns.Should().Be("x509_san_dns");
        Oid4VpConstants.ClientIdSchemes.X509SanUri.Should().Be("x509_san_uri");
        Oid4VpConstants.ClientIdSchemes.VerifierAttestation.Should().Be("verifier_attestation");
        Oid4VpConstants.ClientIdSchemes.PreRegistered.Should().Be("pre-registered");
    }

    /// <summary>
    /// Tests PresentationExchange constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_PresentationExchange_HaveExpectedValues()
    {
        Oid4VpConstants.PresentationExchange.Version.Should().Be("2.0.0");
        Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.All.Should().Be("all");
        Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick.Should().Be("pick");
    }

    /// <summary>
    /// Tests FilterTypes constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_FilterTypes_HaveExpectedValues()
    {
        Oid4VpConstants.PresentationExchange.FilterTypes.String.Should().Be("string");
        Oid4VpConstants.PresentationExchange.FilterTypes.Number.Should().Be("number");
        Oid4VpConstants.PresentationExchange.FilterTypes.Array.Should().Be("array");
        Oid4VpConstants.PresentationExchange.FilterTypes.Object.Should().Be("object");
        Oid4VpConstants.PresentationExchange.FilterTypes.Boolean.Should().Be("boolean");
    }

    /// <summary>
    /// Tests PathNestedProperties constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_PathNestedProperties_HaveExpectedValues()
    {
        Oid4VpConstants.PresentationExchange.PathNestedProperties.Format.Should().Be("format");
        Oid4VpConstants.PresentationExchange.PathNestedProperties.Path.Should().Be("path");
        Oid4VpConstants.PresentationExchange.PathNestedProperties.PathNested.Should().Be("path_nested");
    }

    /// <summary>
    /// Tests ErrorCodes constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_ErrorCodes_HaveExpectedValues()
    {
        Oid4VpConstants.ErrorCodes.InvalidRequest.Should().Be("invalid_request");
        Oid4VpConstants.ErrorCodes.UnauthorizedClient.Should().Be("unauthorized_client");
        Oid4VpConstants.ErrorCodes.AccessDenied.Should().Be("access_denied");
        Oid4VpConstants.ErrorCodes.UnsupportedResponseType.Should().Be("unsupported_response_type");
        Oid4VpConstants.ErrorCodes.InvalidScope.Should().Be("invalid_scope");
        Oid4VpConstants.ErrorCodes.ServerError.Should().Be("server_error");
        Oid4VpConstants.ErrorCodes.TemporarilyUnavailable.Should().Be("temporarily_unavailable");
        Oid4VpConstants.ErrorCodes.VpFormatsNotSupported.Should().Be("vp_formats_not_supported");
        Oid4VpConstants.ErrorCodes.InvalidPresentationDefinitionUri.Should().Be("invalid_presentation_definition_uri");
        Oid4VpConstants.ErrorCodes.InvalidPresentationDefinitionObject.Should().Be("invalid_presentation_definition_object");
    }

    /// <summary>
    /// Tests MediaTypes constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_MediaTypes_HaveExpectedValues()
    {
        Oid4VpConstants.MediaTypes.SdJwt.Should().Be("application/sd-jwt");
        Oid4VpConstants.MediaTypes.KeyBindingJwt.Should().Be("application/kb+jwt");
        Oid4VpConstants.MediaTypes.VpToken.Should().Be("application/vp-token");
    }

    /// <summary>
    /// Tests JsonPaths constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_JsonPaths_HaveExpectedValues()
    {
        Oid4VpConstants.JsonPaths.CredentialType.Should().Be("$.vct");
        Oid4VpConstants.JsonPaths.CredentialSubject.Should().Be("$.credentialSubject");
        Oid4VpConstants.JsonPaths.Issuer.Should().Be("$.iss");
        Oid4VpConstants.JsonPaths.Subject.Should().Be("$.sub");
        Oid4VpConstants.JsonPaths.ExpirationTime.Should().Be("$.exp");
        Oid4VpConstants.JsonPaths.IssuedAt.Should().Be("$.iat");
        Oid4VpConstants.JsonPaths.NotBefore.Should().Be("$.nbf");
    }

    /// <summary>
    /// Tests RequestUriMethods constants.
    /// </summary>
    [Fact]
    public void Oid4VpConstants_RequestUriMethods_HaveExpectedValues()
    {
        Oid4VpConstants.RequestUriMethods.Get.Should().Be("get");
        Oid4VpConstants.RequestUriMethods.Post.Should().Be("post");
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests creating a complete presentation submission with nested descriptors.
    /// </summary>
    [Fact]
    public void Integration_CreateComplexPresentationSubmission_Succeeds()
    {
        // Arrange & Act
        var submission = PresentationSubmission.CreateMultiple(
            "submission-123",
            "definition-456",
            InputDescriptorMapping.Create("identity-credential", Oid4VpConstants.SdJwtVcFormat, "$[0]")
                .WithPathNested(PathNestedDescriptor.Create("$.vc", "jwt_vc")
                    .WithPathNested(PathNestedDescriptor.Create("$.credentialSubject"))),
            InputDescriptorMapping.CreateForSdJwt("driver-license", 1),
            InputDescriptorMapping.Create("insurance", Oid4VpConstants.SdJwtVcLegacyFormat, "$[2]")
        );

        // Assert
        submission.Validate();  // Should not throw
        submission.DescriptorMap.Should().HaveCount(3);
        submission.DescriptorMap[0].PathNested.Should().NotBeNull();
        submission.DescriptorMap[0].PathNested!.PathNested.Should().NotBeNull();
    }

    /// <summary>
    /// Tests full JSON serialization and deserialization workflow.
    /// </summary>
    [Fact]
    public void Integration_FullJsonWorkflow_Succeeds()
    {
        // Arrange
        var original = PresentationSubmission.CreateSingle(
            "sub-1", "def-1", "desc-1", Oid4VpConstants.SdJwtVcFormat);
        original.DescriptorMap[0].PathNested = PathNestedDescriptor.Create("$.vc")
            .WithPathNested(PathNestedDescriptor.Create("$.claims"));

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<PresentationSubmission>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Validate();  // Should not throw
        deserialized.DescriptorMap[0].PathNested.Should().NotBeNull();
        deserialized.DescriptorMap[0].PathNested!.PathNested.Should().NotBeNull();
        deserialized.DescriptorMap[0].PathNested!.PathNested!.Path.Should().Be("$.claims");
    }

    /// <summary>
    /// Tests fluent builder pattern with method chaining.
    /// </summary>
    [Fact]
    public void Integration_FluentBuilder_Succeeds()
    {
        // Arrange & Act
        var submission = new PresentationSubmission
        {
            Id = "sub-1",
            DefinitionId = "def-1",
            DescriptorMap = Array.Empty<InputDescriptorMapping>()
        }
        .WithMapping(InputDescriptorMapping.CreateForSdJwt("cred-1", 0)
            .WithPathNested(PathNestedDescriptor.Create("$.vc")))
        .WithMapping(InputDescriptorMapping.CreateForSdJwt("cred-2", 1));

        // Assert
        submission.Validate();  // Should not throw
        submission.DescriptorMap.Should().HaveCount(2);
    }

    #endregion
}
