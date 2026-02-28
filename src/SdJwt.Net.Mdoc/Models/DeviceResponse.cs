using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// DeviceResponse structure per ISO 18013-5.
/// Contains one or more documents presented by the holder.
/// </summary>
public class DeviceResponse : ICborSerializable
{
    /// <summary>
    /// Version of the DeviceResponse structure.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Documents included in the response.
    /// </summary>
    public List<Document> Documents { get; set; } = new();

    /// <summary>
    /// Document errors for documents that could not be returned.
    /// </summary>
    public List<DocumentError>? DocumentErrors { get; set; }

    /// <summary>
    /// Overall status code.
    /// </summary>
    public int Status { get; set; } = 0;

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();
        cbor.Add("version", Version);

        var docsArray = CBORObject.NewArray();
        foreach (var doc in Documents)
        {
            docsArray.Add(doc.ToCborObject());
        }
        cbor.Add("documents", docsArray);

        if (DocumentErrors != null && DocumentErrors.Count > 0)
        {
            var errorsArray = CBORObject.NewArray();
            foreach (var error in DocumentErrors)
            {
                var errorMap = CBORObject.NewMap();
                errorMap.Add(error.DocType, error.ErrorCode);
                errorsArray.Add(errorMap);
            }
            cbor.Add("documentErrors", errorsArray);
        }

        cbor.Add("status", Status);

        return cbor;
    }

    /// <summary>
    /// Creates a DeviceResponse from CBOR bytes.
    /// </summary>
    /// <param name="cbor">The CBOR bytes.</param>
    /// <returns>A new DeviceResponse instance.</returns>
    public static DeviceResponse FromCbor(byte[] cbor)
    {
        return FromCborObject(CBORObject.DecodeFromBytes(cbor));
    }

    /// <summary>
    /// Creates a DeviceResponse from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new DeviceResponse instance.</returns>
    public static DeviceResponse FromCborObject(CBORObject cbor)
    {
        var response = new DeviceResponse();

        if (cbor.ContainsKey("version"))
        {
            response.Version = cbor["version"].AsString();
        }

        if (cbor.ContainsKey("documents"))
        {
            foreach (var docCbor in cbor["documents"].Values)
            {
                response.Documents.Add(Document.FromCborObject(docCbor));
            }
        }

        if (cbor.ContainsKey("documentErrors"))
        {
            response.DocumentErrors = new List<DocumentError>();
            foreach (var errorCbor in cbor["documentErrors"].Values)
            {
                foreach (var key in errorCbor.Keys)
                {
                    response.DocumentErrors.Add(new DocumentError
                    {
                        DocType = key.AsString(),
                        ErrorCode = errorCbor[key].AsInt32()
                    });
                }
            }
        }

        if (cbor.ContainsKey("status"))
        {
            response.Status = cbor["status"].AsInt32();
        }

        return response;
    }
}
