using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Handover;

/// <summary>
/// Session transcript for mdoc authentication.
/// Contains device engagement, reader key, and handover data.
/// </summary>
public class SessionTranscript : ICborSerializable
{
    /// <summary>
    /// Device engagement data (null for OpenID4VP).
    /// </summary>
    public byte[]? DeviceEngagement
    {
        get; set;
    }

    /// <summary>
    /// Reader key (EReaderKey.Pub) - null for OpenID4VP.
    /// </summary>
    public byte[]? EReaderKeyPub
    {
        get; set;
    }

    /// <summary>
    /// Handover structure (OID4VPHandover, BrowserHandover, etc.).
    /// </summary>
    public ICborSerializable? Handover
    {
        get; set;
    }

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var array = CBORObject.NewArray();

        // DeviceEngagementBytes (null for OpenID4VP)
        if (DeviceEngagement != null)
        {
            array.Add(CBORObject.FromObjectAndTag(DeviceEngagement, 24));
        }
        else
        {
            array.Add(CBORObject.Null);
        }

        // EReaderKeyBytes (null for OpenID4VP)
        if (EReaderKeyPub != null)
        {
            array.Add(CBORObject.FromObjectAndTag(EReaderKeyPub, 24));
        }
        else
        {
            array.Add(CBORObject.Null);
        }

        // Handover
        if (Handover != null)
        {
            array.Add(Handover.ToCborObject());
        }
        else
        {
            array.Add(CBORObject.Null);
        }

        return array;
    }

    /// <summary>
    /// Creates a SessionTranscript from CBOR bytes.
    /// </summary>
    /// <param name="cbor">The CBOR bytes.</param>
    /// <returns>A new SessionTranscript instance.</returns>
    public static SessionTranscript FromCbor(byte[] cbor)
    {
        return FromCborObject(CBORObject.DecodeFromBytes(cbor));
    }

    /// <summary>
    /// Creates a SessionTranscript from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new SessionTranscript instance.</returns>
    public static SessionTranscript FromCborObject(CBORObject cbor)
    {
        var transcript = new SessionTranscript();

        if (cbor.Count > 0 && !cbor[0].IsNull)
        {
            var devEngCbor = cbor[0];
            transcript.DeviceEngagement = devEngCbor.HasMostOuterTag(24)
                ? devEngCbor.GetByteString()
                : devEngCbor.EncodeToBytes();
        }

        if (cbor.Count > 1 && !cbor[1].IsNull)
        {
            var readerKeyCbor = cbor[1];
            transcript.EReaderKeyPub = readerKeyCbor.HasMostOuterTag(24)
                ? readerKeyCbor.GetByteString()
                : readerKeyCbor.EncodeToBytes();
        }

        // Note: Handover deserialization requires knowing the type

        return transcript;
    }

    /// <summary>
    /// Creates a SessionTranscript for OpenID4VP redirect flow.
    /// </summary>
    /// <param name="clientId">The verifier's client ID.</param>
    /// <param name="nonce">The nonce from the authorization request.</param>
    /// <param name="mdocGeneratedNonce">The mdoc-generated nonce (optional).</param>
    /// <param name="responseUri">The response URI.</param>
    /// <returns>A SessionTranscript configured for OpenID4VP.</returns>
    public static SessionTranscript ForOpenId4Vp(
        string clientId,
        string nonce,
        string? mdocGeneratedNonce,
        string responseUri)
    {
        var handover = OpenId4VpHandover.Create(clientId, responseUri, nonce, mdocGeneratedNonce ?? string.Empty);
        return new SessionTranscript
        {
            DeviceEngagement = null,
            EReaderKeyPub = null,
            Handover = handover
        };
    }

    /// <summary>
    /// Creates a SessionTranscript for OpenID4VP DC API flow.
    /// </summary>
    /// <param name="origin">The verifier's origin.</param>
    /// <param name="nonce">The nonce from the request.</param>
    /// <param name="clientId">The client ID (optional, defaults to origin).</param>
    /// <returns>A SessionTranscript configured for DC API.</returns>
    public static SessionTranscript ForOpenId4VpDcApi(
        string origin,
        string nonce,
        string? clientId)
    {
        var handover = OpenId4VpDcApiHandover.Create(origin, clientId ?? origin, nonce);
        return new SessionTranscript
        {
            DeviceEngagement = null,
            EReaderKeyPub = null,
            Handover = handover
        };
    }
}
