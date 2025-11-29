namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Constants defined by the DIF Presentation Exchange 2.1.1 specification.
/// </summary>
public static class PresentationExchangeConstants
{
    /// <summary>
    /// Presentation Exchange specification version.
    /// </summary>
    public const string SpecVersion = "https://identity.foundation/presentation-exchange/spec/v2.1.1/";

    /// <summary>
    /// Input descriptor format identifiers.
    /// </summary>
    public static class Formats
    {
        /// <summary>
        /// JSON Web Token format.
        /// </summary>
        public const string Jwt = "jwt";

        /// <summary>
        /// JWT Verifiable Credential format.
        /// </summary>
        public const string JwtVc = "jwt_vc";

        /// <summary>
        /// JWT Verifiable Presentation format.
        /// </summary>
        public const string JwtVp = "jwt_vp";

        /// <summary>
        /// SD-JWT format.
        /// </summary>
        public const string SdJwt = "sd-jwt";

        /// <summary>
        /// SD-JWT Verifiable Credential format.
        /// </summary>
        public const string SdJwtVc = "vc+sd-jwt";

        /// <summary>
        /// Linked Data Proof format.
        /// </summary>
        public const string Ldp = "ldp";

        /// <summary>
        /// Linked Data Verifiable Credential format.
        /// </summary>
        public const string LdpVc = "ldp_vc";

        /// <summary>
        /// Linked Data Verifiable Presentation format.
        /// </summary>
        public const string LdpVp = "ldp_vp";

        /// <summary>
        /// All supported formats.
        /// </summary>
        public static readonly string[] All = { Jwt, JwtVc, JwtVp, SdJwt, SdJwtVc, Ldp, LdpVc, LdpVp };
    }

    /// <summary>
    /// Constraint operators for field validation.
    /// </summary>
    public static class Operators
    {
        /// <summary>
        /// The field value must be equal to the specified value.
        /// </summary>
        public const string Equal = "eq";

        /// <summary>
        /// The field value must not be equal to the specified value.
        /// </summary>
        public const string NotEqual = "ne";

        /// <summary>
        /// The field value must be less than the specified value.
        /// </summary>
        public const string LessThan = "lt";

        /// <summary>
        /// The field value must be less than or equal to the specified value.
        /// </summary>
        public const string LessThanOrEqual = "le";

        /// <summary>
        /// The field value must be greater than the specified value.
        /// </summary>
        public const string GreaterThan = "gt";

        /// <summary>
        /// The field value must be greater than or equal to the specified value.
        /// </summary>
        public const string GreaterThanOrEqual = "ge";

        /// <summary>
        /// The field value must be one of the specified values.
        /// </summary>
        public const string In = "in";

        /// <summary>
        /// The field value must not be one of the specified values.
        /// </summary>
        public const string NotIn = "not_in";

        /// <summary>
        /// The field value must contain the specified substring.
        /// </summary>
        public const string Contains = "contains";

        /// <summary>
        /// The field value must start with the specified string.
        /// </summary>
        public const string StartsWith = "starts_with";

        /// <summary>
        /// The field value must end with the specified string.
        /// </summary>
        public const string EndsWith = "ends_with";

        /// <summary>
        /// The field value must match the specified regular expression.
        /// </summary>
        public const string Matches = "matches";

        /// <summary>
        /// The field must exist (not null/undefined).
        /// </summary>
        public const string Exists = "exists";

        /// <summary>
        /// The field must be of the specified type.
        /// </summary>
        public const string Type = "type";

        /// <summary>
        /// All supported operators.
        /// </summary>
        public static readonly string[] All = 
        {
            Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
            In, NotIn, Contains, StartsWith, EndsWith, Matches, Exists, Type
        };
    }

    /// <summary>
    /// Submission requirement rules.
    /// </summary>
    public static class SubmissionRules
    {
        /// <summary>
        /// All input descriptors in the group must be satisfied.
        /// </summary>
        public const string All = "all";

        /// <summary>
        /// Only one input descriptor in the group needs to be satisfied.
        /// </summary>
        public const string Pick = "pick";
    }

    /// <summary>
    /// Error codes for presentation exchange.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// No matching credentials found.
        /// </summary>
        public const string NoMatchingCredentials = "no_matching_credentials";

        /// <summary>
        /// Invalid presentation definition.
        /// </summary>
        public const string InvalidPresentationDefinition = "invalid_presentation_definition";

        /// <summary>
        /// Constraint evaluation failed.
        /// </summary>
        public const string ConstraintEvaluationFailed = "constraint_evaluation_failed";

        /// <summary>
        /// JSON path evaluation failed.
        /// </summary>
        public const string JsonPathEvaluationFailed = "jsonpath_evaluation_failed";

        /// <summary>
        /// Submission requirement not satisfied.
        /// </summary>
        public const string SubmissionRequirementNotSatisfied = "submission_requirement_not_satisfied";
    }

    /// <summary>
    /// Default values and limits.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default maximum number of credentials to process.
        /// </summary>
        public const int MaxCredentials = 1000;

        /// <summary>
        /// Default timeout for constraint evaluation in milliseconds.
        /// </summary>
        public const int ConstraintEvaluationTimeoutMs = 5000;

        /// <summary>
        /// Maximum depth for recursive constraint evaluation.
        /// </summary>
        public const int MaxConstraintDepth = 10;
    }

    /// <summary>
    /// Common JSON paths used in credential selection.
    /// </summary>
    public static class CommonJsonPaths
    {
        /// <summary>
        /// Path to the credential type.
        /// </summary>
        public const string CredentialType = "$.type";

        /// <summary>
        /// Path to the credential issuer.
        /// </summary>
        public const string Issuer = "$.iss";

        /// <summary>
        /// Path to the credential subject.
        /// </summary>
        public const string Subject = "$.sub";

        /// <summary>
        /// Path to the verifiable credential type.
        /// </summary>
        public const string VcType = "$.vc.type";

        /// <summary>
        /// Path to the verifiable credential subject.
        /// </summary>
        public const string VcCredentialSubject = "$.vc.credentialSubject";

        /// <summary>
        /// Path to SD-JWT verifiable credential type.
        /// </summary>
        public const string VctType = "$.vct";

        /// <summary>
        /// Path to issuance date.
        /// </summary>
        public const string IssuanceDate = "$.iat";

        /// <summary>
        /// Path to expiration date.
        /// </summary>
        public const string ExpirationDate = "$.exp";
    }
}