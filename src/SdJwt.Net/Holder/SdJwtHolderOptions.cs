namespace SdJwt.Net.Holder;

/// <summary>
/// Options controlling SD-JWT holder presentation behavior.
/// </summary>
public class SdJwtHolderOptions {
        /// <summary>
        /// Enables strict RFC 9901 checks when creating presentations.
        /// Defaults to true.
        /// </summary>
        public bool StrictMode { get; set; } = true;
}
