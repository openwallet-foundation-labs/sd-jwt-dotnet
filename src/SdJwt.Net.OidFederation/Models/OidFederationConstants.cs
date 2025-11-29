namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Contains constants defined by the OpenID Federation 1.0 specification.
/// </summary>
public static class OidFederationConstants
{
    /// <summary>
    /// Well-known endpoints as defined in OpenID Federation 1.0.
    /// </summary>
    public static class WellKnownEndpoints
    {
        /// <summary>
        /// The well-known path for entity configuration.
        /// Every federation entity must publish their configuration at this path.
        /// </summary>
        public const string EntityConfiguration = "/.well-known/openid-federation";
    }

    /// <summary>
    /// Federation-specific endpoints.
    /// </summary>
    public static class FederationEndpoints
    {
        /// <summary>
        /// Endpoint for fetching entity statements about subordinates.
        /// </summary>
        public const string FederationFetch = "federation_fetch_endpoint";

        /// <summary>
        /// Endpoint for listing subordinate entities.
        /// </summary>
        public const string FederationList = "federation_list_endpoint";

        /// <summary>
        /// Endpoint for resolving entity metadata through the federation.
        /// </summary>
        public const string FederationResolve = "federation_resolve_endpoint";

        /// <summary>
        /// Endpoint for checking trust mark status.
        /// </summary>
        public const string TrustMarkStatus = "federation_trust_mark_status_endpoint";
    }

    /// <summary>
    /// JWT header parameters specific to OpenID Federation.
    /// </summary>
    public static class JwtHeaders
    {
        /// <summary>
        /// The typ header value for entity configuration JWTs.
        /// </summary>
        public const string EntityConfigurationType = "entity-configuration+jwt";

        /// <summary>
        /// The typ header value for entity statement JWTs.
        /// </summary>
        public const string EntityStatementType = "entity-statement+jwt";

        /// <summary>
        /// The typ header value for trust mark JWTs.
        /// </summary>
        public const string TrustMarkType = "trust-mark+jwt";
    }

    /// <summary>
    /// Supported JWT signing algorithms for federation entities.
    /// </summary>
    public static class SigningAlgorithms
    {
        /// <summary>
        /// ECDSA with SHA-256 (recommended for federation).
        /// </summary>
        public const string ES256 = "ES256";

        /// <summary>
        /// ECDSA with SHA-384.
        /// </summary>
        public const string ES384 = "ES384";

        /// <summary>
        /// ECDSA with SHA-512.
        /// </summary>
        public const string ES512 = "ES512";

        /// <summary>
        /// RSA with SHA-256.
        /// </summary>
        public const string RS256 = "RS256";

        /// <summary>
        /// RSA with SHA-384.
        /// </summary>
        public const string RS384 = "RS384";

        /// <summary>
        /// RSA with SHA-512.
        /// </summary>
        public const string RS512 = "RS512";

        /// <summary>
        /// RSA-PSS with SHA-256.
        /// </summary>
        public const string PS256 = "PS256";

        /// <summary>
        /// RSA-PSS with SHA-384.
        /// </summary>
        public const string PS384 = "PS384";

        /// <summary>
        /// RSA-PSS with SHA-512.
        /// </summary>
        public const string PS512 = "PS512";

        /// <summary>
        /// Gets all supported signing algorithms.
        /// </summary>
        public static readonly string[] All = { ES256, ES384, ES512, RS256, RS384, RS512, PS256, PS384, PS512 };

        /// <summary>
        /// Gets the recommended signing algorithms for federation use.
        /// </summary>
        public static readonly string[] Recommended = { ES256, RS256 };
    }

    /// <summary>
    /// Entity types in OpenID Federation.
    /// </summary>
    public static class EntityTypes
    {
        /// <summary>
        /// Trust Anchor - the root of trust in a federation.
        /// </summary>
        public const string TrustAnchor = "trust_anchor";

        /// <summary>
        /// Intermediate Authority - an intermediate entity in the trust chain.
        /// </summary>
        public const string IntermediateAuthority = "intermediate_authority";

        /// <summary>
        /// Leaf Entity - end entities that participate in protocols.
        /// </summary>
        public const string LeafEntity = "leaf_entity";
    }

    /// <summary>
    /// HTTP query parameters used in federation requests.
    /// </summary>
    public static class QueryParameters
    {
        /// <summary>
        /// The entity identifier parameter.
        /// Used to specify which entity to fetch information about.
        /// </summary>
        public const string EntityId = "entity_id";

        /// <summary>
        /// The issuer parameter.
        /// Used to specify the issuer of an entity statement.
        /// </summary>
        public const string Issuer = "iss";

        /// <summary>
        /// The subject parameter.
        /// Used to specify the subject of an entity statement.
        /// </summary>
        public const string Subject = "sub";

        /// <summary>
        /// The trust mark identifier parameter.
        /// </summary>
        public const string TrustMarkId = "trust_mark_id";
    }

    /// <summary>
    /// HTTP content types used in federation communication.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// Content type for entity configuration JWTs.
        /// </summary>
        public const string EntityConfiguration = "application/entity-configuration+jwt";

        /// <summary>
        /// Content type for entity statement JWTs.
        /// </summary>
        public const string EntityStatement = "application/entity-statement+jwt";

        /// <summary>
        /// Content type for trust mark JWTs.
        /// </summary>
        public const string TrustMark = "application/trust-mark+jwt";

        /// <summary>
        /// Content type for JSON responses.
        /// </summary>
        public const string Json = "application/json";
    }

    /// <summary>
    /// Error codes specific to OpenID Federation.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Invalid entity identifier.
        /// </summary>
        public const string InvalidEntityId = "invalid_entity_id";

        /// <summary>
        /// Entity not found.
        /// </summary>
        public const string EntityNotFound = "entity_not_found";

        /// <summary>
        /// Invalid trust chain.
        /// </summary>
        public const string InvalidTrustChain = "invalid_trust_chain";

        /// <summary>
        /// Trust chain resolution failed.
        /// </summary>
        public const string TrustChainResolutionFailed = "trust_chain_resolution_failed";

        /// <summary>
        /// Invalid entity statement.
        /// </summary>
        public const string InvalidEntityStatement = "invalid_entity_statement";

        /// <summary>
        /// Invalid entity configuration.
        /// </summary>
        public const string InvalidEntityConfiguration = "invalid_entity_configuration";

        /// <summary>
        /// Trust mark validation failed.
        /// </summary>
        public const string TrustMarkValidationFailed = "trust_mark_validation_failed";

        /// <summary>
        /// Metadata policy violation.
        /// </summary>
        public const string MetadataPolicyViolation = "metadata_policy_violation";
    }

    /// <summary>
    /// Default values and limits.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default maximum path length for trust chains.
        /// </summary>
        public const int MaxPathLength = 10;

        /// <summary>
        /// Default entity configuration validity period in hours.
        /// </summary>
        public const int DefaultValidityHours = 24;

        /// <summary>
        /// Maximum allowed validity period in hours.
        /// </summary>
        public const int MaxValidityHours = 8760; // 1 year

        /// <summary>
        /// HTTP client timeout for federation requests in seconds.
        /// </summary>
        public const int HttpTimeoutSeconds = 30;

        /// <summary>
        /// Maximum number of redirects to follow.
        /// </summary>
        public const int MaxRedirects = 3;
    }

    /// <summary>
    /// Cache-related constants.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Default cache duration for entity configurations in minutes.
        /// </summary>
        public const int DefaultCacheDurationMinutes = 60;

        /// <summary>
        /// Maximum cache duration in minutes.
        /// </summary>
        public const int MaxCacheDurationMinutes = 1440; // 24 hours

        /// <summary>
        /// Cache key prefix for entity configurations.
        /// </summary>
        public const string EntityConfigurationPrefix = "federation:entity-config:";

        /// <summary>
        /// Cache key prefix for entity statements.
        /// </summary>
        public const string EntityStatementPrefix = "federation:entity-statement:";

        /// <summary>
        /// Cache key prefix for trust chains.
        /// </summary>
        public const string TrustChainPrefix = "federation:trust-chain:";
    }
}