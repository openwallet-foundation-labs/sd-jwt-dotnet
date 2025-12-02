using System.Text.Json.Serialization;

namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents the Status List data structure within the Status List Token.
/// Contains status information of many Referenced Tokens represented by one or multiple bits.
/// </summary>
public class StatusList
{
    /// <summary>
    /// Gets or sets the number of bits per Referenced Token in the compressed byte array.
    /// Required. The allowed values for bits are 1, 2, 4 and 8.
    /// </summary>
    [JsonPropertyName("bits")]
    public int Bits { get; set; }

    /// <summary>
    /// Gets or sets the status values for all Referenced Tokens.
    /// Required. Base64url-encoded compressed byte array as specified in the specification.
    /// </summary>
    [JsonPropertyName("lst")]
    public string List { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the aggregation URI.
    /// Optional. URI to retrieve the Status List Aggregation for this type of Referenced Token or Issuer.
    /// </summary>
    [JsonPropertyName("aggregation_uri")]
    public string? AggregationUri { get; set; }
}

/// <summary>
/// Represents raw status list data for testing and advanced scenarios.
/// Contains uncompressed status values and associated metadata.
/// </summary>
public class StatusListData
{
    /// <summary>
    /// Gets or sets the number of bits per status value.
    /// Must be 1, 2, 4, or 8.
    /// </summary>
    public int Bits { get; set; }

    /// <summary>
    /// Gets or sets the raw status data as a byte array.
    /// Each status value is encoded according to the Bits property.
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// Gets the number of status entries in this data.
    /// </summary>
    public int Count
    {
        get
        {
            if (Data == null || Bits == 0)
                return 0;

            return (Data.Length * 8) / Bits;
        }
    }

    /// <summary>
    /// Validates the status list data.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
    public void Validate()
    {
        if (Bits != 1 && Bits != 2 && Bits != 4 && Bits != 8)
            throw new InvalidOperationException("Bits must be 1, 2, 4, or 8");

        if (Data == null)
            throw new InvalidOperationException("Data cannot be null");

        if (Data.Length == 0)
            throw new InvalidOperationException("Data cannot be empty");
    }

    /// <summary>
    /// Gets the status value at the specified index.
    /// </summary>
    /// <param name="index">The index to retrieve</param>
    /// <returns>The status value</returns>
    public byte GetStatus(int index)
    {
        if (Data == null)
            throw new InvalidOperationException("Data is null");

        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var bitIndex = index * Bits;
        byte statusValue = 0;

        for (int bit = 0; bit < Bits; bit++)
        {
            var globalBitIndex = bitIndex + bit;
            var byteIndex = globalBitIndex / 8;
            var bitInByte = globalBitIndex % 8;

            if (byteIndex < Data.Length &&
                (Data[byteIndex] & (1 << bitInByte)) != 0)
            {
                statusValue |= (byte)(1 << bit);
            }
        }

        return statusValue;
    }

    /// <summary>
    /// Sets the status value at the specified index.
    /// </summary>
    /// <param name="index">The index to set</param>
    /// <param name="statusValue">The status value to set</param>
    public void SetStatus(int index, byte statusValue)
    {
        if (Data == null)
            throw new InvalidOperationException("Data is null");

        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var maxValue = (1 << Bits) - 1;
        if (statusValue > maxValue)
            throw new ArgumentException($"Status value {statusValue} exceeds maximum for {Bits} bits ({maxValue})", nameof(statusValue));

        var bitIndex = index * Bits;

        for (int bit = 0; bit < Bits; bit++)
        {
            var globalBitIndex = bitIndex + bit;
            var byteIndex = globalBitIndex / 8;
            var bitInByte = globalBitIndex % 8;

            if (byteIndex < Data.Length)
            {
                if ((statusValue & (1 << bit)) != 0)
                {
                    Data[byteIndex] |= (byte)(1 << bitInByte);
                }
                else
                {
                    Data[byteIndex] &= (byte)~(1 << bitInByte);
                }
            }
        }
    }

    /// <summary>
    /// Creates a new StatusListData with the specified capacity.
    /// </summary>
    /// <param name="capacity">The number of status entries</param>
    /// <param name="bits">The number of bits per entry</param>
    /// <returns>A new StatusListData instance</returns>
    public static StatusListData Create(int capacity, int bits)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));

        if (bits != 1 && bits != 2 && bits != 4 && bits != 8)
            throw new ArgumentException("Bits must be 1, 2, 4, or 8", nameof(bits));

        var totalBits = capacity * bits;
        var byteCount = (totalBits + 7) / 8; // Round up to nearest byte

        return new StatusListData
        {
            Bits = bits,
            Data = new byte[byteCount]
        };
    }
}