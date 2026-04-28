namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Abstraction for a compliance profile that validates cryptographic operations
/// across wallet flows. Implementations bridge to HAIP or EUDIW ARF validators.
/// </summary>
public interface IComplianceProfile
{
    /// <summary>
    /// Gets the display name of the compliance profile (e.g., "HAIP Level 2", "EUDIW ARF").
    /// </summary>
    string ProfileName
    {
        get;
    }

    /// <summary>
    /// Validates whether the specified algorithm is compliant with this profile.
    /// </summary>
    /// <param name="algorithm">The algorithm identifier (e.g., "ES256").</param>
    /// <returns>True if the algorithm is allowed.</returns>
    bool IsAlgorithmAllowed(string algorithm);

    /// <summary>
    /// Validates whether the specified key type and curve are compliant.
    /// </summary>
    /// <param name="keyType">The key type (e.g., "EC").</param>
    /// <param name="curve">The curve name (e.g., "P-256").</param>
    /// <returns>True if the key parameters are allowed.</returns>
    bool IsKeyAllowed(string keyType, string? curve);
}
