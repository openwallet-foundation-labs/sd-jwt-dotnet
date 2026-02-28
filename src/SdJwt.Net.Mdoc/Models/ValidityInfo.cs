using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// Validity information per ISO 18013-5 Section 9.1.2.4.
/// </summary>
public class ValidityInfo : ICborSerializable
{
    /// <summary>
    /// Timestamp when the MSO was signed.
    /// </summary>
    public DateTimeOffset Signed { get; set; }

    /// <summary>
    /// Timestamp from which the MSO is valid.
    /// </summary>
    public DateTimeOffset ValidFrom { get; set; }

    /// <summary>
    /// Timestamp until which the MSO is valid.
    /// </summary>
    public DateTimeOffset ValidUntil { get; set; }

    /// <summary>
    /// Optional expected update timestamp.
    /// </summary>
    public DateTimeOffset? ExpectedUpdate { get; set; }

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();
        cbor.Add("signed", CBORObject.FromObjectAndTag(Signed.ToString("yyyy-MM-ddTHH:mm:ssZ"), 0));
        cbor.Add("validFrom", CBORObject.FromObjectAndTag(ValidFrom.ToString("yyyy-MM-ddTHH:mm:ssZ"), 0));
        cbor.Add("validUntil", CBORObject.FromObjectAndTag(ValidUntil.ToString("yyyy-MM-ddTHH:mm:ssZ"), 0));

        if (ExpectedUpdate.HasValue)
        {
            cbor.Add("expectedUpdate", CBORObject.FromObjectAndTag(ExpectedUpdate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"), 0));
        }

        return cbor;
    }

    /// <summary>
    /// Creates a ValidityInfo from CBOR.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new ValidityInfo instance.</returns>
    public static ValidityInfo FromCborObject(CBORObject cbor)
    {
        var info = new ValidityInfo
        {
            Signed = DateTimeOffset.Parse(cbor["signed"].AsString()),
            ValidFrom = DateTimeOffset.Parse(cbor["validFrom"].AsString()),
            ValidUntil = DateTimeOffset.Parse(cbor["validUntil"].AsString())
        };

        if (cbor.ContainsKey("expectedUpdate"))
        {
            info.ExpectedUpdate = DateTimeOffset.Parse(cbor["expectedUpdate"].AsString());
        }

        return info;
    }
}
