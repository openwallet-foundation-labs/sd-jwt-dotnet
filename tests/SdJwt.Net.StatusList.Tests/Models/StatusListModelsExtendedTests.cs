using FluentAssertions;
using SdJwt.Net.StatusList.Models;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Models;

/// <summary>
/// Extended test coverage for StatusList models to ensure comprehensive coverage.
/// </summary>
public class StatusListModelsExtendedTests
{
    [Fact]
    public void StatusTypeExtensions_GetName_WithUnknownValue_ShouldReturnUnknownFormat()
    {
        // Arrange
        var unknownStatus = (StatusType)99;

        // Act
        var name = unknownStatus.GetName();

        // Assert
        name.Should().Be("UNKNOWN_63"); // 99 in hex is 63
    }

    [Fact]
    public void StatusTypeExtensions_ToStringValue_WithUnknownValue_ShouldReturnUnknownFormat()
    {
        // Arrange
        var unknownStatus = (StatusType)99;

        // Act
        var stringValue = unknownStatus.ToStringValue();

        // Assert
        stringValue.Should().Be("unknown_99");
    }

    [Fact]
    public void StatusTypeExtensions_GetDescription_WithUnknownValue_ShouldReturnUnknownFormat()
    {
        // Arrange
        var unknownStatus = (StatusType)99;

        // Act
        var description = unknownStatus.GetDescription();

        // Assert
        description.Should().StartWith("Unknown status type with value 0x");
        description.Should().Contain("63"); // 99 in hex
    }

    [Fact]
    public void StatusTypeExtensions_FromValue_WithApplicationSpecificRange_ShouldReturnApplicationSpecific()
    {
        // Test all values in the application-specific range (0x0B - 0x0F)
        for (int value = 0x0B; value <= 0x0F; value++)
        {
            var result = StatusTypeExtensions.FromValue(value);
            result.Should().Be(StatusType.ApplicationSpecific);
        }
    }

    [Fact]
    public void StatusTypeExtensions_FromValue_WithArbitraryValue_ShouldReturnCastedValue()
    {
        // Arrange
        var arbitraryValue = 42;

        // Act
        var result = StatusTypeExtensions.FromValue(arbitraryValue);

        // Assert
        result.Should().Be((StatusType)arbitraryValue);
    }

    [Fact]
    public void StatusListData_Count_WithVariousBitSizes_ShouldCalculateCorrectly()
    {
        // Test different bit sizes
        var testCases = new[]
        {
            new { Bits = 1, ByteLength = 10, ExpectedCount = 80 }, // 10 * 8 / 1
            new { Bits = 2, ByteLength = 10, ExpectedCount = 40 }, // 10 * 8 / 2
            new { Bits = 4, ByteLength = 10, ExpectedCount = 20 }, // 10 * 8 / 4
            new { Bits = 8, ByteLength = 10, ExpectedCount = 10 }, // 10 * 8 / 8
        };

        foreach (var testCase in testCases)
        {
            var statusData = new StatusListData
            {
                Bits = testCase.Bits,
                Data = new byte[testCase.ByteLength]
            };

            statusData.Count.Should().Be(testCase.ExpectedCount, 
                $"For {testCase.Bits} bits with {testCase.ByteLength} bytes");
        }
    }

    [Fact]
    public void StatusListData_Create_WithEdgeCaseSizes_ShouldWorkCorrectly()
    {
        // Test edge cases
        var testCases = new[]
        {
            new { Capacity = 1, Bits = 1, ExpectedByteLength = 1 },
            new { Capacity = 7, Bits = 1, ExpectedByteLength = 1 }, // 7 bits fits in 1 byte
            new { Capacity = 9, Bits = 1, ExpectedByteLength = 2 }, // 9 bits needs 2 bytes
            new { Capacity = 1, Bits = 8, ExpectedByteLength = 1 },
            new { Capacity = 2, Bits = 8, ExpectedByteLength = 2 },
        };

        foreach (var testCase in testCases)
        {
            var result = StatusListData.Create(testCase.Capacity, testCase.Bits);
            
            result.Capacity.Should().Be(testCase.Capacity);
            result.Data!.Length.Should().Be(testCase.ExpectedByteLength,
                $"For capacity {testCase.Capacity} with {testCase.Bits} bits");
        }
    }

    [Fact]
    public void StatusListData_SetStatus_GetStatus_WithBoundaryValues_ShouldWorkCorrectly()
    {
        var testCases = new[]
        {
            new { Bits = 1, MaxValue = 1 },
            new { Bits = 2, MaxValue = 3 },
            new { Bits = 4, MaxValue = 15 },
            new { Bits = 8, MaxValue = 255 }
        };

        foreach (var testCase in testCases)
        {
            var statusData = StatusListData.Create(2, testCase.Bits);

            // Test minimum value (always 0)
            statusData.SetStatus(0, 0);
            statusData.GetStatus(0).Should().Be(0);

            // Test maximum value
            statusData.SetStatus(1, (byte)testCase.MaxValue);
            statusData.GetStatus(1).Should().Be((byte)testCase.MaxValue);
        }
    }

    [Fact]
    public void StatusListData_GetStatus_SetStatus_WithNegativeIndex_ShouldThrow()
    {
        // Arrange
        var statusData = StatusListData.Create(10, 2);

        // Act & Assert - GetStatus
        var actGet = () => statusData.GetStatus(-1);
        actGet.Should().Throw<ArgumentOutOfRangeException>()
              .WithParameterName("index");

        // Act & Assert - SetStatus
        var actSet = () => statusData.SetStatus(-1, 1);
        actSet.Should().Throw<ArgumentOutOfRangeException>()
              .WithParameterName("index");
    }

    [Fact]
    public void StatusCheckResult_Properties_ShouldReflectStatusCorrectly()
    {
        // Test each status type
        var testCases = new[]
        {
            new { Status = StatusType.Valid, ExpectedIsValid = true, ExpectedIsInvalid = false, ExpectedIsSuspended = false, ExpectedIsActive = true },
            new { Status = StatusType.Invalid, ExpectedIsValid = false, ExpectedIsInvalid = true, ExpectedIsSuspended = false, ExpectedIsActive = false },
            new { Status = StatusType.Suspended, ExpectedIsValid = false, ExpectedIsInvalid = false, ExpectedIsSuspended = true, ExpectedIsActive = false },
            new { Status = StatusType.UnderInvestigation, ExpectedIsValid = false, ExpectedIsInvalid = false, ExpectedIsSuspended = false, ExpectedIsActive = false }
        };

        foreach (var testCase in testCases)
        {
            var result = new StatusCheckResult { Status = testCase.Status };

            result.IsValid.Should().Be(testCase.ExpectedIsValid, $"IsValid for {testCase.Status}");
            result.IsInvalid.Should().Be(testCase.ExpectedIsInvalid, $"IsInvalid for {testCase.Status}");
            result.IsSuspended.Should().Be(testCase.ExpectedIsSuspended, $"IsSuspended for {testCase.Status}");
            result.IsActive.Should().Be(testCase.ExpectedIsActive, $"IsActive for {testCase.Status}");
        }
    }

    [Fact]
    public void StatusCheckResult_StaticFactoryMethods_ShouldSetCorrectProperties()
    {
        // Test Success
        var success = StatusCheckResult.Success();
        success.Status.Should().Be(StatusType.Valid);
        success.StatusValue.Should().Be(0x00);
        success.ErrorMessage.Should().BeNull();

        // Test Revoked
        var revoked = StatusCheckResult.Revoked();
        revoked.Status.Should().Be(StatusType.Invalid);
        revoked.StatusValue.Should().Be(0x01);
        revoked.ErrorMessage.Should().BeNull();

        // Test Suspended
        var suspended = StatusCheckResult.Suspended();
        suspended.Status.Should().Be(StatusType.Suspended);
        suspended.StatusValue.Should().Be(0x02);
        suspended.ErrorMessage.Should().BeNull();

        // Test Failed
        var errorMessage = "Test error";
        var failed = StatusCheckResult.Failed(errorMessage);
        failed.Status.Should().Be(StatusType.Invalid);
        failed.StatusValue.Should().Be(-1);
        failed.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void StatusListReference_Validate_WithValidAbsoluteHttpsUri_ShouldNotThrow()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "https://example.com/status-list",
            Index = 0
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void StatusListReference_Validate_WithValidAbsoluteHttpUri_ShouldNotThrow()
    {
        // Note: The current implementation allows HTTP URIs
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "http://example.com/status-list",
            Index = 0
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void StatusListReference_Validate_WithFtpUri_ShouldNotThrow()
    {
        // Note: The current implementation allows any absolute URI
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "ftp://example.com/status-list",
            Index = 0
        };

        // Act & Assert
        var act = () => reference.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void StatusList_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var statusList = new SdJwt.Net.StatusList.Models.StatusList();

        // Assert
        statusList.Bits.Should().Be(0);
        statusList.List.Should().Be(string.Empty);
        statusList.AggregationUri.Should().BeNull();
    }

    [Fact]
    public void StatusListAggregation_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var aggregation = new StatusListAggregation();

        // Assert
        aggregation.StatusLists.Should().NotBeNull();
        aggregation.StatusLists.Should().BeEmpty();
    }

    [Fact]
    public void StatusListData_SetStatus_WithAllBitsSet_ShouldWorkCorrectly()
    {
        // Test with 4-bit values where all bits are set
        var statusData = StatusListData.Create(4, 4);

        // Set all possible 4-bit values
        for (byte value = 0; value <= 15; value++)
        {
            statusData.SetStatus(value % 4, value);
            statusData.GetStatus(value % 4).Should().Be(value);
        }
    }

    [Theory]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(2, 0, 3)]
    [InlineData(2, 1, 2)]
    [InlineData(2, 2, 1)]
    [InlineData(2, 3, 0)]
    [InlineData(4, 0, 15)]
    [InlineData(4, 7, 8)]
    [InlineData(4, 15, 0)]
    [InlineData(8, 0, 255)]
    [InlineData(8, 127, 128)]
    [InlineData(8, 255, 0)]
    public void StatusListData_SetStatus_WithSpecificBitPatterns_ShouldWorkCorrectly(int bits, byte setValue, byte expectedOtherValue)
    {
        // Create a status list with 2 entries
        var statusData = StatusListData.Create(2, bits);

        // Set the first entry
        statusData.SetStatus(0, setValue);
        
        // Set the second entry to a different value
        statusData.SetStatus(1, expectedOtherValue);

        // Verify both values
        statusData.GetStatus(0).Should().Be(setValue);
        statusData.GetStatus(1).Should().Be(expectedOtherValue);
    }
}
