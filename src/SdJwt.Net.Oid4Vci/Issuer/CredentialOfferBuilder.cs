using SdJwt.Net.Oid4Vci.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Issuer;

/// <summary>
/// Fluent builder for creating credential offers.
/// </summary>
public class CredentialOfferBuilder {
        private readonly CredentialOffer _credentialOffer;
        private static readonly JsonSerializerOptions JsonOptions = new() {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
#if NET6_0_OR_GREATER
            ,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
#endif
        };

        private CredentialOfferBuilder(string credentialIssuer) {
                _credentialOffer = new CredentialOffer {
                        CredentialIssuer = credentialIssuer
                };
        }

        /// <summary>
        /// Creates a new credential offer builder.
        /// </summary>
        /// <param name="credentialIssuer">The credential issuer URL</param>
        /// <returns>A new CredentialOfferBuilder instance</returns>
        public static CredentialOfferBuilder Create(string credentialIssuer) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(credentialIssuer, nameof(credentialIssuer));
#else
        if (string.IsNullOrWhiteSpace(credentialIssuer))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialIssuer));
#endif
                return new CredentialOfferBuilder(credentialIssuer);
        }

        /// <summary>
        /// Adds a credential configuration identifier to the offer.
        /// </summary>
        /// <param name="configurationId">The credential configuration identifier</param>
        /// <returns>This builder instance for method chaining</returns>
        public CredentialOfferBuilder AddConfigurationId(string configurationId) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(configurationId, nameof(configurationId));
#else
        if (string.IsNullOrWhiteSpace(configurationId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(configurationId));
#endif

                var currentIds = _credentialOffer.CredentialConfigurationIds?.ToList() ?? new List<string>();
                if (!currentIds.Contains(configurationId)) {
                        currentIds.Add(configurationId);
                        _credentialOffer.CredentialConfigurationIds = currentIds.ToArray();
                }

                return this;
        }

        /// <summary>
        /// Adds multiple credential configuration identifiers to the offer.
        /// </summary>
        /// <param name="configurationIds">The credential configuration identifiers</param>
        /// <returns>This builder instance for method chaining</returns>
        public CredentialOfferBuilder AddConfigurationIds(params string[] configurationIds) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(configurationIds, nameof(configurationIds));
#else
        if (configurationIds == null)
            throw new ArgumentNullException(nameof(configurationIds));
#endif

                foreach (var id in configurationIds) {
                        AddConfigurationId(id);
                }

                return this;
        }

        /// <summary>
        /// Adds a pre-authorized code grant to the offer.
        /// </summary>
        /// <param name="preAuthorizedCode">The pre-authorized code</param>
        /// <param name="pinLength">Optional PIN length for transaction code</param>
        /// <param name="inputMode">Optional input mode for transaction code</param>
        /// <param name="description">Optional description for transaction code</param>
        /// <param name="interval">Optional polling interval in seconds</param>
        /// <returns>This builder instance for method chaining</returns>
        public CredentialOfferBuilder UsePreAuthorizedCode(
            string preAuthorizedCode,
            int? pinLength = null,
            string? inputMode = null,
            string? description = null,
            int? interval = null) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(preAuthorizedCode, nameof(preAuthorizedCode));
#else
        if (string.IsNullOrWhiteSpace(preAuthorizedCode))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(preAuthorizedCode));
#endif

                TransactionCode? transactionCode = null;
                if (pinLength.HasValue || !string.IsNullOrEmpty(inputMode) || !string.IsNullOrEmpty(description)) {
                        transactionCode = new TransactionCode {
                                Length = pinLength,
                                InputMode = inputMode ?? Oid4VciConstants.InputModes.Numeric,
                                Description = description
                        };
                }

                _credentialOffer.AddPreAuthorizedCodeGrant(preAuthorizedCode, transactionCode, interval);
                return this;
        }

        /// <summary>
        /// Adds a pre-authorized code grant with transaction code to the offer.
        /// </summary>
        /// <param name="preAuthorizedCode">The pre-authorized code</param>
        /// <param name="transactionCode">The transaction code configuration</param>
        /// <param name="interval">Optional polling interval in seconds</param>
        /// <returns>This builder instance for method chaining</returns>
        public CredentialOfferBuilder UsePreAuthorizedCode(string preAuthorizedCode, TransactionCode transactionCode, int? interval = null) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(preAuthorizedCode, nameof(preAuthorizedCode));
                ArgumentNullException.ThrowIfNull(transactionCode, nameof(transactionCode));
#else
        if (string.IsNullOrWhiteSpace(preAuthorizedCode))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(preAuthorizedCode));
        if (transactionCode == null)
            throw new ArgumentNullException(nameof(transactionCode));
#endif

                _credentialOffer.AddPreAuthorizedCodeGrant(preAuthorizedCode, transactionCode, interval);
                return this;
        }

        /// <summary>
        /// Adds an authorization code grant to the offer.
        /// </summary>
        /// <param name="issuerState">Optional issuer state parameter</param>
        /// <param name="authorizationServer">Optional authorization server endpoint</param>
        /// <returns>This builder instance for method chaining</returns>
        public CredentialOfferBuilder UseAuthorizationCode(string? issuerState = null, string? authorizationServer = null) {
                _credentialOffer.AddAuthorizationCodeGrant(issuerState, authorizationServer);
                return this;
        }

        /// <summary>
        /// Adds a custom grant to the offer.
        /// </summary>
        /// <param name="grantType">The grant type identifier</param>
        /// <param name="grantData">The grant-specific data</param>
        /// <returns>This builder instance for method chaining</returns>
        public CredentialOfferBuilder AddCustomGrant(string grantType, object grantData) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(grantType, nameof(grantType));
                ArgumentNullException.ThrowIfNull(grantData, nameof(grantData));
#else
        if (string.IsNullOrWhiteSpace(grantType))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(grantType));
        if (grantData == null)
            throw new ArgumentNullException(nameof(grantData));
#endif

                _credentialOffer.Grants ??= new Dictionary<string, object>();
                _credentialOffer.Grants[grantType] = grantData;

                return this;
        }

        /// <summary>
        /// Builds the credential offer object.
        /// </summary>
        /// <returns>The constructed CredentialOffer</returns>
        /// <exception cref="InvalidOperationException">Thrown when required fields are missing</exception>
        public CredentialOffer Build() {
                if (string.IsNullOrWhiteSpace(_credentialOffer.CredentialIssuer)) {
                        throw new InvalidOperationException("CredentialIssuer is required");
                }

                if (_credentialOffer.CredentialConfigurationIds == null || _credentialOffer.CredentialConfigurationIds.Length == 0) {
                        throw new InvalidOperationException("At least one credential configuration ID is required");
                }

                // Return a copy to prevent further modifications
                var json = JsonSerializer.Serialize(_credentialOffer, JsonOptions);
                return JsonSerializer.Deserialize<CredentialOffer>(json, JsonOptions)!;
        }

        /// <summary>
        /// Builds the credential offer as a JSON string.
        /// </summary>
        /// <returns>The credential offer as JSON</returns>
        public string BuildJson() {
                var credentialOffer = Build();
                return JsonSerializer.Serialize(credentialOffer, JsonOptions);
        }

        /// <summary>
        /// Builds the credential offer as a complete URI.
        /// </summary>
        /// <returns>The credential offer URI</returns>
        public string BuildUri() {
                var credentialOffer = Build();
                return Client.CredentialOfferParser.CreateUri(credentialOffer);
        }
}