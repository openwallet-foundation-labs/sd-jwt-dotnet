using System.Security.Cryptography;
using System.Text;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Handover;

/// <summary>
/// OpenID4VP handover structure for DC API (Digital Credentials API) flow.
/// Used for browser-based credential presentations.
/// </summary>
public class OpenId4VpDcApiHandover : ICborSerializable
{
    /// <summary>
    /// Origin of the verifier website.
    /// </summary>
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// Client ID of the verifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Nonce for freshness.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        // BrowserHandover = [
        //   "openid4vp",
        //   originHash: bstr (SHA-256 of origin),
        //   nonce: tstr,
        //   aud: tstr  ; client_id
        // ]
        var originHash = ComputeSha256(Encoding.UTF8.GetBytes(Origin));

        var array = CBORObject.NewArray();
        array.Add("openid4vp");
        array.Add(originHash);
        array.Add(Nonce);
        array.Add(ClientId);

        return array;
    }

    private static byte[] ComputeSha256(byte[] data)
    {
#if NETSTANDARD2_1
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
#else
        return SHA256.HashData(data);
#endif
    }

    /// <summary>
    /// Creates an OpenId4VpDcApiHandover from parameters.
    /// </summary>
    /// <param name="origin">The verifier origin.</param>
    /// <param name="clientId">The client ID.</param>
    /// <param name="nonce">The nonce.</param>
    /// <returns>A new OpenId4VpDcApiHandover instance.</returns>
    public static OpenId4VpDcApiHandover Create(string origin, string clientId, string nonce)
    {
        return new OpenId4VpDcApiHandover
        {
            Origin = origin,
            ClientId = clientId,
            Nonce = nonce
        };
    }

    /// <summary>
    /// Creates a SessionTranscript with this handover.
    /// </summary>
    /// <returns>A SessionTranscript for DC API flow.</returns>
    public SessionTranscript CreateSessionTranscript()
    {
        return new SessionTranscript
        {
            DeviceEngagement = null,
            EReaderKeyPub = null,
            Handover = this
        };
    }
}
