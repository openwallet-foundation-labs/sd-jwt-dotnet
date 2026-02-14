using FluentAssertions;
using SdJwt.Net.StatusList.Models;
using System;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Models;

public class StatusListExceptionTests
{
    [Fact]
    public void ConcurrencyException_WithMessage_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Concurrency conflict detected";

        // Act
        var exception = new ConcurrencyException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ConcurrencyException_WithMessageAndInnerException_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Concurrency conflict detected";
        var innerException = new InvalidOperationException("Operation failed");

        // Act
        var exception = new ConcurrencyException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ConcurrencyException_ShouldBeAssignableToException()
    {
        // Arrange
        var exception = new ConcurrencyException("test message");

        // Act & Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}

public class StatusListAdvancedTests
{
    [Fact]
    public void StatusType_Valid_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var statusType = StatusType.Valid;

        // Assert
        ((int)statusType).Should().Be(0x00);
        statusType.GetName().Should().Be("VALID");
        statusType.ToStringValue().Should().Be("valid");
    }

    [Fact]
    public void StatusType_Invalid_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var statusType = StatusType.Invalid;

        // Assert
        ((int)statusType).Should().Be(0x01);
        statusType.GetName().Should().Be("INVALID");
        statusType.ToStringValue().Should().Be("invalid");
    }

    [Fact]
    public void StatusType_Suspended_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var statusType = StatusType.Suspended;

        // Assert
        ((int)statusType).Should().Be(0x02);
        statusType.GetName().Should().Be("SUSPENDED");
        statusType.ToStringValue().Should().Be("suspended");
    }

    [Fact]
    public void StatusType_UnderInvestigation_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var statusType = StatusType.UnderInvestigation;

        // Assert
        ((int)statusType).Should().Be(0x03);
        statusType.GetName().Should().Be("UNDER_INVESTIGATION");
        statusType.ToStringValue().Should().Be("under_investigation");
    }

    [Fact]
    public void StatusTypeExtensions_FromString_ShouldParseCorrectly()
    {
        // Act & Assert
        StatusTypeExtensions.FromString("valid").Should().Be(StatusType.Valid);
        StatusTypeExtensions.FromString("invalid").Should().Be(StatusType.Invalid);
        StatusTypeExtensions.FromString("suspended").Should().Be(StatusType.Suspended);
        StatusTypeExtensions.FromString("under_investigation").Should().Be(StatusType.UnderInvestigation);
    }

    [Fact]
    public void StatusTypeExtensions_FromString_InvalidValue_ShouldThrow()
    {
        // Act & Assert
        var act = () => StatusTypeExtensions.FromString("unknown_status");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Unknown status type value: unknown_status*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void StatusTypeExtensions_FromString_NullOrEmpty_ShouldThrow(string? value)
    {
        // Act & Assert
        var act = () => StatusTypeExtensions.FromString(value!);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("value");
    }

    [Fact]
    public void StatusTypeExtensions_FromValue_ShouldParseCorrectly()
    {
        // Act & Assert
        StatusTypeExtensions.FromValue(0x00).Should().Be(StatusType.Valid);
        StatusTypeExtensions.FromValue(0x01).Should().Be(StatusType.Invalid);
        StatusTypeExtensions.FromValue(0x02).Should().Be(StatusType.Suspended);
        StatusTypeExtensions.FromValue(0x03).Should().Be(StatusType.UnderInvestigation);
    }

    [Fact]
    public void StatusTypeExtensions_GetDescription_ShouldReturnCorrectValues()
    {
        // Act & Assert
        StatusType.Valid.GetDescription().Should().Contain("valid, correct or legal");
        StatusType.Invalid.GetDescription().Should().Contain("revoked, annulled, taken back");
        StatusType.Suspended.GetDescription().Should().Contain("temporarily invalid");
        StatusType.UnderInvestigation.GetDescription().Should().Contain("under investigation");
    }

    [Fact]
    public void StatusCheckResult_Success_ShouldCreateValidResult()
    {
        // Act
        var result = StatusCheckResult.Success();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid);
        result.StatusValue.Should().Be(0x00);
        result.IsValid.Should().BeTrue();
        result.IsInvalid.Should().BeFalse();
        result.IsSuspended.Should().BeFalse();
        result.IsActive.Should().BeTrue();
        result.RetrievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void StatusCheckResult_Revoked_ShouldCreateInvalidResult()
    {
        // Act
        var result = StatusCheckResult.Revoked();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
        result.StatusValue.Should().Be(0x01);
        result.IsValid.Should().BeFalse();
        result.IsInvalid.Should().BeTrue();
        result.IsSuspended.Should().BeFalse();
        result.IsActive.Should().BeFalse();
        result.RetrievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void StatusCheckResult_Suspended_ShouldCreateSuspendedResult()
    {
        // Act
        var result = StatusCheckResult.Suspended();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Suspended);
        result.StatusValue.Should().Be(0x02);
        result.IsValid.Should().BeFalse();
        result.IsInvalid.Should().BeFalse();
        result.IsSuspended.Should().BeTrue();
        result.IsActive.Should().BeFalse();
        result.RetrievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void StatusCheckResult_Failed_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Network error occurred";

        // Act
        var result = StatusCheckResult.Failed(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
        result.StatusValue.Should().Be(-1);
        result.ErrorMessage.Should().Be(errorMessage);
        result.RetrievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void StatusListReference_PropertiesAccess_ShouldWorkCorrectly()
    {
        // Arrange
        var reference = new StatusListReference();

        // Act
        reference.Uri = "https://issuer.example.com/status-list/1";
        reference.Index = 123;

        // Assert
        reference.Uri.Should().Be("https://issuer.example.com/status-list/1");
        reference.Index.Should().Be(123);
    }

    [Fact]
    public void StatusListReference_Validate_ShouldValidateCorrectly()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "https://issuer.example.com/status-list/1",
            Index = 123
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void StatusListReference_Validate_InvalidUri_ShouldThrow(string? invalidUri)
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = invalidUri!,
            Index = 123
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Status list URI is required");
    }

    [Fact]
    public void StatusListReference_Validate_InvalidIndex_ShouldThrow()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "https://issuer.example.com/status-list/1",
            Index = -1
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Status list index must be non-negative");
    }

    [Fact]
    public void StatusListReference_Validate_NonAbsoluteUri_ShouldThrow()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "relative-uri",
            Index = 123
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Status list URI must be a valid absolute URI");
    }

    [Fact]
    public void StatusListData_Create_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var statusListData = StatusListData.Create(1000, 2);

        // Assert
        statusListData.Should().NotBeNull();
        statusListData.Bits.Should().Be(2);
        statusListData.Count.Should().Be(1000);
        statusListData.Data.Should().NotBeNull();
        statusListData.Data!.Length.Should().Be(250); // 1000 * 2 bits = 2000 bits = 250 bytes
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void StatusListData_Create_InvalidCapacity_ShouldThrow(int capacity)
    {
        // Act & Assert
        var act = () => StatusListData.Create(capacity, 2);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("capacity");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(16)]
    public void StatusListData_Create_InvalidBits_ShouldThrow(int bits)
    {
        // Act & Assert
        var act = () => StatusListData.Create(100, bits);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("bits");
    }

    [Fact]
    public void StatusListData_GetStatus_SetStatus_ShouldWorkCorrectly()
    {
        // Arrange
        var statusData = StatusListData.Create(10, 2);

        // Act
        statusData.SetStatus(0, 0); // Valid
        statusData.SetStatus(1, 1); // Invalid
        statusData.SetStatus(2, 2); // Suspended
        statusData.SetStatus(3, 3); // Under Investigation

        // Assert
        statusData.GetStatus(0).Should().Be(0);
        statusData.GetStatus(1).Should().Be(1);
        statusData.GetStatus(2).Should().Be(2);
        statusData.GetStatus(3).Should().Be(3);
    }

    [Fact]
    public void StatusListData_Validate_ShouldValidateCorrectly()
    {
        // Arrange
        var statusData = StatusListData.Create(10, 2);

        // Act & Assert
        var act = () => statusData.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void StatusListData_Validate_InvalidBits_ShouldThrow()
    {
        // Arrange
        var statusData = new StatusListData
        {
            Bits = 3, // Invalid
            Data = new byte[10]
        };

        // Act & Assert
        var act = () => statusData.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Bits must be 1, 2, 4, or 8");
    }

    [Fact]
    public void StatusListData_Validate_NullData_ShouldThrow()
    {
        // Arrange
        var statusData = new StatusListData
        {
            Bits = 2,
            Data = null
        };

        // Act & Assert
        var act = () => statusData.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data cannot be null");
    }

    [Fact]
    public void StatusListData_Validate_EmptyData_ShouldThrow()
    {
        // Arrange
        var statusData = new StatusListData
        {
            Bits = 2,
            Data = Array.Empty<byte>()
        };

        // Act & Assert
        var act = () => statusData.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data cannot be empty");
    }

    [Fact]
    public void StatusListData_GetStatus_IndexOutOfRange_ShouldThrow()
    {
        // Arrange
        var statusData = StatusListData.Create(10, 2);

        // Act & Assert
        var act = () => statusData.GetStatus(10);
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("index");
    }

    [Fact]
    public void StatusListData_SetStatus_IndexOutOfRange_ShouldThrow()
    {
        // Arrange
        var statusData = StatusListData.Create(10, 2);

        // Act & Assert
        var act = () => statusData.SetStatus(10, 1);
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("index");
    }

    [Fact]
    public void StatusListData_SetStatus_ValueTooLarge_ShouldThrow()
    {
        // Arrange
        var statusData = StatusListData.Create(10, 2); // 2 bits = max value 3

        // Act & Assert
        var act = () => statusData.SetStatus(0, 4); // Value 4 is too large for 2 bits
        act.Should().Throw<ArgumentException>()
           .WithParameterName("statusValue");
    }

    [Fact]
    public void StatusList_Properties_ShouldWorkCorrectly()
    {
        // Arrange - Use the actual StatusList class from Models namespace
        var statusList = new SdJwt.Net.StatusList.Models.StatusList();

        // Act
        statusList.Bits = 2;
        statusList.List = "eJwLBAAAUwBT";
        statusList.AggregationUri = "https://example.com/aggregation";

        // Assert
        statusList.Bits.Should().Be(2);
        statusList.List.Should().Be("eJwLBAAAUwBT");
        statusList.AggregationUri.Should().Be("https://example.com/aggregation");
    }

    [Fact]
    public void StatusListAggregation_Properties_ShouldWorkCorrectly()
    {
        // Arrange
        var aggregation = new StatusListAggregation();
        var statusLists = new[] { "https://example.com/status1", "https://example.com/status2" };

        // Act
        aggregation.StatusLists = statusLists;

        // Assert
        aggregation.StatusLists.Should().BeEquivalentTo(statusLists);
        aggregation.StatusLists.Should().HaveCount(2);
    }

    [Fact]
    public void StatusListData_Count_WithNullData_ShouldReturnZero()
    {
        // Arrange
        var statusData = new StatusListData
        {
            Bits = 2,
            Data = null
        };

        // Act & Assert
        statusData.Count.Should().Be(0);
    }

    [Fact]
    public void StatusListData_Count_WithZeroBits_ShouldReturnZero()
    {
        // Arrange
        var statusData = new StatusListData
        {
            Bits = 0,
            Data = new byte[10]
        };

        // Act & Assert
        statusData.Count.Should().Be(0);
    }

    [Fact]
    public void StatusListData_GetStatus_WithNullData_ShouldThrow()
    {
        // Arrange
        var statusData = new StatusListData
        {
            Bits = 2,
            Data = null
        };

        // Act & Assert
        var act = () => statusData.GetStatus(0);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data is null");
    }

    [Fact]
    public void StatusListData_SetStatus_WithNullData_ShouldThrow()
    {
        // Arrange
        var statusData = new StatusListData
        {
            Bits = 2,
            Data = null
        };

        // Act & Assert
        var act = () => statusData.SetStatus(0, 1);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Data is null");
    }

    [Fact]
    public void StatusListData_BitManipulation_1Bit_ShouldWorkCorrectly()
    {
        // Arrange
        var statusData = StatusListData.Create(8, 1);

        // Act - Set alternating pattern
        for (int i = 0; i < 8; i++)
        {
            statusData.SetStatus(i, (byte)(i % 2));
        }

        // Assert
        for (int i = 0; i < 8; i++)
        {
            statusData.GetStatus(i).Should().Be((byte)(i % 2));
        }
    }

    [Fact]
    public void StatusListData_BitManipulation_4Bit_ShouldWorkCorrectly()
    {
        // Arrange
        var statusData = StatusListData.Create(4, 4);

        // Act - Set different values
        statusData.SetStatus(0, 0);   // 0000
        statusData.SetStatus(1, 5);   // 0101
        statusData.SetStatus(2, 10);  // 1010
        statusData.SetStatus(3, 15);  // 1111

        // Assert
        statusData.GetStatus(0).Should().Be(0);
        statusData.GetStatus(1).Should().Be(5);
        statusData.GetStatus(2).Should().Be(10);
        statusData.GetStatus(3).Should().Be(15);
    }

    [Fact]
    public void StatusListData_BitManipulation_8Bit_ShouldWorkCorrectly()
    {
        // Arrange
        var statusData = StatusListData.Create(3, 8);

        // Act
        statusData.SetStatus(0, 0);     // 00000000
        statusData.SetStatus(1, 128);   // 10000000
        statusData.SetStatus(2, 255);   // 11111111

        // Assert
        statusData.GetStatus(0).Should().Be(0);
        statusData.GetStatus(1).Should().Be(128);
        statusData.GetStatus(2).Should().Be(255);
    }
}

// Mock exception class for testing
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
    public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}
