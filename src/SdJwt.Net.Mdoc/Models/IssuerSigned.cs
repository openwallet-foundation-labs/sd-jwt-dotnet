using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// Individual data element in IssuerSigned.
/// </summary>
public class IssuerSignedItem : ICborSerializable
{
    /// <summary>
    /// The digest ID for this item.
    /// </summary>
    public int DigestId
    {
        get; set;
    }

    /// <summary>
    /// Random bytes for digest computation (salt).
    /// </summary>
    public byte[] Random { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The element identifier (claim name).
    /// </summary>
    public string ElementIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The element value.
    /// </summary>
    public object? ElementValue
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
        var cbor = CBORObject.NewMap();
        cbor.Add("digestID", DigestId);
        cbor.Add("random", Random);
        cbor.Add("elementIdentifier", ElementIdentifier);
        cbor.Add("elementValue", CBORObject.FromObject(ElementValue));
        return cbor;
    }

    /// <summary>
    /// Creates an IssuerSignedItem from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new IssuerSignedItem instance.</returns>
    public static IssuerSignedItem FromCborObject(CBORObject cbor)
    {
        var item = new IssuerSignedItem
        {
            DigestId = cbor["digestID"].AsInt32(),
            Random = cbor["random"].GetByteString(),
            ElementIdentifier = cbor["elementIdentifier"].AsString()
        };

        var elementValue = cbor["elementValue"];
        item.ElementValue = ConvertCborValue(elementValue);

        return item;
    }

    private static object? ConvertCborValue(CBORObject cbor)
    {
        if (cbor.IsNull)
            return null;

        return cbor.Type switch
        {
            CBORType.Boolean => cbor.AsBoolean(),
            CBORType.Integer => cbor.AsInt32(),
            CBORType.FloatingPoint => cbor.AsDouble(),
            CBORType.TextString => cbor.AsString(),
            CBORType.ByteString => cbor.GetByteString(),
            _ => cbor
        };
    }
}

/// <summary>
/// IssuerSigned structure containing namespaced data elements and issuer authentication.
/// </summary>
public class IssuerSigned : ICborSerializable
{
    /// <summary>
    /// Namespaced data elements.
    /// Key: namespace, Value: list of IssuerSignedItems.
    /// </summary>
    public Dictionary<string, List<IssuerSignedItem>> NameSpaces { get; set; } = new();

    /// <summary>
    /// Issuer authentication (COSE_Sign1 containing MSO).
    /// </summary>
    public byte[] IssuerAuth { get; set; } = Array.Empty<byte>();

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();

        var nameSpacesCbor = CBORObject.NewMap();
        foreach (var (nameSpace, items) in NameSpaces)
        {
            var itemsArray = CBORObject.NewArray();
            foreach (var item in items)
            {
                // Each item is tagged with 24 (encoded CBOR data item)
                var itemCbor = CBORObject.FromObjectAndTag(item.ToCbor(), 24);
                itemsArray.Add(itemCbor);
            }
            nameSpacesCbor.Add(nameSpace, itemsArray);
        }
        cbor.Add("nameSpaces", nameSpacesCbor);

        if (IssuerAuth.Length > 0)
        {
            cbor.Add("issuerAuth", CBORObject.DecodeFromBytes(IssuerAuth));
        }

        return cbor;
    }

    /// <summary>
    /// Creates an IssuerSigned from CBOR bytes.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded data.</param>
    /// <returns>A new IssuerSigned instance.</returns>
    public static IssuerSigned FromCbor(byte[] cborData)
    {
        if (cborData == null)
            throw new ArgumentNullException(nameof(cborData));
        var cbor = CBORObject.DecodeFromBytes(cborData);
        return FromCborObject(cbor);
    }

    /// <summary>
    /// Creates an IssuerSigned from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new IssuerSigned instance.</returns>
    public static IssuerSigned FromCborObject(CBORObject cbor)
    {
        var issuerSigned = new IssuerSigned();

        if (cbor.ContainsKey("nameSpaces"))
        {
            var nameSpacesCbor = cbor["nameSpaces"];
            foreach (var key in nameSpacesCbor.Keys)
            {
                var nameSpace = key.AsString();
                var items = new List<IssuerSignedItem>();

                var itemsArray = nameSpacesCbor[key];
                foreach (var itemCbor in itemsArray.Values)
                {
                    // Unwrap tag 24 if present
                    var actualCbor = itemCbor.HasMostOuterTag(24)
                        ? CBORObject.DecodeFromBytes(itemCbor.GetByteString())
                        : itemCbor;

                    items.Add(IssuerSignedItem.FromCborObject(actualCbor));
                }

                issuerSigned.NameSpaces[nameSpace] = items;
            }
        }

        if (cbor.ContainsKey("issuerAuth"))
        {
            issuerSigned.IssuerAuth = cbor["issuerAuth"].EncodeToBytes();
        }

        return issuerSigned;
    }
}
