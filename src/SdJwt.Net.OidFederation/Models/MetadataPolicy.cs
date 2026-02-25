using System.Text.Json.Serialization;

namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Represents metadata policy rules that can be applied to entity metadata.
/// Used in Entity Statements to constrain subordinate entities.
/// </summary>
public class MetadataPolicy {
        /// <summary>
        /// Gets or sets policy rules for federation entity metadata.
        /// Optional. Rules that apply to federation_entity metadata.
        /// </summary>
        [JsonPropertyName("federation_entity")]
        public MetadataPolicyRules? FederationEntity { get; set; }

        /// <summary>
        /// Gets or sets policy rules for OpenID Connect Relying Party metadata.
        /// Optional. Rules that apply to openid_relying_party metadata.
        /// </summary>
        [JsonPropertyName("openid_relying_party")]
        public MetadataPolicyRules? OpenIdRelyingParty { get; set; }

        /// <summary>
        /// Gets or sets policy rules for OpenID Connect Provider metadata.
        /// Optional. Rules that apply to openid_provider metadata.
        /// </summary>
        [JsonPropertyName("openid_provider")]
        public MetadataPolicyRules? OpenIdProvider { get; set; }

        /// <summary>
        /// Gets or sets policy rules for OAuth Authorization Server metadata.
        /// Optional. Rules that apply to oauth_authorization_server metadata.
        /// </summary>
        [JsonPropertyName("oauth_authorization_server")]
        public MetadataPolicyRules? OAuthAuthorizationServer { get; set; }

        /// <summary>
        /// Gets or sets policy rules for OID4VCI Credential Issuer metadata.
        /// Optional. Rules that apply to openid_credential_issuer metadata.
        /// </summary>
        [JsonPropertyName("openid_credential_issuer")]
        public MetadataPolicyRules? OpenIdCredentialIssuer { get; set; }

        /// <summary>
        /// Gets or sets policy rules for OID4VP Verifier metadata.
        /// Optional. Rules that apply to openid_relying_party_verifier metadata.
        /// </summary>
        [JsonPropertyName("openid_relying_party_verifier")]
        public MetadataPolicyRules? OpenIdRelyingPartyVerifier { get; set; }

        /// <summary>
        /// Gets or sets additional policy rules for other protocols.
        /// Allows extension for future or custom protocols.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, MetadataPolicyRules>? AdditionalPolicies { get; set; }

        /// <summary>
        /// Validates the metadata policy.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the policy is invalid</exception>
        public void Validate() {
                FederationEntity?.Validate();
                OpenIdRelyingParty?.Validate();
                OpenIdProvider?.Validate();
                OAuthAuthorizationServer?.Validate();
                OpenIdCredentialIssuer?.Validate();
                OpenIdRelyingPartyVerifier?.Validate();

                if (AdditionalPolicies != null) {
                        foreach (var policy in AdditionalPolicies.Values) {
                                policy?.Validate();
                        }
                }
        }

        /// <summary>
        /// Gets the policy rules for a specific protocol.
        /// </summary>
        /// <param name="protocol">The protocol identifier</param>
        /// <returns>The policy rules or null if not found</returns>
        public MetadataPolicyRules? GetPolicyRules(string protocol) => protocol switch {
                "federation_entity" => FederationEntity,
                "openid_relying_party" => OpenIdRelyingParty,
                "openid_provider" => OpenIdProvider,
                "oauth_authorization_server" => OAuthAuthorizationServer,
                "openid_credential_issuer" => OpenIdCredentialIssuer,
                "openid_relying_party_verifier" => OpenIdRelyingPartyVerifier,
                _ => AdditionalPolicies?.GetValueOrDefault(protocol)
        };
}

/// <summary>
/// Represents policy rules for a specific metadata type.
/// Contains operators that define how metadata values should be constrained.
/// </summary>
public class MetadataPolicyRules {
        /// <summary>
        /// Gets or sets field-specific policy operators.
        /// Key is the field name, value is the policy operators for that field.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? FieldPolicies { get; set; }

        /// <summary>
        /// Validates the metadata policy rules.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the rules are invalid</exception>
        public void Validate() {
                if (FieldPolicies != null) {
                        foreach (var field in FieldPolicies.Keys) {
                                if (string.IsNullOrWhiteSpace(field))
                                        throw new InvalidOperationException("Field name cannot be null or empty");
                        }
                }
        }

        /// <summary>
        /// Gets the policy operators for a specific field.
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The policy operators or null if not found</returns>
        public object? GetFieldPolicy(string fieldName) {
                return FieldPolicies?.GetValueOrDefault(fieldName);
        }

        /// <summary>
        /// Sets policy operators for a specific field.
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <param name="operators">The policy operators</param>
        public void SetFieldPolicy(string fieldName, object operators) {
                FieldPolicies ??= new Dictionary<string, object>();
                FieldPolicies[fieldName] = operators;
        }
}

/// <summary>
/// Common metadata policy operators as defined in OpenID Federation 1.0.
/// </summary>
public static class PolicyOperators {
        /// <summary>
        /// Value operator - sets the exact value.
        /// </summary>
        public const string Value = "value";

        /// <summary>
        /// Add operator - adds values to an array.
        /// </summary>
        public const string Add = "add";

        /// <summary>
        /// Default operator - sets default value if not present.
        /// </summary>
        public const string Default = "default";

        /// <summary>
        /// Essential operator - marks a parameter as essential.
        /// </summary>
        public const string Essential = "essential";

        /// <summary>
        /// One of operator - restricts to one of the specified values.
        /// </summary>
        public const string OneOf = "one_of";

        /// <summary>
        /// Subset of operator - restricts to a subset of specified values.
        /// </summary>
        public const string SubsetOf = "subset_of";

        /// <summary>
        /// Superset of operator - requires a superset of specified values.
        /// </summary>
        public const string SupersetOf = "superset_of";

        /// <summary>
        /// Creates a value policy operator.
        /// </summary>
        /// <param name="value">The required value</param>
        /// <returns>Policy operator object</returns>
        public static Dictionary<string, object> CreateValue(object value) {
                return new Dictionary<string, object> { { Value, value } };
        }

        /// <summary>
        /// Creates an essential policy operator.
        /// </summary>
        /// <param name="isEssential">Whether the parameter is essential</param>
        /// <returns>Policy operator object</returns>
        public static Dictionary<string, object> CreateEssential(bool isEssential = true) {
                return new Dictionary<string, object> { { Essential, isEssential } };
        }

        /// <summary>
        /// Creates a one-of policy operator.
        /// </summary>
        /// <param name="allowedValues">Array of allowed values</param>
        /// <returns>Policy operator object</returns>
        public static Dictionary<string, object> CreateOneOf(params object[] allowedValues) {
                return new Dictionary<string, object> { { OneOf, allowedValues } };
        }

        /// <summary>
        /// Creates a subset-of policy operator.
        /// </summary>
        /// <param name="allowedValues">Array of allowed values</param>
        /// <returns>Policy operator object</returns>
        public static Dictionary<string, object> CreateSubsetOf(params object[] allowedValues) {
                return new Dictionary<string, object> { { SubsetOf, allowedValues } };
        }
}