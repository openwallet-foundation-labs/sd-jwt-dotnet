using System.Security.Cryptography;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Handover;

/// <summary>
/// OpenID4VP handover structure for redirect flow per OID4VP spec.
/// Used as part of SessionTranscript for device authentication.
/// </summary>
public class OpenId4VpHandover : ICborSerializable
{
    /// <summary>
    /// Client ID of the verifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Response URI where the response should be sent.
    /// </summary>
    public string ResponseUri { get; set; } = string.Empty;

    /// <summary>
    /// Nonce for freshness.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// mdoc generated nonce for binding.
    /// </summary>
    public string MdocGeneratedNonce { get; set; } = string.Empty;

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        // OID4VPHandover = [
        //   clientIdHash: bstr,
        //   responseUriHash: bstr,
        //   nonce: tstr
        // ]
        var clientIdHash = ComputeSha256(ClientId, MdocGeneratedNonce);
        var responseUriHash = ComputeSha256(ResponseUri, MdocGeneratedNonce);

        var array = CBORObject.NewArray();
        array.Add(clientIdHash);
        array.Add(responseUriHash);
        array.Add(Nonce);

        return array;
    }

    /// <summary>
    /// Creates an OpenId4VpHandover from parameters.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="responseUri">The response URI.</param>
    /// <param name="nonce">The nonce.</param>
    /// <param name="mdocGeneratedNonce">The mdoc generated nonce.</param>
    /// <returns>A new OpenId4VpHandover instance.</returns>
    public static OpenId4VpHandover Create(
        string clientId,
        string responseUri,
        string nonce,
        string mdocGeneratedNonce)
    {
        return new OpenId4VpHandover
        {
            ClientId = clientId,
            ResponseUri = responseUri,
            Nonce = nonce,
            MdocGeneratedNonce = mdocGeneratedNonce
        };
    }

    /// <summary>
    /// Creates a SessionTranscript with this handover.
    /// </summary>
    /// <returns>A SessionTranscript for OpenID4VP.</returns>
    public SessionTranscript CreateSessionTranscript()
    {
        return new SessionTranscript
        {
            DeviceEngagement = null,
            EReaderKeyPub = null,
            Handover = this
        };
    }

    private static byte[] ComputeSha256(string value, string mdocGeneratedNonce)
    {
        // Per OpenID4VP Appendix B.2 the hash is taken over the CBOR-encoded array
        // [value, mdoc_generated_nonce], not a plain string concatenation. The array
        // encoding unambiguously delimits the two inputs and is interoperable with
        // other mdoc implementations.
        var toHash = CBORObject.NewArray();
        toHash.Add(value);
        toHash.Add(mdocGeneratedNonce);
        var combined = toHash.EncodeToBytes();
#if NETSTANDARD2_1
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(combined);
#else
        return SHA256.HashData(combined);
#endif
    }
}
