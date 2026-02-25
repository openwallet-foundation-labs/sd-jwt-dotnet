using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents format constraints for credential formats as defined in DIF Presentation Exchange 2.1.1.
/// Specifies which credential formats are acceptable and their specific requirements.
/// </summary>
public class FormatConstraints {
        /// <summary>
        /// Gets or sets the constraints for JWT format credentials.
        /// Optional. Used for plain JWT tokens.
        /// </summary>
        [JsonPropertyName("jwt")]
        public JwtFormatConstraints? Jwt { get; set; }

        /// <summary>
        /// Gets or sets the constraints for JWT Verifiable Credential format.
        /// Optional. Used for W3C Verifiable Credentials in JWT format.
        /// </summary>
        [JsonPropertyName("jwt_vc")]
        public JwtFormatConstraints? JwtVc { get; set; }

        /// <summary>
        /// Gets or sets the constraints for JWT Verifiable Presentation format.
        /// Optional. Used for W3C Verifiable Presentations in JWT format.
        /// </summary>
        [JsonPropertyName("jwt_vp")]
        public JwtFormatConstraints? JwtVp { get; set; }

        /// <summary>
        /// Gets or sets the constraints for SD-JWT format credentials.
        /// Optional. Used for Selective Disclosure JWT tokens.
        /// </summary>
        [JsonPropertyName("sd-jwt")]
        public SdJwtFormatConstraints? SdJwt { get; set; }

        /// <summary>
        /// Gets or sets the constraints for SD-JWT Verifiable Credential format.
        /// Optional. Used for SD-JWT based Verifiable Credentials.
        /// </summary>
        [JsonPropertyName("vc+sd-jwt")]
        public SdJwtFormatConstraints? SdJwtVc { get; set; }

        /// <summary>
        /// Gets or sets the constraints for Linked Data Proof format.
        /// Optional. Used for JSON-LD credentials with proof.
        /// </summary>
        [JsonPropertyName("ldp")]
        public LdpFormatConstraints? Ldp { get; set; }

        /// <summary>
        /// Gets or sets the constraints for Linked Data Verifiable Credential format.
        /// Optional. Used for JSON-LD Verifiable Credentials.
        /// </summary>
        [JsonPropertyName("ldp_vc")]
        public LdpFormatConstraints? LdpVc { get; set; }

        /// <summary>
        /// Gets or sets the constraints for Linked Data Verifiable Presentation format.
        /// Optional. Used for JSON-LD Verifiable Presentations.
        /// </summary>
        [JsonPropertyName("ldp_vp")]
        public LdpFormatConstraints? LdpVp { get; set; }

        /// <summary>
        /// Validates the format constraints according to DIF PEX 2.1.1 requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
        public void Validate() {
                // Validate individual format constraints
                Jwt?.Validate();
                JwtVc?.Validate();
                JwtVp?.Validate();
                SdJwt?.Validate();
                SdJwtVc?.Validate();
                Ldp?.Validate();
                LdpVc?.Validate();
                LdpVp?.Validate();

                // Ensure at least one format is specified
                if (!HasAnyFormat()) {
                        throw new InvalidOperationException("At least one format constraint must be specified");
                }
        }

        /// <summary>
        /// Checks if any format constraints are specified.
        /// </summary>
        /// <returns>True if at least one format constraint is specified</returns>
        public bool HasAnyFormat() {
                return Jwt != null || JwtVc != null || JwtVp != null ||
                       SdJwt != null || SdJwtVc != null ||
                       Ldp != null || LdpVc != null || LdpVp != null;
        }

        /// <summary>
        /// Gets all supported format identifiers.
        /// </summary>
        /// <returns>Array of supported format identifiers</returns>
        public string[] GetSupportedFormats() {
                var formats = new List<string>();

                if (Jwt != null) formats.Add(PresentationExchangeConstants.Formats.Jwt);
                if (JwtVc != null) formats.Add(PresentationExchangeConstants.Formats.JwtVc);
                if (JwtVp != null) formats.Add(PresentationExchangeConstants.Formats.JwtVp);
                if (SdJwt != null) formats.Add(PresentationExchangeConstants.Formats.SdJwt);
                if (SdJwtVc != null) formats.Add(PresentationExchangeConstants.Formats.SdJwtVc);
                if (Ldp != null) formats.Add(PresentationExchangeConstants.Formats.Ldp);
                if (LdpVc != null) formats.Add(PresentationExchangeConstants.Formats.LdpVc);
                if (LdpVp != null) formats.Add(PresentationExchangeConstants.Formats.LdpVp);

                return formats.ToArray();
        }

        /// <summary>
        /// Checks if a specific format is supported.
        /// </summary>
        /// <param name="format">The format identifier to check</param>
        /// <returns>True if the format is supported</returns>
        public bool SupportsFormat(string format) {
                return format switch {
                        PresentationExchangeConstants.Formats.Jwt => Jwt != null,
                        PresentationExchangeConstants.Formats.JwtVc => JwtVc != null,
                        PresentationExchangeConstants.Formats.JwtVp => JwtVp != null,
                        PresentationExchangeConstants.Formats.SdJwt => SdJwt != null,
                        PresentationExchangeConstants.Formats.SdJwtVc => SdJwtVc != null,
                        PresentationExchangeConstants.Formats.Ldp => Ldp != null,
                        PresentationExchangeConstants.Formats.LdpVc => LdpVc != null,
                        PresentationExchangeConstants.Formats.LdpVp => LdpVp != null,
                        _ => false
                };
        }

        /// <summary>
        /// Creates format constraints that accept only SD-JWT VC format.
        /// </summary>
        /// <returns>A new FormatConstraints instance for SD-JWT VC</returns>
        public static FormatConstraints CreateForSdJwtVc() {
                return new FormatConstraints {
                        SdJwtVc = new SdJwtFormatConstraints()
                };
        }

        /// <summary>
        /// Creates format constraints that accept only JWT VC format.
        /// </summary>
        /// <param name="algorithms">Optional allowed signing algorithms</param>
        /// <returns>A new FormatConstraints instance for JWT VC</returns>
        public static FormatConstraints CreateForJwtVc(string[]? algorithms = null) {
                return new FormatConstraints {
                        JwtVc = new JwtFormatConstraints {
                                Alg = algorithms
                        }
                };
        }

        /// <summary>
        /// Creates format constraints that accept both SD-JWT VC and JWT VC formats.
        /// </summary>
        /// <returns>A new FormatConstraints instance for both formats</returns>
        public static FormatConstraints CreateForVcFormats() {
                return new FormatConstraints {
                        SdJwtVc = new SdJwtFormatConstraints(),
                        JwtVc = new JwtFormatConstraints()
                };
        }

        /// <summary>
        /// Creates format constraints that accept all JWT-based formats.
        /// </summary>
        /// <returns>A new FormatConstraints instance for all JWT formats</returns>
        public static FormatConstraints CreateForJwtFormats() {
                return new FormatConstraints {
                        Jwt = new JwtFormatConstraints(),
                        JwtVc = new JwtFormatConstraints(),
                        JwtVp = new JwtFormatConstraints(),
                        SdJwt = new SdJwtFormatConstraints(),
                        SdJwtVc = new SdJwtFormatConstraints()
                };
        }

        /// <summary>
        /// Creates format constraints that accept all supported formats.
        /// </summary>
        /// <returns>A new FormatConstraints instance for all formats</returns>
        public static FormatConstraints CreateForAllFormats() {
                return new FormatConstraints {
                        Jwt = new JwtFormatConstraints(),
                        JwtVc = new JwtFormatConstraints(),
                        JwtVp = new JwtFormatConstraints(),
                        SdJwt = new SdJwtFormatConstraints(),
                        SdJwtVc = new SdJwtFormatConstraints(),
                        Ldp = new LdpFormatConstraints(),
                        LdpVc = new LdpFormatConstraints(),
                        LdpVp = new LdpFormatConstraints()
                };
        }
}

/// <summary>
/// Represents constraints specific to JWT-based credential formats.
/// </summary>
public class JwtFormatConstraints {
        /// <summary>
        /// Gets or sets the allowed signing algorithms.
        /// Optional. If not specified, any algorithm is acceptable.
        /// </summary>
        [JsonPropertyName("alg")]
        public string[]? Alg { get; set; }

        /// <summary>
        /// Gets or sets the allowed proof types for JWT credentials.
        /// Optional. Used for specific JWT proof requirements.
        /// </summary>
        [JsonPropertyName("proof_type")]
        public string[]? ProofType { get; set; }

        /// <summary>
        /// Validates the JWT format constraints.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
        public void Validate() {
                if (Alg != null) {
                        foreach (var algorithm in Alg) {
                                if (string.IsNullOrWhiteSpace(algorithm))
                                        throw new InvalidOperationException("Algorithm name cannot be null or empty");
                        }
                }

                if (ProofType != null) {
                        foreach (var proof in ProofType) {
                                if (string.IsNullOrWhiteSpace(proof))
                                        throw new InvalidOperationException("Proof type cannot be null or empty");
                        }
                }
        }
}

/// <summary>
/// Represents constraints specific to SD-JWT credential formats.
/// </summary>
public class SdJwtFormatConstraints {
        /// <summary>
        /// Gets or sets the allowed signing algorithms for SD-JWT.
        /// Optional. If not specified, any algorithm is acceptable.
        /// </summary>
        [JsonPropertyName("alg")]
        public string[]? Alg { get; set; }

        /// <summary>
        /// Gets or sets the required hash algorithms for selective disclosure.
        /// Optional. Specifies requirements for disclosure hash algorithms.
        /// </summary>
        [JsonPropertyName("sd_alg")]
        public string[]? SdAlg { get; set; }

        /// <summary>
        /// Gets or sets the required key binding JWT algorithms.
        /// Optional. Used for holder binding in SD-JWT.
        /// </summary>
        [JsonPropertyName("kb_alg")]
        public string[]? KbAlg { get; set; }

        /// <summary>
        /// Validates the SD-JWT format constraints.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
        public void Validate() {
                if (Alg != null) {
                        foreach (var algorithm in Alg) {
                                if (string.IsNullOrWhiteSpace(algorithm))
                                        throw new InvalidOperationException("Algorithm name cannot be null or empty");
                        }
                }

                if (SdAlg != null) {
                        foreach (var algorithm in SdAlg) {
                                if (string.IsNullOrWhiteSpace(algorithm))
                                        throw new InvalidOperationException("SD algorithm name cannot be null or empty");
                        }
                }

                if (KbAlg != null) {
                        foreach (var algorithm in KbAlg) {
                                if (string.IsNullOrWhiteSpace(algorithm))
                                        throw new InvalidOperationException("KB algorithm name cannot be null or empty");
                        }
                }
        }
}

/// <summary>
/// Represents constraints specific to Linked Data Proof credential formats.
/// </summary>
public class LdpFormatConstraints {
        /// <summary>
        /// Gets or sets the allowed proof types for Linked Data credentials.
        /// Optional. Specifies which cryptographic proof types are acceptable.
        /// </summary>
        [JsonPropertyName("proof_type")]
        public string[]? ProofType { get; set; }

        /// <summary>
        /// Validates the LDP format constraints.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
        public void Validate() {
                if (ProofType != null) {
                        foreach (var proof in ProofType) {
                                if (string.IsNullOrWhiteSpace(proof))
                                        throw new InvalidOperationException("Proof type cannot be null or empty");
                        }
                }
        }
}