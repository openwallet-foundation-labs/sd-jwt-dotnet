using PeterO.Cbor;

namespace SdJwt.Net.Mdoc.Cbor;

/// <summary>
/// Interface for types that can be serialized to/from CBOR.
/// </summary>
public interface ICborSerializable
{
    /// <summary>
    /// Serializes this object to CBOR bytes.
    /// </summary>
    /// <returns>The CBOR-encoded bytes.</returns>
    byte[] ToCbor();

    /// <summary>
    /// Gets the CBOR object representation.
    /// </summary>
    /// <returns>The CBORObject representation.</returns>
    CBORObject ToCborObject();
}
