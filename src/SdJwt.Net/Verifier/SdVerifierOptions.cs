namespace SdJwt.Net.Verifier;

/// <summary>
/// Options controlling SD-JWT verifier behavior.
/// </summary>
public class SdVerifierOptions {
        /// <summary>
        /// Enables strict RFC 9901 validation checks.
        /// Defaults to true.
        /// </summary>
        public bool StrictMode { get; set; } = true;

        /// <summary>
        /// Key Binding policy controls.
        /// </summary>
        public KeyBindingValidationPolicy KeyBinding { get; set; } = new();
}
