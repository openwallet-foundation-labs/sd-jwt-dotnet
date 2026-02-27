using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents a field constraint as defined in DIF Presentation Exchange 2.1.1.
/// Specifies requirements for specific credential fields using JSON paths and filters.
/// </summary>
public class Field {
        /// <summary>
        /// Gets or sets the JSON paths that identify the field location in the credential.
        /// Required. Must contain at least one valid JSON path.
        /// </summary>
        [JsonPropertyName("path")]
        public string[]? Path { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this field constraint.
        /// Optional. Used for referencing in presentation submissions.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name for this field constraint.
        /// Optional. Used for display purposes in user interfaces.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the purpose or reason for this field constraint.
        /// Optional. Explains why this field is required.
        /// </summary>
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }

        /// <summary>
        /// Gets or sets the filter criteria for validating the field value.
        /// Optional. If not specified, only the existence of the field is checked.
        /// </summary>
        [JsonPropertyName("filter")]
        public FieldFilter? Filter { get; set; }

        /// <summary>
        /// Gets or sets whether this field is required to be present in the credential.
        /// Optional. Defaults to true if not specified.
        /// </summary>
        [JsonPropertyName("optional")]
        public bool Optional { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the value of this field should be disclosed in the presentation.
        /// Optional. Used for selective disclosure scenarios.
        /// </summary>
        [JsonPropertyName("intent_to_retain")]
        public bool IntentToRetain { get; set; } = false;

        /// <summary>
        /// Validates the field constraint according to DIF PEX 2.1.1 requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the field constraint is invalid</exception>
        public void Validate() {
                if (Path == null || Path.Length == 0)
                        throw new InvalidOperationException("Field constraint must have at least one path");

                foreach (var path in Path) {
                        if (string.IsNullOrWhiteSpace(path))
                                throw new InvalidOperationException("Field path cannot be null or empty");

                        if (!path.StartsWith("$"))
                                throw new InvalidOperationException($"Field path must be a valid JSON path starting with '$': {path}");
                }

                // Validate filter if present
                if (Filter is PredicateFilter predicateFilter) {
                        predicateFilter.Validate();
                }
                else {
                        Filter?.Validate();
                }
        }

        /// <summary>
        /// Creates a basic field constraint with a single path.
        /// </summary>
        /// <param name="path">The JSON path to the field</param>
        /// <param name="filter">Optional filter criteria</param>
        /// <param name="optional">Whether the field is optional</param>
        /// <returns>A new Field instance</returns>
        public static Field Create(string path, FieldFilter? filter = null, bool optional = false) {
                if (string.IsNullOrWhiteSpace(path))
                        throw new ArgumentException("Path cannot be null or empty", nameof(path));

                return new Field {
                        Path = new[] { path },
                        Filter = filter,
                        Optional = optional
                };
        }

        /// <summary>
        /// Creates a field constraint with multiple possible paths.
        /// </summary>
        /// <param name="paths">The JSON paths to the field</param>
        /// <param name="filter">Optional filter criteria</param>
        /// <param name="optional">Whether the field is optional</param>
        /// <returns>A new Field instance</returns>
        public static Field Create(string[] paths, FieldFilter? filter = null, bool optional = false) {
                if (paths == null || paths.Length == 0)
                        throw new ArgumentException("At least one path is required", nameof(paths));

                return new Field {
                        Path = paths,
                        Filter = filter,
                        Optional = optional
                };
        }

        /// <summary>
        /// Creates a field constraint that requires a specific string value.
        /// </summary>
        /// <param name="path">The JSON path to the field</param>
        /// <param name="value">The required string value</param>
        /// <param name="optional">Whether the field is optional</param>
        /// <returns>A new Field instance with a const filter</returns>
        public static Field CreateForValue(string path, string value, bool optional = false) {
                return Create(path, FieldFilter.CreateConst(value), optional);
        }

        /// <summary>
        /// Creates a field constraint that requires one of several values.
        /// </summary>
        /// <param name="path">The JSON path to the field</param>
        /// <param name="values">The allowed values</param>
        /// <param name="optional">Whether the field is optional</param>
        /// <returns>A new Field instance with an enum filter</returns>
        public static Field CreateForValues(string path, string[] values, bool optional = false) {
                return Create(path, FieldFilter.CreateEnum(values), optional);
        }

        /// <summary>
        /// Creates a field constraint that requires a value within a numeric range.
        /// </summary>
        /// <param name="path">The JSON path to the field</param>
        /// <param name="minimum">The minimum allowed value (inclusive)</param>
        /// <param name="maximum">The maximum allowed value (inclusive)</param>
        /// <param name="optional">Whether the field is optional</param>
        /// <returns>A new Field instance with range filters</returns>
        public static Field CreateForRange(string path, object? minimum = null, object? maximum = null, bool optional = false) {
                var filter = new FieldFilter {
                        Type = "number"
                };

                if (minimum != null)
                        filter.Minimum = minimum;
                if (maximum != null)
                        filter.Maximum = maximum;

                return Create(path, filter, optional);
        }

        /// <summary>
        /// Creates a field constraint that checks for the existence of a field.
        /// </summary>
        /// <param name="path">The JSON path to the field</param>
        /// <param name="optional">Whether the field is optional</param>
        /// <returns>A new Field instance that only checks existence</returns>
        public static Field CreateForExistence(string path, bool optional = false) {
                return Create(path, null, optional);
        }

        /// <summary>
        /// Creates a field constraint for an issuer URL.
        /// </summary>
        /// <param name="issuerUrl">The required issuer URL</param>
        /// <param name="optional">Whether the issuer constraint is optional</param>
        /// <returns>A new Field instance for issuer validation</returns>
        public static Field CreateForIssuer(string issuerUrl, bool optional = false) {
                return CreateForValue(PresentationExchangeConstants.CommonJsonPaths.Issuer, issuerUrl, optional);
        }

        /// <summary>
        /// Creates a field constraint for multiple allowed issuers.
        /// </summary>
        /// <param name="issuerUrls">The allowed issuer URLs</param>
        /// <param name="optional">Whether the issuer constraint is optional</param>
        /// <returns>A new Field instance for issuer validation</returns>
        public static Field CreateForIssuers(string[] issuerUrls, bool optional = false) {
                return CreateForValues(PresentationExchangeConstants.CommonJsonPaths.Issuer, issuerUrls, optional);
        }

        /// <summary>
        /// Creates a field constraint for a credential type.
        /// </summary>
        /// <param name="credentialType">The required credential type</param>
        /// <param name="isVc">Whether this is a verifiable credential type</param>
        /// <param name="optional">Whether the type constraint is optional</param>
        /// <returns>A new Field instance for type validation</returns>
        public static Field CreateForType(string credentialType, bool isVc = false, bool optional = false) {
                var path = isVc
                    ? PresentationExchangeConstants.CommonJsonPaths.VcType
                    : PresentationExchangeConstants.CommonJsonPaths.VctType;

                var filter = isVc
                    ? new FieldFilter {
                            Type = "array",
                            Contains = new { Const = credentialType }
                    }
                    : FieldFilter.CreateConst(credentialType);

                return Create(path, filter, optional);
        }

        /// <summary>
        /// Creates a field constraint for age verification (over a certain age).
        /// </summary>
        /// <param name="minimumAge">The minimum required age</param>
        /// <param name="agePath">The JSON path to the age field (default: $.age)</param>
        /// <param name="optional">Whether the age constraint is optional</param>
        /// <returns>A new Field instance for age validation</returns>
        public static Field CreateForMinimumAge(int minimumAge, string agePath = "$.age", bool optional = false) {
                return CreateForRange(agePath, minimumAge, null, optional);
        }

        /// <summary>
        /// Creates a field constraint for age verification using predicates.
        /// </summary>
        /// <param name="minimumAge">The minimum age to verify</param>
        /// <param name="agePath">The JSON path to the age field (default: $.age)</param>
        /// <param name="useZeroKnowledge">Whether to use zero-knowledge proofs</param>
        /// <param name="optional">Whether the age constraint is optional</param>
        /// <returns>A new Field instance for age verification</returns>
        public static Field CreateForAgeVerification(int minimumAge, string agePath = "$.age", bool useZeroKnowledge = false, bool optional = false) {
                return Create(agePath, PredicateFilter.CreateAgeOver(minimumAge, useZeroKnowledge), optional);
        }

        /// <summary>
        /// Creates a field constraint for income verification using predicates.
        /// </summary>
        /// <param name="minimumIncome">The minimum income to verify</param>
        /// <param name="incomePath">The JSON path to the income field (default: $.income)</param>
        /// <param name="useZeroKnowledge">Whether to use zero-knowledge proofs</param>
        /// <param name="optional">Whether the income constraint is optional</param>
        /// <returns>A new Field instance for income verification</returns>
        public static Field CreateForIncomeVerification(decimal minimumIncome, string incomePath = "$.income", bool useZeroKnowledge = true, bool optional = false) {
                return Create(incomePath, PredicateFilter.CreateSalaryOver(minimumIncome, useZeroKnowledge), optional);
        }

        /// <summary>
        /// Creates a field constraint for citizenship verification.
        /// </summary>
        /// <param name="allowedCountries">Array of allowed country codes</param>
        /// <param name="citizenshipPath">The JSON path to the citizenship field (default: $.citizenship)</param>
        /// <param name="optional">Whether the citizenship constraint is optional</param>
        /// <returns>A new Field instance for citizenship verification</returns>
        public static Field CreateForCitizenshipVerification(string[] allowedCountries, string citizenshipPath = "$.citizenship", bool optional = false) {
                return Create(citizenshipPath, PredicateFilter.CreateCitizenshipIn(allowedCountries), optional);
        }

        /// <summary>
        /// Creates a field constraint for credit score verification using range predicates.
        /// </summary>
        /// <param name="minimumScore">The minimum credit score</param>
        /// <param name="maximumScore">The maximum credit score (optional)</param>
        /// <param name="scorePath">The JSON path to the credit score field (default: $.credit_score)</param>
        /// <param name="useZeroKnowledge">Whether to use zero-knowledge proofs</param>
        /// <param name="optional">Whether the score constraint is optional</param>
        /// <returns>A new Field instance for credit score verification</returns>
        public static Field CreateForCreditScoreVerification(int minimumScore, int? maximumScore = null, string scorePath = "$.credit_score", bool useZeroKnowledge = true, bool optional = false) {
                if (maximumScore.HasValue) {
                        return Create(scorePath, PredicateFilter.CreateRange(minimumScore, maximumScore.Value, useZeroKnowledge), optional);
                }
                else {
                        var filter = new PredicateFilter {
                                Type = "predicate",
                                Predicate = "greater_than_or_equal",
                                Threshold = minimumScore,
                                ZeroKnowledge = useZeroKnowledge,
                                ProofType = useZeroKnowledge ? "range-proof" : null
                        };
                        return Create(scorePath, filter, optional);
                }
        }

        /// <summary>
        /// Gets the primary JSON path for this field (first path in the array).
        /// </summary>
        /// <returns>The primary JSON path or null if no paths are defined</returns>
        public string? GetPrimaryPath() {
                return Path?.FirstOrDefault();
        }

        /// <summary>
        /// Checks if this field constraint has a filter defined.
        /// </summary>
        /// <returns>True if a filter is defined</returns>
        public bool HasFilter() {
                return Filter != null;
        }

        /// <summary>
        /// Checks if this field requires selective disclosure.
        /// </summary>
        /// <returns>True if the field should be disclosed selectively</returns>
        public bool RequiresSelectiveDisclosure() {
                return IntentToRetain;
        }
}
