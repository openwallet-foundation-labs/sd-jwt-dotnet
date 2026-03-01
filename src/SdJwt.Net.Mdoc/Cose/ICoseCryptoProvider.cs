using System.Security.Cryptography;

namespace SdJwt.Net.Mdoc.Cose;

/// <summary>
/// Abstraction for COSE cryptographic operations, enabling platform-specific implementations.
/// </summary>
public interface ICoseCryptoProvider
{
    /// <summary>
    /// Signs a CoseSign1 structure.
    /// </summary>
    /// <param name="coseSign1">The COSE_Sign1 structure to sign.</param>
    /// <param name="privateKey">The private key for signing.</param>
    /// <returns>The complete COSE_Sign1 CBOR bytes.</returns>
    byte[] Sign(CoseSign1 coseSign1, ECDsa privateKey);

    /// <summary>
    /// Verifies a CoseSign1 signature.
    /// </summary>
    /// <param name="coseSign1">The COSE_Sign1 structure to verify.</param>
    /// <param name="publicKey">The public key for verification.</param>
    /// <param name="externalAad">Optional external additional authenticated data.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    bool Verify(CoseSign1 coseSign1, ECDsa publicKey, byte[]? externalAad = null);

    /// <summary>
    /// Creates a COSE_Sign1 signature.
    /// </summary>
    /// <param name="payload">The payload to sign.</param>
    /// <param name="privateKey">The private key for signing.</param>
    /// <param name="algorithm">The signing algorithm.</param>
    /// <param name="protectedHeaders">Optional protected headers.</param>
    /// <param name="externalAad">Optional external additional authenticated data.</param>
    /// <returns>The signature bytes.</returns>
    Task<byte[]> SignAsync(
        byte[] payload,
        CoseKey privateKey,
        CoseAlgorithm algorithm,
        byte[]? protectedHeaders = null,
        byte[]? externalAad = null);

    /// <summary>
    /// Verifies a COSE_Sign1 signature.
    /// </summary>
    /// <param name="signatureStructure">The Sig_structure bytes.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="publicKey">The public key for verification.</param>
    /// <param name="algorithm">The signing algorithm.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    Task<bool> VerifyAsync(
        byte[] signatureStructure,
        byte[] signature,
        CoseKey publicKey,
        CoseAlgorithm algorithm);

    /// <summary>
    /// Creates a COSE_Mac0 MAC.
    /// </summary>
    /// <param name="payload">The payload to MAC.</param>
    /// <param name="key">The symmetric key.</param>
    /// <param name="algorithm">The MAC algorithm.</param>
    /// <param name="externalAad">Optional external additional authenticated data.</param>
    /// <returns>The MAC bytes.</returns>
    Task<byte[]> MacAsync(
        byte[] payload,
        byte[] key,
        CoseAlgorithm algorithm,
        byte[]? externalAad = null);

    /// <summary>
    /// Verifies a COSE_Mac0 MAC.
    /// </summary>
    /// <param name="macStructure">The MAC_structure bytes.</param>
    /// <param name="mac">The MAC to verify.</param>
    /// <param name="key">The symmetric key.</param>
    /// <returns>True if the MAC is valid; otherwise, false.</returns>
    Task<bool> VerifyMacAsync(
        byte[] macStructure,
        byte[] mac,
        byte[] key);
}
