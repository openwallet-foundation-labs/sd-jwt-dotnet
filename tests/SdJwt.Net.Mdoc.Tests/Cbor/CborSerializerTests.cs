using FluentAssertions;
using SdJwt.Net.Mdoc.Cbor;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Cbor;

/// <summary>
/// Tests for CBOR serialization interface and utilities.
/// </summary>
public class CborSerializerTests
{
    [Fact]
    public void Serialize_WithString_ReturnsCborBytes()
    {
        // Arrange
        var value = "test string";

        // Act
        var result = CborUtils.SerializeString(value);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Serialize_WithByteArray_ReturnsCborBytes()
    {
        // Arrange
        var value = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var result = CborUtils.SerializeBytes(value);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Deserialize_WithCborString_ReturnsString()
    {
        // Arrange
        var original = "test string";
        var cborData = CborUtils.SerializeString(original);

        // Act
        var result = CborUtils.DeserializeString(cborData);

        // Assert
        result.Should().Be(original);
    }

    [Fact]
    public void Deserialize_WithCborBytes_ReturnsByteArray()
    {
        // Arrange
        var original = new byte[] { 0x01, 0x02, 0x03 };
        var cborData = CborUtils.SerializeBytes(original);

        // Act
        var result = CborUtils.DeserializeBytes(cborData);

        // Assert
        result.Should().BeEquivalentTo(original);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Serialize_WithInteger_RoundTrips(int value)
    {
        // Act
        var cborData = CborUtils.SerializeInt(value);
        var result = CborUtils.DeserializeInt(cborData);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Serialize_WithNullString_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CborUtils.SerializeString(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Serialize_WithNullBytes_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CborUtils.SerializeBytes(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_WithNullData_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CborUtils.DeserializeString(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_WithEmptyData_ThrowsArgumentException()
    {
        // Act
        var act = () => CborUtils.DeserializeString(Array.Empty<byte>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Serialize_WithDateTimeOffset_RoundTrips()
    {
        // Arrange
        var value = new DateTimeOffset(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);

        // Act
        var cborData = CborUtils.SerializeDateTimeOffset(value);
        var result = CborUtils.DeserializeDateTimeOffset(cborData);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Serialize_WithBool_RoundTrips()
    {
        // Act
        var trueData = CborUtils.SerializeBool(true);
        var falseData = CborUtils.SerializeBool(false);
        var trueResult = CborUtils.DeserializeBool(trueData);
        var falseResult = CborUtils.DeserializeBool(falseData);

        // Assert
        trueResult.Should().BeTrue();
        falseResult.Should().BeFalse();
    }
}
