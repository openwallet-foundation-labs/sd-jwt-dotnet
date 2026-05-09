using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// Reader authentication structure per ISO 18013-5 Section 9.1.4.
/// ReaderAuthentication = ["ReaderAuthentication", SessionTranscript, ItemsRequestBytes]
/// This is the external data used for COSE_Sign1 verification of the reader's signature.
/// </summary>
public class ReaderAuthentication : ICborSerializable
{
    /// <summary>
    /// The session transcript bytes.
    /// </summary>
    public byte[] SessionTranscript { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The ItemsRequest bytes (tag-24 wrapped).
    /// </summary>
    public byte[] ItemsRequestBytes { get; set; } = Array.Empty<byte>();

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var array = CBORObject.NewArray();
        array.Add("ReaderAuthentication");
        array.Add(CBORObject.DecodeFromBytes(SessionTranscript));
        array.Add(CBORObject.FromObjectAndTag(ItemsRequestBytes, 24));

        return array;
    }

    /// <summary>
    /// Creates a ReaderAuthentication from CBOR bytes.
    /// </summary>
    /// <param name="cbor">The CBOR bytes.</param>
    /// <returns>A new ReaderAuthentication instance.</returns>
    public static ReaderAuthentication FromCbor(byte[] cbor)
    {
        return FromCborObject(CBORObject.DecodeFromBytes(cbor));
    }

    /// <summary>
    /// Creates a ReaderAuthentication from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new ReaderAuthentication instance.</returns>
    public static ReaderAuthentication FromCborObject(CBORObject cbor)
    {
        var auth = new ReaderAuthentication();

        if (cbor.Count > 1)
        {
            auth.SessionTranscript = cbor[1].EncodeToBytes();
        }

        if (cbor.Count > 2)
        {
            var itemsReqCbor = cbor[2];
            if (itemsReqCbor.HasMostOuterTag(24))
            {
                auth.ItemsRequestBytes = itemsReqCbor.UntagOne().GetByteString();
            }
            else
            {
                auth.ItemsRequestBytes = itemsReqCbor.EncodeToBytes();
            }
        }

        return auth;
    }
}
