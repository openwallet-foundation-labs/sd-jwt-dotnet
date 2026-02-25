using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Detects the format of credential objects for presentation exchange.
/// Supports various credential formats including SD-JWT, JWT VC, and Linked Data.
/// </summary>
public class CredentialFormatDetector {
        private readonly ILogger<CredentialFormatDetector> _logger;

        /// <summary>
        /// Initializes a new instance of the CredentialFormatDetector class.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public CredentialFormatDetector(ILogger<CredentialFormatDetector> logger) {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Detects the format of a credential asynchronously.
        /// </summary>
        /// <param name="credential">The credential to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the credential format information</returns>
        public async Task<CredentialFormatInfo> DetectFormatAsync(
            object credential,
            CancellationToken cancellationToken = default) {
                try {
                        _logger.LogDebug("Detecting credential format for credential of type: {Type}", credential.GetType().Name);

                        // Handle string credentials (JWT, SD-JWT)
                        if (credential is string credentialString) {
                                return await DetectStringCredentialFormatAsync(credentialString, cancellationToken);
                        }

                        // Handle structured object credentials
                        return await DetectObjectCredentialFormatAsync(credential, cancellationToken);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error detecting credential format");

                        return new CredentialFormatInfo {
                                Format = "unknown",
                                IsSupported = false,
                                DetectionError = ex.Message
                        };
                }
        }

        /// <summary>
        /// Detects the format of string-based credentials.
        /// </summary>
        /// <param name="credentialString">The credential string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the credential format information</returns>
        private async Task<CredentialFormatInfo> DetectStringCredentialFormatAsync(
            string credentialString,
            CancellationToken cancellationToken = default) {
                if (string.IsNullOrWhiteSpace(credentialString)) {
                        return new CredentialFormatInfo {
                                Format = "unknown",
                                IsSupported = false,
                                DetectionError = "Empty or null credential string"
                        };
                }

                // Check for SD-JWT format (contains ~ separators)
                if (credentialString.Contains("~")) {
                        return await AnalyzeSdJwtAsync(credentialString, cancellationToken);
                }

                // Check for JWT format (three base64url parts separated by dots)
                var jwtParts = credentialString.Split('.');
                if (jwtParts.Length == 3) {
                        return await AnalyzeJwtAsync(credentialString, jwtParts, cancellationToken);
                }

                // Check if it's JSON (Linked Data format)
                if (credentialString.Trim().StartsWith("{")) {
                        return await AnalyzeJsonCredentialAsync(credentialString, cancellationToken);
                }

                return new CredentialFormatInfo {
                        Format = "unknown",
                        IsSupported = false,
                        DetectionError = "Unrecognized string credential format"
                };
        }

        /// <summary>
        /// Detects the format of object-based credentials.
        /// </summary>
        /// <param name="credential">The credential object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the credential format information</returns>
        private async Task<CredentialFormatInfo> DetectObjectCredentialFormatAsync(
            object credential,
            CancellationToken cancellationToken = default) {
                try {
                        // Serialize to JSON for analysis
                        var json = System.Text.Json.JsonSerializer.Serialize(credential);
                        return await AnalyzeJsonCredentialAsync(json, cancellationToken);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error serializing credential object to JSON");

                        return new CredentialFormatInfo {
                                Format = "unknown",
                                IsSupported = false,
                                DetectionError = $"Unable to serialize credential: {ex.Message}"
                        };
                }
        }

        /// <summary>
        /// Analyzes SD-JWT credentials.
        /// </summary>
        /// <param name="sdJwtString">The SD-JWT string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the credential format information</returns>
        private Task<CredentialFormatInfo> AnalyzeSdJwtAsync(
            string sdJwtString,
            CancellationToken cancellationToken = default) {
                try {
                        var parts = sdJwtString.Split('~');

                        if (parts.Length < 2) {
                                return Task.FromResult(new CredentialFormatInfo {
                                        Format = "unknown",
                                        IsSupported = false,
                                        DetectionError = "Invalid SD-JWT format: insufficient parts"
                                });
                        }

                        // The first part should be a JWT
                        var jwtPart = parts[0];
                        var jwtParts = jwtPart.Split('.');

                        if (jwtParts.Length != 3) {
                                return Task.FromResult(new CredentialFormatInfo {
                                        Format = "unknown",
                                        IsSupported = false,
                                        DetectionError = "Invalid SD-JWT format: JWT part is malformed"
                                });
                        }

                        // Try to decode the payload to determine if it's VC format
                        var payload = DecodeJwtPayload(jwtParts[1]);

                        var formatInfo = new CredentialFormatInfo {
                                IsSupported = true,
                                SupportsSelectiveDisclosure = true,
                                DisclosureCount = parts.Length - 1
                        };

                        // Check if it's SD-JWT VC format
                        if (payload.ContainsKey("vct") || payload.ContainsKey("vc")) {
                                formatInfo.Format = PresentationExchangeConstants.Formats.SdJwtVc;
                                formatInfo.IsVerifiableCredential = true;
                        }
                        else {
                                formatInfo.Format = PresentationExchangeConstants.Formats.SdJwt;
                        }

                        _logger.LogDebug("Detected SD-JWT format: {Format}, Disclosures: {Count}",
                            formatInfo.Format, formatInfo.DisclosureCount);

                        return Task.FromResult(formatInfo);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error analyzing SD-JWT credential");

                        return Task.FromResult(new CredentialFormatInfo {
                                Format = "unknown",
                                IsSupported = false,
                                DetectionError = $"SD-JWT analysis failed: {ex.Message}"
                        });
                }
        }

        /// <summary>
        /// Analyzes JWT credentials.
        /// </summary>
        /// <param name="jwtString">The JWT string</param>
        /// <param name="jwtParts">The JWT parts</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the credential format information</returns>
        private Task<CredentialFormatInfo> AnalyzeJwtAsync(
            string jwtString,
            string[] jwtParts,
            CancellationToken cancellationToken = default) {
                try {
                        var payload = DecodeJwtPayload(jwtParts[1]);

                        var formatInfo = new CredentialFormatInfo {
                                IsSupported = true,
                                SupportsSelectiveDisclosure = false
                        };

                        // Check if it's a verifiable credential
                        if (payload.ContainsKey("vc")) {
                                formatInfo.Format = PresentationExchangeConstants.Formats.JwtVc;
                                formatInfo.IsVerifiableCredential = true;
                        }
                        // Check if it's a verifiable presentation
                        else if (payload.ContainsKey("vp")) {
                                formatInfo.Format = PresentationExchangeConstants.Formats.JwtVp;
                                formatInfo.IsVerifiablePresentation = true;
                        }
                        else {
                                formatInfo.Format = PresentationExchangeConstants.Formats.Jwt;
                        }

                        _logger.LogDebug("Detected JWT format: {Format}", formatInfo.Format);

                        return Task.FromResult(formatInfo);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error analyzing JWT credential");

                        return Task.FromResult(new CredentialFormatInfo {
                                Format = "unknown",
                                IsSupported = false,
                                DetectionError = $"JWT analysis failed: {ex.Message}"
                        });
                }
        }

        /// <summary>
        /// Analyzes JSON-based credentials (Linked Data).
        /// </summary>
        /// <param name="jsonString">The JSON string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the credential format information</returns>
        private Task<CredentialFormatInfo> AnalyzeJsonCredentialAsync(
            string jsonString,
            CancellationToken cancellationToken = default) {
                try {
                        using var document = System.Text.Json.JsonDocument.Parse(jsonString);
                        var root = document.RootElement;

                        var formatInfo = new CredentialFormatInfo {
                                IsSupported = true,
                                SupportsSelectiveDisclosure = false
                        };

                        // Check for verifiable credential properties
                        if (root.TryGetProperty("@context", out _) && root.TryGetProperty("type", out var typeProperty)) {
                                var hasCredentialType = false;
                                var hasPresentationType = false;

                                if (typeProperty.ValueKind == System.Text.Json.JsonValueKind.Array) {
                                        foreach (var type in typeProperty.EnumerateArray()) {
                                                var typeValue = type.GetString();
                                                if (typeValue == "VerifiableCredential") {
                                                        hasCredentialType = true;
                                                }
                                                else if (typeValue == "VerifiablePresentation") {
                                                        hasPresentationType = true;
                                                }
                                        }
                                }
                                else if (typeProperty.ValueKind == System.Text.Json.JsonValueKind.String) {
                                        var typeValue = typeProperty.GetString();
                                        hasCredentialType = typeValue == "VerifiableCredential";
                                        hasPresentationType = typeValue == "VerifiablePresentation";
                                }

                                if (hasCredentialType) {
                                        formatInfo.Format = PresentationExchangeConstants.Formats.LdpVc;
                                        formatInfo.IsVerifiableCredential = true;
                                }
                                else if (hasPresentationType) {
                                        formatInfo.Format = PresentationExchangeConstants.Formats.LdpVp;
                                        formatInfo.IsVerifiablePresentation = true;
                                }
                                else {
                                        formatInfo.Format = PresentationExchangeConstants.Formats.Ldp;
                                }
                        }
                        else {
                                formatInfo.Format = "json";
                                formatInfo.IsSupported = false;
                                formatInfo.DetectionError = "JSON format detected but not a recognized verifiable credential format";
                        }

                        // Check for selective disclosure support
                        if (root.TryGetProperty("_sd", out _) || root.TryGetProperty("_sd_alg", out _)) {
                                formatInfo.SupportsSelectiveDisclosure = true;
                        }

                        _logger.LogDebug("Detected JSON credential format: {Format}", formatInfo.Format);

                        return Task.FromResult(formatInfo);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error analyzing JSON credential");

                        return Task.FromResult(new CredentialFormatInfo {
                                Format = "unknown",
                                IsSupported = false,
                                DetectionError = $"JSON analysis failed: {ex.Message}"
                        });
                }
        }

        /// <summary>
        /// Decodes a JWT payload from base64url encoding.
        /// </summary>
        /// <param name="payloadPart">The base64url encoded payload</param>
        /// <returns>Dictionary representing the payload</returns>
        private Dictionary<string, object> DecodeJwtPayload(string payloadPart) {
                try {
                        // Add padding if necessary
                        var paddedPayload = payloadPart;
                        var padding = 4 - (payloadPart.Length % 4);
                        if (padding != 4) {
                                paddedPayload += new string('=', padding);
                        }

                        // Convert from base64url to base64
                        paddedPayload = paddedPayload.Replace('-', '+').Replace('_', '/');

                        // Decode
                        var payloadBytes = Convert.FromBase64String(paddedPayload);
                        var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

                        // Parse JSON
                        using var document = System.Text.Json.JsonDocument.Parse(payloadJson);
                        var payload = new Dictionary<string, object>();

                        foreach (var property in document.RootElement.EnumerateObject()) {
                                payload[property.Name] = property.Value.Clone();
                        }

                        return payload;
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error decoding JWT payload");
                        return new Dictionary<string, object>();
                }
        }
}

/// <summary>
/// Contains information about a detected credential format.
/// </summary>
public class CredentialFormatInfo {
        /// <summary>
        /// Gets or sets the detected format identifier.
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the format is supported by the presentation exchange engine.
        /// </summary>
        public bool IsSupported { get; set; }

        /// <summary>
        /// Gets or sets whether the credential supports selective disclosure.
        /// </summary>
        public bool SupportsSelectiveDisclosure { get; set; }

        /// <summary>
        /// Gets or sets whether this is a verifiable credential.
        /// </summary>
        public bool IsVerifiableCredential { get; set; }

        /// <summary>
        /// Gets or sets whether this is a verifiable presentation.
        /// </summary>
        public bool IsVerifiablePresentation { get; set; }

        /// <summary>
        /// Gets or sets the number of disclosures (for SD-JWT formats).
        /// </summary>
        public int? DisclosureCount { get; set; }

        /// <summary>
        /// Gets or sets any error that occurred during format detection.
        /// </summary>
        public string? DetectionError { get; set; }

        /// <summary>
        /// Gets or sets additional format-specific properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// Adds a property to the format information.
        /// </summary>
        /// <param name="key">The property key</param>
        /// <param name="value">The property value</param>
        public void AddProperty(string key, object value) {
                Properties[key] = value;
        }

        /// <summary>
        /// Gets a property value if it exists.
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="key">The property key</param>
        /// <returns>The property value or default if not found</returns>
        public T? GetProperty<T>(string key) {
                if (Properties.TryGetValue(key, out var value) && value is T typedValue) {
                        return typedValue;
                }

                return default;
        }
}