using PeterO.Cbor;

namespace SdJwt.Net.Mdoc.Cbor;

/// <summary>
/// CBOR serialization utilities for mdoc-specific structures.
/// </summary>
public static class CborUtils
{
    /// <summary>
    /// Serializes a string to CBOR bytes.
    /// </summary>
    /// <param name="value">The string to serialize.</param>
    /// <returns>The CBOR-encoded bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static byte[] SerializeString(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        return CBORObject.FromObject(value).EncodeToBytes();
    }

    /// <summary>
    /// Serializes a byte array to CBOR bytes.
    /// </summary>
    /// <param name="value">The byte array to serialize.</param>
    /// <returns>The CBOR-encoded bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static byte[] SerializeBytes(byte[] value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        return CBORObject.FromObject(value).EncodeToBytes();
    }

    /// <summary>
    /// Serializes an integer to CBOR bytes.
    /// </summary>
    /// <param name="value">The integer to serialize.</param>
    /// <returns>The CBOR-encoded bytes.</returns>
    public static byte[] SerializeInt(int value)
    {
        return CBORObject.FromObject(value).EncodeToBytes();
    }

    /// <summary>
    /// Serializes a boolean to CBOR bytes.
    /// </summary>
    /// <param name="value">The boolean to serialize.</param>
    /// <returns>The CBOR-encoded bytes.</returns>
    public static byte[] SerializeBool(bool value)
    {
        return CBORObject.FromObject(value).EncodeToBytes();
    }

    /// <summary>
    /// Serializes a DateTimeOffset to CBOR bytes with tag 0 (date/time string).
    /// </summary>
    /// <param name="value">The DateTimeOffset to serialize.</param>
    /// <returns>The CBOR-encoded bytes.</returns>
    public static byte[] SerializeDateTimeOffset(DateTimeOffset value)
    {
        // ISO 8601 format per CBOR tag 0
        var dateString = value.ToString("yyyy-MM-ddTHH:mm:ssZ");
        return CBORObject.FromObjectAndTag(dateString, 0).EncodeToBytes();
    }

    /// <summary>
    /// Deserializes CBOR bytes to a string.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded bytes.</param>
    /// <returns>The deserialized string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cborData is null.</exception>
    /// <exception cref="ArgumentException">Thrown when cborData is empty.</exception>
    public static string DeserializeString(byte[] cborData)
    {
        if (cborData == null)
            throw new ArgumentNullException(nameof(cborData));
        if (cborData.Length == 0)
        {
            throw new ArgumentException("CBOR data cannot be empty.", nameof(cborData));
        }

        var cbor = CBORObject.DecodeFromBytes(cborData);
        return cbor.AsString();
    }

    /// <summary>
    /// Deserializes CBOR bytes to a byte array.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded bytes.</param>
    /// <returns>The deserialized byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cborData is null.</exception>
    /// <exception cref="ArgumentException">Thrown when cborData is empty.</exception>
    public static byte[] DeserializeBytes(byte[] cborData)
    {
        if (cborData == null)
            throw new ArgumentNullException(nameof(cborData));
        if (cborData.Length == 0)
        {
            throw new ArgumentException("CBOR data cannot be empty.", nameof(cborData));
        }

        var cbor = CBORObject.DecodeFromBytes(cborData);
        return cbor.GetByteString();
    }

    /// <summary>
    /// Deserializes CBOR bytes to an integer.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded bytes.</param>
    /// <returns>The deserialized integer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cborData is null.</exception>
    /// <exception cref="ArgumentException">Thrown when cborData is empty.</exception>
    public static int DeserializeInt(byte[] cborData)
    {
        if (cborData == null)
            throw new ArgumentNullException(nameof(cborData));
        if (cborData.Length == 0)
        {
            throw new ArgumentException("CBOR data cannot be empty.", nameof(cborData));
        }

        var cbor = CBORObject.DecodeFromBytes(cborData);
        return cbor.AsInt32();
    }

    /// <summary>
    /// Deserializes CBOR bytes to a boolean.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded bytes.</param>
    /// <returns>The deserialized boolean.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cborData is null.</exception>
    /// <exception cref="ArgumentException">Thrown when cborData is empty.</exception>
    public static bool DeserializeBool(byte[] cborData)
    {
        if (cborData == null)
            throw new ArgumentNullException(nameof(cborData));
        if (cborData.Length == 0)
        {
            throw new ArgumentException("CBOR data cannot be empty.", nameof(cborData));
        }

        var cbor = CBORObject.DecodeFromBytes(cborData);
        return cbor.AsBoolean();
    }

    /// <summary>
    /// Deserializes CBOR bytes to a DateTimeOffset.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded bytes.</param>
    /// <returns>The deserialized DateTimeOffset.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cborData is null.</exception>
    /// <exception cref="ArgumentException">Thrown when cborData is empty.</exception>
    public static DateTimeOffset DeserializeDateTimeOffset(byte[] cborData)
    {
        if (cborData == null)
            throw new ArgumentNullException(nameof(cborData));
        if (cborData.Length == 0)
        {
            throw new ArgumentException("CBOR data cannot be empty.", nameof(cborData));
        }

        var cbor = CBORObject.DecodeFromBytes(cborData);
        var dateString = cbor.AsString();
        return DateTimeOffset.Parse(dateString);
    }

    /// <summary>
    /// Deserializes CBOR bytes to the specified ICborSerializable type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="cborData">The CBOR-encoded bytes.</param>
    /// <returns>The deserialized object.</returns>
    public static T Deserialize<T>(byte[] cborData) where T : ICborSerializable, new()
    {
        if (cborData == null)
            throw new ArgumentNullException(nameof(cborData));
        if (cborData.Length == 0)
        {
            throw new ArgumentException("CBOR data cannot be empty.", nameof(cborData));
        }

        // For complex types, create a new instance and populate from CBOR
        var cbor = CBORObject.DecodeFromBytes(cborData);
        return FromCborObject<T>(cbor);
    }

    /// <summary>
    /// Creates an instance of T from a CBORObject.
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>The created instance.</returns>
    internal static T FromCborObject<T>(CBORObject cbor) where T : ICborSerializable, new()
    {
        // This is a placeholder - actual implementation depends on T
        return new T();
    }

    /// <summary>
    /// Serializes an object to CBOR bytes using tagged encoding.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The CBOR-encoded bytes.</returns>
    public static byte[] Serialize<T>(T value) where T : ICborSerializable
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        return value.ToCbor();
    }
}
