using FluentAssertions;
using SdJwt.Net.StatusList.Models;
using Xunit;
using StatusListModel = SdJwt.Net.StatusList.Models.StatusList;

namespace SdJwt.Net.StatusList.Tests.Models;

public class StatusListDataTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldCreateInstance()
    {
        // Act
        var statusListData = new StatusListData();

        // Assert
        statusListData.Should().NotBeNull();
        statusListData.Bits.Should().Be(0);
        statusListData.Data.Should().BeNull();
        statusListData.Count.Should().Be(0);
    }

    [Fact]
    public void Count_WithValidDataAndBits_ShouldCalculateCorrectly()
    {
        // Arrange
        var data = new byte[] { 0xFF, 0xFF }; // 16 bits total
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = data
        };

        // Act & Assert
        statusListData.Count.Should().Be(16); // 16 bits / 1 bit per entry = 16 entries
    }

    [Fact]
    public void Count_WithDifferentBitSizes_ShouldCalculateCorrectly()
    {
        // Test 2 bits per entry
        var data = new byte[] { 0xFF, 0xFF }; // 16 bits total
        var statusListData = new StatusListData
        {
            Bits = 2,
            Data = data
        };
        statusListData.Count.Should().Be(8); // 16 bits / 2 bits per entry = 8 entries

        // Test 4 bits per entry
        statusListData.Bits = 4;
        statusListData.Count.Should().Be(4); // 16 bits / 4 bits per entry = 4 entries

        // Test 8 bits per entry
        statusListData.Bits = 8;
        statusListData.Count.Should().Be(2); // 16 bits / 8 bits per entry = 2 entries
    }

    [Fact]
    public void Count_WithNullData_ShouldReturnZero()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = null
        };

        // Act & Assert
        statusListData.Count.Should().Be(0);
    }

    [Fact]
    public void Count_WithZeroBits_ShouldReturnZero()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 0,
            Data = new byte[] { 0xFF }
        };

        // Act & Assert
        statusListData.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void Validate_WithValidBits_ShouldNotThrow(int bits)
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = bits,
            Data = new byte[] { 0x00 }
        };

        // Act & Assert
        var act = () => statusListData.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(9)]
    [InlineData(16)]
    public void Validate_WithInvalidBits_ShouldThrow(int invalidBits)
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = invalidBits,
            Data = new byte[] { 0x00 }
        };

        // Act & Assert
        var act = () => statusListData.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Bits must be 1, 2, 4, or 8");
    }

    [Fact]
    public void Validate_WithNullData_ShouldThrow()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = null
        };

        // Act & Assert
        var act = () => statusListData.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data cannot be null");
    }

    [Fact]
    public void Validate_WithEmptyData_ShouldThrow()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = Array.Empty<byte>()
        };

        // Act & Assert
        var act = () => statusListData.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data cannot be empty");
    }

    [Fact]
    public void GetStatus_WithValidIndex_ShouldReturnCorrectValue()
    {
        // Arrange
        var data = new byte[] { 0b10101010 }; // Alternating bits
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = data
        };

        // Act & Assert
        statusListData.GetStatus(0).Should().Be(0); // First bit is 0
        statusListData.GetStatus(1).Should().Be(1); // Second bit is 1
        statusListData.GetStatus(2).Should().Be(0); // Third bit is 0
        statusListData.GetStatus(3).Should().Be(1); // Fourth bit is 1
    }

    [Fact]
    public void GetStatus_With2BitValues_ShouldReturnCorrectValue()
    {
        // Arrange - byte 0b11001010 with 2 bits per status
        var data = new byte[] { 0b11001010 };
        var statusListData = new StatusListData
        {
            Bits = 2,
            Data = data
        };

        // Act & Assert
        // First 2 bits: 10 = 2
        statusListData.GetStatus(0).Should().Be(2);
        // Next 2 bits: 10 = 2  
        statusListData.GetStatus(1).Should().Be(2);
        // Next 2 bits: 00 = 0
        statusListData.GetStatus(2).Should().Be(0);
        // Last 2 bits: 11 = 3
        statusListData.GetStatus(3).Should().Be(3);
    }

    [Fact]
    public void GetStatus_WithNullData_ShouldThrow()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = null
        };

        // Act & Assert
        var act = () => statusListData.GetStatus(0);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data is null");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void GetStatus_WithInvalidIndex_ShouldThrow(int invalidIndex)
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = new byte[] { 0xFF }
        };

        // Act & Assert
        var act = () => statusListData.GetStatus(invalidIndex);
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("index");
    }

    [Fact]
    public void SetStatus_WithValidValues_ShouldUpdateData()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = new byte[] { 0x00 }
        };

        // Act
        statusListData.SetStatus(0, 1);
        statusListData.SetStatus(2, 1);
        statusListData.SetStatus(4, 1);

        // Assert
        statusListData.GetStatus(0).Should().Be(1);
        statusListData.GetStatus(1).Should().Be(0);
        statusListData.GetStatus(2).Should().Be(1);
        statusListData.GetStatus(3).Should().Be(0);
        statusListData.GetStatus(4).Should().Be(1);
    }

    [Fact]
    public void SetStatus_With2BitValues_ShouldUpdateData()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 2,
            Data = new byte[] { 0x00 }
        };

        // Act
        statusListData.SetStatus(0, 3); // 11 in binary
        statusListData.SetStatus(1, 1); // 01 in binary
        statusListData.SetStatus(2, 2); // 10 in binary

        // Assert
        statusListData.GetStatus(0).Should().Be(3);
        statusListData.GetStatus(1).Should().Be(1);
        statusListData.GetStatus(2).Should().Be(2);
        statusListData.GetStatus(3).Should().Be(0);
    }

    [Fact]
    public void SetStatus_WithNullData_ShouldThrow()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = null
        };

        // Act & Assert
        var act = () => statusListData.SetStatus(0, 1);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data is null");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void SetStatus_WithInvalidIndex_ShouldThrow(int invalidIndex)
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = new byte[] { 0xFF }
        };

        // Act & Assert
        var act = () => statusListData.SetStatus(invalidIndex, 1);
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("index");
    }

    [Fact]
    public void SetStatus_WithValueTooLarge_ShouldThrow()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 1,
            Data = new byte[] { 0x00 }
        };

        // Act & Assert
        var act = () => statusListData.SetStatus(0, 2); // Max value for 1 bit is 1
        act.Should().Throw<ArgumentException>()
           .WithMessage("*exceeds maximum*")
           .WithParameterName("statusValue");
    }

    [Fact]
    public void SetStatus_With2BitValueTooLarge_ShouldThrow()
    {
        // Arrange
        var statusListData = new StatusListData
        {
            Bits = 2,
            Data = new byte[] { 0x00 }
        };

        // Act & Assert
        var act = () => statusListData.SetStatus(0, 4); // Max value for 2 bits is 3
        act.Should().Throw<ArgumentException>()
           .WithMessage("*exceeds maximum*")
           .WithParameterName("statusValue");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidCapacity_ShouldThrow(int invalidCapacity)
    {
        // Act & Assert
        var act = () => StatusListData.Create(invalidCapacity, 1);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Capacity must be positive*")
           .WithParameterName("capacity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(9)]
    public void Create_WithInvalidBits_ShouldThrow(int invalidBits)
    {
        // Act & Assert
        var act = () => StatusListData.Create(100, invalidBits);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Bits must be 1, 2, 4, or 8*")
           .WithParameterName("bits");
    }

    [Theory]
    [InlineData(1, 1, 8)]  // 1 capacity needs 1 byte, which can hold 8 entries
    [InlineData(8, 1, 8)]  // 8 capacity needs 1 byte, which can hold 8 entries  
    [InlineData(9, 2, 16)] // 9 capacity needs 2 bytes, which can hold 16 entries
    [InlineData(16, 2, 16)] // 16 capacity needs 2 bytes, which can hold 16 entries
    [InlineData(17, 3, 24)] // 17 capacity needs 3 bytes, which can hold 24 entries
    public void Create_WithValidParameters_ShouldCalculateCorrectByteSize(int capacity, int expectedBytes, int expectedCount)
    {
        // Act
        var statusListData = StatusListData.Create(capacity, 1);

        // Assert
        statusListData.Should().NotBeNull();
        statusListData.Bits.Should().Be(1);
        statusListData.Data.Should().NotBeNull();
        statusListData.Data!.Length.Should().Be(expectedBytes);
        statusListData.Count.Should().Be(expectedCount); // Count reflects actual storage capacity, not input capacity
    }

    [Fact]
    public void Create_With2BitsPerEntry_ShouldCalculateCorrectByteSize()
    {
        // Test various capacities with 2 bits per entry
        var statusListData = StatusListData.Create(4, 2); // 4 entries * 2 bits = 8 bits = 1 byte
        statusListData.Data!.Length.Should().Be(1);
        statusListData.Count.Should().Be(4); // With 2 bits per entry, 1 byte (8 bits) can hold 4 entries

        statusListData = StatusListData.Create(5, 2); // 5 entries * 2 bits = 10 bits = 2 bytes (rounded up)
        statusListData.Data!.Length.Should().Be(2);
        statusListData.Count.Should().Be(8); // With 2 bits per entry, 2 bytes (16 bits) can hold 8 entries, not 5
    }

    [Fact]
    public void Create_WithLargeCapacity_ShouldWork()
    {
        // Arrange & Act
        var statusListData = StatusListData.Create(100000, 1);

        // Assert
        statusListData.Should().NotBeNull();
        statusListData.Bits.Should().Be(1);
        statusListData.Count.Should().Be(100000);
        statusListData.Data.Should().NotBeNull();
        statusListData.Data!.Length.Should().Be(12500); // 100000 bits / 8 = 12500 bytes
    }

    [Fact]
    public void RoundTripTest_SetAndGetStatus_ShouldMaintainValues()
    {
        // Arrange
        var statusListData = StatusListData.Create(1000, 2);

        // Act & Assert - Test setting and getting various values
        for (int i = 0; i < 1000; i++)
        {
            byte expectedValue = (byte)(i % 4); // Values 0-3 for 2-bit entries
            statusListData.SetStatus(i, expectedValue);
            statusListData.GetStatus(i).Should().Be(expectedValue);
        }
    }
}

public class StatusListTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldCreateInstance()
    {
        // Act
        var statusList = new StatusListModel();

        // Assert
        statusList.Should().NotBeNull();
        statusList.Bits.Should().Be(0);
        statusList.List.Should().Be(string.Empty);
        statusList.AggregationUri.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var bits = 1;
        var list = "eNrbuRgAAhcBIA";
        var aggregationUri = "https://example.com/aggregation";

        // Act
        var statusList = new StatusListModel
        {
            Bits = bits,
            List = list,
            AggregationUri = aggregationUri
        };

        // Assert
        statusList.Should().NotBeNull();
        statusList.Bits.Should().Be(bits);
        statusList.List.Should().Be(list);
        statusList.AggregationUri.Should().Be(aggregationUri);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void Bits_WithValidValues_ShouldSetCorrectly(int validBits)
    {
        // Act
        var statusList = new StatusListModel { Bits = validBits };

        // Assert
        statusList.Bits.Should().Be(validBits);
    }

    [Fact]
    public void List_WithBase64EncodedData_ShouldSetCorrectly()
    {
        // Arrange
        var base64Data = "eNrbuRgAAhcBIA=="; // Example base64url encoded data

        // Act
        var statusList = new StatusListModel { List = base64Data };

        // Assert
        statusList.List.Should().Be(base64Data);
    }

    [Fact]
    public void AggregationUri_WithValidUri_ShouldSetCorrectly()
    {
        // Arrange
        var uri = "https://example.com/status/aggregation";

        // Act
        var statusList = new StatusListModel { AggregationUri = uri };

        // Assert
        statusList.AggregationUri.Should().Be(uri);
    }

    [Fact]
    public void AggregationUri_WithNull_ShouldAllowNull()
    {
        // Act
        var statusList = new StatusListModel { AggregationUri = null };

        // Assert
        statusList.AggregationUri.Should().BeNull();
    }
}

public class StatusListReferenceTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldCreateInstance()
    {
        // Act
        var reference = new StatusListReference();

        // Assert
        reference.Should().NotBeNull();
        reference.Uri.Should().Be(string.Empty);
        reference.Index.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var uri = "https://example.com/status/123";
        var index = 42;

        // Act
        var reference = new StatusListReference
        {
            Uri = uri,
            Index = index
        };

        // Assert
        reference.Should().NotBeNull();
        reference.Uri.Should().Be(uri);
        reference.Index.Should().Be(index);
    }

    [Fact]
    public void Validate_WithValidReference_ShouldNotThrow()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "https://example.com/status/123",
            Index = 42
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidUri_ShouldThrow(string? invalidUri)
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = invalidUri!,
            Index = 42
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*URI*required*");
    }

    [Fact]
    public void Validate_WithNegativeIndex_ShouldThrow()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "https://example.com/status/123",
            Index = -1
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Index*non-negative*");
    }

    [Fact]
    public void Validate_WithZeroIndex_ShouldNotThrow()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "https://example.com/status/123",
            Index = 0
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().NotThrow();
    }
}

public class StatusClaimTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldCreateInstance()
    {
        // Act
        var claim = new StatusClaim();

        // Assert
        claim.Should().NotBeNull();
        claim.StatusList.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValidStatusList_ShouldCreateInstance()
    {
        // Arrange
        var statusList = new StatusListReference
        {
            Uri = "https://example.com/status/123",
            Index = 42
        };

        // Act
        var claim = new StatusClaim { StatusList = statusList };

        // Assert
        claim.Should().NotBeNull();
        claim.StatusList.Should().Be(statusList);
    }

    [Fact]
    public void Validate_WithValidStatusList_ShouldNotThrow()
    {
        // Arrange
        var claim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = "https://example.com/status/123",
                Index = 42
            }
        };

        // Act & Assert
        var act = () => claim.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullStatusList_ShouldThrow()
    {
        // Arrange
        var claim = new StatusClaim { StatusList = null };

        // Act & Assert
        var act = () => claim.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*StatusList*required*");
    }

    [Fact]
    public void Validate_WithInvalidStatusList_ShouldThrow()
    {
        // Arrange
        var claim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = "", // Invalid URI
                Index = 42
            }
        };

        // Act & Assert
        var act = () => claim.Validate();
        act.Should().Throw<InvalidOperationException>();
    }
}

public class StatusCheckResultTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldCreateInstance()
    {
        // Act
        var result = new StatusCheckResult();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid);
        result.StatusValue.Should().Be(0);
        result.IsValid.Should().BeTrue(); // Based on Status property
        result.IsActive.Should().BeTrue(); // Based on Status property
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var result = new StatusCheckResult
        {
            Status = StatusType.Invalid,
            StatusValue = 1,
            ErrorMessage = "Test error"
        };

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
        result.StatusValue.Should().Be(1);
        result.IsValid.Should().BeFalse(); // Based on Status property
        result.IsActive.Should().BeFalse(); // Based on Status property
        result.ErrorMessage.Should().Be("Test error");
    }

    [Fact]
    public void Success_ShouldCreateValidActiveResult()
    {
        // Act
        var result = StatusCheckResult.Success();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid);
        result.StatusValue.Should().Be(0);
        result.IsValid.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Revoked_ShouldCreateValidInactiveResult()
    {
        // Act
        var result = StatusCheckResult.Revoked();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
        result.StatusValue.Should().Be(1);
        result.IsValid.Should().BeFalse();
        result.IsActive.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Suspended_ShouldCreateValidInactiveResult()
    {
        // Act
        var result = StatusCheckResult.Suspended();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Suspended);
        result.StatusValue.Should().Be(2);
        result.IsValid.Should().BeFalse(); // Suspended is not valid
        result.IsActive.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failed_ShouldCreateInvalidInactiveResultWithMessage()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = StatusCheckResult.Failed(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
        result.StatusValue.Should().Be(-1);
        result.IsValid.Should().BeFalse();
        result.IsActive.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void Failed_WithNullMessage_ShouldCreateInvalidResult()
    {
        // Act
        var result = StatusCheckResult.Failed(null!);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
        result.StatusValue.Should().Be(-1);
        result.IsValid.Should().BeFalse();
        result.IsActive.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
    }
}

public class StatusTypeExtensionsTests
{
    [Fact]
    public void ToStringValue_WithValidStatus_ShouldReturnCorrectString()
    {
        // Assert
        StatusType.Valid.ToStringValue().Should().Be("valid");
        StatusType.Invalid.ToStringValue().Should().Be("invalid");
        StatusType.Suspended.ToStringValue().Should().Be("suspended");
        StatusType.UnderInvestigation.ToStringValue().Should().Be("under_investigation");
    }

    [Fact]
    public void FromString_WithValidStrings_ShouldReturnCorrectEnum()
    {
        // Assert
        StatusTypeExtensions.FromString("valid").Should().Be(StatusType.Valid);
        StatusTypeExtensions.FromString("invalid").Should().Be(StatusType.Invalid);
        StatusTypeExtensions.FromString("suspended").Should().Be(StatusType.Suspended);
        StatusTypeExtensions.FromString("under_investigation").Should().Be(StatusType.UnderInvestigation);
    }

    [Fact]
    public void FromString_WithCaseVariations_ShouldReturnCorrectEnum()
    {
        // Assert - test case insensitivity
        StatusTypeExtensions.FromString("VALID").Should().Be(StatusType.Valid);
        StatusTypeExtensions.FromString("Valid").Should().Be(StatusType.Valid);
        StatusTypeExtensions.FromString("INVALID").Should().Be(StatusType.Invalid);
        StatusTypeExtensions.FromString("Invalid").Should().Be(StatusType.Invalid);
        StatusTypeExtensions.FromString("SUSPENDED").Should().Be(StatusType.Suspended);
        StatusTypeExtensions.FromString("Suspended").Should().Be(StatusType.Suspended);
        StatusTypeExtensions.FromString("UNDER_INVESTIGATION").Should().Be(StatusType.UnderInvestigation);
        StatusTypeExtensions.FromString("Under_Investigation").Should().Be(StatusType.UnderInvestigation);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("unknown")]
    [InlineData("active")]
    [InlineData("revoked")]
    public void FromString_WithInvalidStrings_ShouldThrow(string? invalidString)
    {
        // Act & Assert
        var act = () => StatusTypeExtensions.FromString(invalidString!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RoundTrip_AllStatusTypes_ShouldMaintainValue()
    {
        // Arrange
        var allStatusTypes = Enum.GetValues<StatusType>();

        // Act & Assert
        foreach (var statusType in allStatusTypes)
        {
            var stringValue = statusType.ToStringValue();
            var parsedBack = StatusTypeExtensions.FromString(stringValue);
            parsedBack.Should().Be(statusType);
        }
    }
}
