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

        /// <summary>
        /// Path to credential status information.
        /// </summary>
        public const string Status = "$.status";

        /// <summary>
        /// Path to age field for age verification predicates.
        /// </summary>
        public const string Age = "$.age";

        /// <summary>
        /// Path to birth date field for age calculation predicates.
        /// </summary>
        public const string BirthDate = "$.birthdate";

        /// <summary>
        /// Path to income field for income verification predicates.
        /// </summary>
        public const string Income = "$.income";

        /// <summary>
        /// Path to salary field for salary verification predicates.
        /// </summary>
        public const string Salary = "$.salary";

        /// <summary>
        /// Path to citizenship field for citizenship verification predicates.
        /// </summary>
        public const string Citizenship = "$.citizenship";

        /// <summary>
        /// Path to nationality field for nationality verification predicates.
        /// </summary>
        public const string Nationality = "$.nationality";

        /// <summary>
        /// Path to credit score field for credit verification predicates.
        /// </summary>
        public const string CreditScore = "$.credit_score";
    }

    /// <summary>
    /// Predicate types for zero-knowledge and privacy-preserving constraints.
    /// </summary>
    public static class PredicateTypes
    {
        /// <summary>
        /// Age verification predicate - proves age is over a threshold without revealing exact age.
        /// </summary>
        public const string AgeOver = "age_over";

        /// <summary>
        /// Greater than comparison predicate.
        /// </summary>
        public const string GreaterThan = "greater_than";

        /// <summary>
        /// Less than comparison predicate.
        /// </summary>
        public const string LessThan = "less_than";

        /// <summary>
        /// Greater than or equal comparison predicate.
        /// </summary>
        public const string GreaterThanOrEqual = "greater_than_or_equal";

        /// <summary>
        /// Less than or equal comparison predicate.
        /// </summary>
        public const string LessThanOrEqual = "less_than_or_equal";

        /// <summary>
        /// Equality predicate.
        /// </summary>
        public const string EqualTo = "equal_to";

        /// <summary>
        /// Inequality predicate.
        /// </summary>
        public const string NotEqualTo = "not_equal_to";

        /// <summary>
        /// Range membership predicate - proves value is within a range.
        /// </summary>
        public const string InRange = "in_range";

        /// <summary>
        /// Set membership predicate - proves value is in a set without revealing which.
        /// </summary>
        public const string InSet = "in_set";

        /// <summary>
        /// Set exclusion predicate - proves value is not in a set.
        /// </summary>
        public const string NotInSet = "not_in_set";

        /// <summary>
        /// Adult verification predicate - proves age >= 18 without revealing exact age.
        /// </summary>
        public const string IsAdult = "is_adult";

        /// <summary>
        /// Citizenship verification predicate - proves citizenship without revealing exact status.
        /// </summary>
        public const string IsCitizen = "is_citizen";

        /// <summary>
        /// All supported predicate types.
        /// </summary>
        public static readonly string[] All = 
        {
            AgeOver, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual,
            EqualTo, NotEqualTo, InRange, InSet, NotInSet, IsAdult, IsCitizen
        };
    }

    /// <summary>
    /// Zero-knowledge proof types supported for predicates.
    /// </summary>
    public static class ProofTypes
    {
        /// <summary>
        /// Range proof for proving values are within a range without revealing the value.
        /// </summary>
        public const string RangeProof = "range-proof";

        /// <summary>
        /// Zero-knowledge Succinct Non-interactive Argument of Knowledge.
        /// </summary>
        public const string ZkSnark = "zk-snark";

        /// <summary>
        /// BBS+ signatures for selective disclosure and predicate proofs.
        /// </summary>
        public const string BbsPlus = "bbs+";

        /// <summary>
        /// Circuit-based zero-knowledge proofs.
        /// </summary>
        public const string Circuit = "circuit";

        /// <summary>
        /// Bulletproofs for efficient range proofs.
        /// </summary>
        public const string Bulletproof = "bulletproof";

        /// <summary>
        /// All supported zero-knowledge proof types.
        /// </summary>
        public static readonly string[] All = 
        {
            RangeProof, ZkSnark, BbsPlus, Circuit, Bulletproof
        };
    }
}