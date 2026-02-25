using FluentAssertions;
using SdJwt.Net.PresentationExchange.Models;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Models;

/// <summary>
/// Extended test coverage for PresentationExchange models to ensure comprehensive coverage.
/// </summary>
public class PresentationExchangeExtendedTests
{
    [Fact]
    public void FieldFilter_CreateConst_ShouldSetConstProperty()
    {
        // Arrange
        var value = "test-value";

        // Act
        var filter = FieldFilter.CreateConst(value);

        // Assert
        filter.Const.Should().Be(value);
        filter.Type.Should().BeNull();
        filter.Enum.Should().BeNull();
    }

    [Fact]
    public void FieldFilter_CreateEnum_WithValidValues_ShouldSetEnumProperty()
    {
        // Arrange
        var values = new object[] { "value1", "value2", 123 };

        // Act
        var filter = FieldFilter.CreateEnum(values);

        // Assert
        filter.Enum.Should().BeEquivalentTo(values);
        filter.Const.Should().BeNull();
    }

    [Fact]
    public void FieldFilter_CreateEnum_WithNullValues_ShouldThrow()
    {
        // Act & Assert
        var act = () => FieldFilter.CreateEnum(null!);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("values");
    }

    [Fact]
    public void FieldFilter_CreateEnum_WithEmptyValues_ShouldThrow()
    {
        // Act & Assert
        var act = () => FieldFilter.CreateEnum();
        act.Should().Throw<ArgumentException>()
           .WithParameterName("values");
    }

    [Fact]
    public void FieldFilter_CreatePattern_WithValidPattern_ShouldSetPatternProperty()
    {
        // Arrange
        var pattern = @"^[A-Z]{2}\d{4}$";

        // Act
        var filter = FieldFilter.CreatePattern(pattern);

        // Assert
        filter.Type.Should().Be("string");
        filter.Pattern.Should().Be(pattern);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FieldFilter_CreatePattern_WithInvalidPattern_ShouldThrow(string? pattern)
    {
        // Act & Assert
        var act = () => FieldFilter.CreatePattern(pattern!);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("pattern");
    }

    [Fact]
    public void FieldFilter_CreateRange_WithNumericRange_ShouldSetMinMaxProperties()
    {
        // Arrange
        var minimum = 18;
        var maximum = 65;

        // Act
        var filter = FieldFilter.CreateRange(minimum, maximum);

        // Assert
        filter.Type.Should().Be("number");
        filter.Minimum.Should().Be(minimum);
        filter.Maximum.Should().Be(maximum);
    }

    [Fact]
    public void FieldFilter_CreateRange_AsInteger_ShouldSetIntegerType()
    {
        // Act
        var filter = FieldFilter.CreateRange(10, 20, isInteger: true);

        // Assert
        filter.Type.Should().Be("integer");
        filter.Minimum.Should().Be(10);
        filter.Maximum.Should().Be(20);
    }

    [Fact]
    public void FieldFilter_CreateRange_WithOnlyMinimum_ShouldSetOnlyMinimum()
    {
        // Act
        var filter = FieldFilter.CreateRange(minimum: 10);

        // Assert
        filter.Type.Should().Be("number");
        filter.Minimum.Should().Be(10);
        filter.Maximum.Should().BeNull();
    }

    [Fact]
    public void FieldFilter_CreateArrayContains_ShouldSetContainsProperty()
    {
        // Arrange
        var value = "required-item";

        // Act
        var filter = FieldFilter.CreateArrayContains(value);

        // Assert
        filter.Type.Should().Be("array");
        filter.Contains.Should().NotBeNull();
    }

    [Fact]
    public void FieldFilter_CreateType_WithValidType_ShouldSetTypeProperty()
    {
        // Arrange
        var type = "boolean";

        // Act
        var filter = FieldFilter.CreateType(type);

        // Assert
        filter.Type.Should().Be(type);
        filter.Const.Should().BeNull();
        filter.Enum.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FieldFilter_CreateType_WithInvalidType_ShouldThrow(string? type)
    {
        // Act & Assert
        var act = () => FieldFilter.CreateType(type!);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("type");
    }

    [Fact]
    public void FieldFilter_Validate_WithValidType_ShouldNotThrow()
    {
        // Arrange
        var filter = new FieldFilter { Type = "string" };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should().NotThrow();
    }

    [Fact]
    public void FieldFilter_Validate_WithInvalidType_ShouldThrow()
    {
        // Arrange
        var filter = new FieldFilter { Type = "invalid-type" };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should()
               .Throw<InvalidOperationException>()
               .WithMessage("Invalid type 'invalid-type'*");
    }

    [Fact]
    public void FieldFilter_Validate_WithNegativeMinLength_ShouldThrow()
    {
        // Arrange
        var filter = new FieldFilter
        {
            Type = "string",
            MinLength = -1
        };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should()
               .Throw<InvalidOperationException>()
               .WithMessage("MinLength must be non-negative");
    }

    [Fact]
    public void FieldFilter_Validate_WithMinLengthGreaterThanMaxLength_ShouldThrow()
    {
        // Arrange
        var filter = new FieldFilter
        {
            Type = "string",
            MinLength = 10,
            MaxLength = 5
        };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should()
               .Throw<InvalidOperationException>()
               .WithMessage("MinLength cannot be greater than MaxLength");
    }

    [Fact]
    public void FieldFilter_Validate_WithNegativeMinItems_ShouldThrow()
    {
        // Arrange
        var filter = new FieldFilter
        {
            Type = "array",
            MinItems = -1
        };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should()
               .Throw<InvalidOperationException>()
               .WithMessage("MinItems must be non-negative");
    }

    [Fact]
    public void FieldFilter_Validate_WithInvalidRequiredProperties_ShouldThrow()
    {
        // Arrange
        var filter = new FieldFilter
        {
            Type = "object",
            Required = new[] { "valid-prop", "", "another-prop" }
        };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should()
               .Throw<InvalidOperationException>()
               .WithMessage("Required property names cannot be null or empty");
    }

    [Fact]
    public void FieldFilter_Validate_WithNonNumericConstForNumberType_ShouldThrow()
    {
        // Arrange
        var filter = new FieldFilter
        {
            Type = "number",
            Const = "not-a-number"
        };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should()
               .Throw<InvalidOperationException>()
               .WithMessage("Const value must be numeric for type 'number'");
    }

    [Fact]
    public void Field_Create_WithMultiplePaths_ShouldSetPathsCorrectly()
    {
        // Arrange
        var paths = new[] { "$.path1", "$.path2", "$.path3" };

        // Act
        var field = Field.Create(paths);

        // Assert
        field.Path.Should().BeEquivalentTo(paths);
        field.Optional.Should().BeFalse();
    }

    [Fact]
    public void Field_CreateForValues_WithMultipleValues_ShouldCreateEnumFilter()
    {
        // Arrange
        var path = "$.status";
        var values = new[] { "active", "inactive", "pending" };

        // Act
        var field = Field.CreateForValues(path, values);

        // Assert
        field.Path.Should().ContainSingle(path);
        field.Filter.Should().NotBeNull();
        field.Filter!.Enum.Should().BeEquivalentTo(values);
    }

    [Fact]
    public void Field_CreateForType_WithVcType_ShouldCreateArrayContainsFilter()
    {
        // Arrange
        var credentialType = "VerifiableCredential";

        // Act
        var field = Field.CreateForType(credentialType, isVc: true);

        // Assert
        field.Path.Should().ContainSingle("$.vc.type");
        field.Filter.Should().NotBeNull();
        field.Filter!.Type.Should().Be("array");
        field.Filter.Contains.Should().NotBeNull();
    }

    [Fact]
    public void Field_CreateForType_WithSdJwtType_ShouldCreateConstFilter()
    {
        // Arrange
        var credentialType = "UniversityDegree";

        // Act
        var field = Field.CreateForType(credentialType, isVc: false);

        // Assert
        field.Path.Should().ContainSingle("$.vct");
        field.Filter.Should().NotBeNull();
        field.Filter!.Const.Should().Be(credentialType);
    }

    [Fact]
    public void Field_CreateForIssuers_WithMultipleIssuers_ShouldCreateEnumFilter()
    {
        // Arrange
        var issuers = new[]
        {
            "https://issuer1.example.com",
            "https://issuer2.example.com",
            "https://issuer3.example.com"
        };

        // Act
        var field = Field.CreateForIssuers(issuers);

        // Assert
        field.Path.Should().ContainSingle("$.iss");
        field.Filter.Should().NotBeNull();
        field.Filter!.Enum.Should().BeEquivalentTo(issuers);
    }

    [Fact]
    public void Field_Validate_WithInvalidJsonPath_ShouldThrow()
    {
        // Arrange
        var field = new Field
        {
            Path = new[] { "invalid-path-without-dollar" }
        };

        // Act & Assert
        field.Invoking(f => f.Validate()).Should()
              .Throw<InvalidOperationException>()
              .WithMessage("Field path must be a valid JSON path starting with '$'*");
    }

    [Fact]
    public void Field_Validate_WithNullPath_ShouldThrow()
    {
        // Arrange
        var field = new Field { Path = null };

        // Act & Assert
        field.Invoking(f => f.Validate()).Should()
              .Throw<InvalidOperationException>()
              .WithMessage("Field constraint must have at least one path");
    }

    [Fact]
    public void Field_Validate_WithEmptyPathArray_ShouldThrow()
    {
        // Arrange
        var field = new Field { Path = Array.Empty<string>() };

        // Act & Assert
        field.Invoking(f => f.Validate()).Should()
              .Throw<InvalidOperationException>()
              .WithMessage("Field constraint must have at least one path");
    }

    [Fact]
    public void Field_Validate_WithEmptyStringInPath_ShouldThrow()
    {
        // Arrange
        var field = new Field
        {
            Path = new[] { "$.valid", "", "$.another" }
        };

        // Act & Assert
        field.Invoking(f => f.Validate()).Should()
              .Throw<InvalidOperationException>()
              .WithMessage("Field path cannot be null or empty");
    }

    [Fact]
    public void Field_GetPrimaryPath_WithMultiplePaths_ShouldReturnFirst()
    {
        // Arrange
        var paths = new[] { "$.primary", "$.secondary", "$.tertiary" };
        var field = new Field { Path = paths };

        // Act
        var primaryPath = field.GetPrimaryPath();

        // Assert
        primaryPath.Should().Be("$.primary");
    }

    [Fact]
    public void Field_GetPrimaryPath_WithNullPath_ShouldReturnNull()
    {
        // Arrange
        var field = new Field { Path = null };

        // Act
        var primaryPath = field.GetPrimaryPath();

        // Assert
        primaryPath.Should().BeNull();
    }

    [Fact]
    public void Field_HasFilter_WithFilter_ShouldReturnTrue()
    {
        // Arrange
        var field = new Field
        {
            Path = new[] { "$.test" },
            Filter = new FieldFilter { Type = "string" }
        };

        // Act
        var hasFilter = field.HasFilter();

        // Assert
        hasFilter.Should().BeTrue();
    }

    [Fact]
    public void Field_HasFilter_WithoutFilter_ShouldReturnFalse()
    {
        // Arrange
        var field = new Field
        {
            Path = new[] { "$.test" },
            Filter = null
        };

        // Act
        var hasFilter = field.HasFilter();

        // Assert
        hasFilter.Should().BeFalse();
    }

    [Fact]
    public void Field_RequiresSelectiveDisclosure_WithIntentToRetain_ShouldReturnTrue()
    {
        // Arrange
        var field = new Field
        {
            Path = new[] { "$.test" },
            IntentToRetain = true
        };

        // Act
        var requiresDisclosure = field.RequiresSelectiveDisclosure();

        // Assert
        requiresDisclosure.Should().BeTrue();
    }

    [Fact]
    public void Field_RequiresSelectiveDisclosure_WithoutIntentToRetain_ShouldReturnFalse()
    {
        // Arrange
        var field = new Field
        {
            Path = new[] { "$.test" },
            IntentToRetain = false
        };

        // Act
        var requiresDisclosure = field.RequiresSelectiveDisclosure();

        // Assert
        requiresDisclosure.Should().BeFalse();
    }

    [Fact]
    public void InputDescriptor_Create_WithAllParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "test-id";
        var name = "Test Name";
        var purpose = "Test Purpose";

        // Act
        var descriptor = InputDescriptor.Create(id, name, purpose);

        // Assert
        descriptor.Id.Should().Be(id);
        descriptor.Name.Should().Be(name);
        descriptor.Purpose.Should().Be(purpose);
        descriptor.Constraints.Should().BeNull();
        descriptor.Format.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void InputDescriptor_Create_WithInvalidId_ShouldThrow(string? id)
    {
        // Act & Assert
        var act = () => InputDescriptor.Create(id!);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("id");
    }

    [Fact]
    public void InputDescriptor_Validate_WithEmptyId_ShouldThrow()
    {
        // Arrange
        var descriptor = new InputDescriptor { Id = "" };

        // Act & Assert
        descriptor.Invoking(d => d.Validate()).Should()
                  .Throw<InvalidOperationException>()
                  .WithMessage("Input descriptor ID is required");
    }

    [Fact]
    public void InputDescriptor_Validate_WithEmptyGroupId_ShouldThrow()
    {
        // Arrange
        var descriptor = new InputDescriptor
        {
            Id = "test-id",
            Group = new[] { "valid-group", "" }
        };

        // Act & Assert
        descriptor.Invoking(d => d.Validate()).Should()
                  .Throw<InvalidOperationException>()
                  .WithMessage("Group identifier cannot be null or empty");
    }

    [Fact]
    public void InputDescriptor_BelongsToGroup_WithMatchingGroup_ShouldReturnTrue()
    {
        // Arrange
        var descriptor = new InputDescriptor
        {
            Id = "test-id",
            Group = new[] { "group1", "group2", "group3" }
        };

        // Act & Assert
        descriptor.BelongsToGroup("group2").Should().BeTrue();
        descriptor.BelongsToGroup("group1", "group4").Should().BeTrue();
        descriptor.BelongsToGroup("group4", "group5").Should().BeFalse();
    }

    [Fact]
    public void InputDescriptor_BelongsToGroup_WithNullGroup_ShouldReturnFalse()
    {
        // Arrange
        var descriptor = new InputDescriptor
        {
            Id = "test-id",
            Group = null
        };

        // Act & Assert
        descriptor.BelongsToGroup("any-group").Should().BeFalse();
    }

    [Fact]
    public void InputDescriptor_GetAcceptedFormats_WithoutFormat_ShouldReturnAllFormats()
    {
        // Arrange
        var descriptor = new InputDescriptor { Id = "test-id" };

        // Act
        var formats = descriptor.GetAcceptedFormats();

        // Assert
        // Use hardcoded expected formats since constants may not be available
        var expectedFormats = new[] { "jwt", "jwt_vc", "jwt_vp", "sd-jwt", "vc+sd-jwt", "ldp", "ldp_vc", "ldp_vp" };
        formats.Should().BeEquivalentTo(expectedFormats);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("number")]
    [InlineData("integer")]
    [InlineData("boolean")]
    [InlineData("array")]
    [InlineData("object")]
    [InlineData("null")]
    public void FieldFilter_Validate_WithValidTypes_ShouldNotThrow(string validType)
    {
        // Arrange
        var filter = new FieldFilter { Type = validType };

        // Act & Assert
        filter.Invoking(f => f.Validate()).Should().NotThrow();
    }

    [Fact]
    public void FieldFilter_ExtensionData_ShouldAllowAdditionalProperties()
    {
        // Arrange
        var filter = new FieldFilter
        {
            Type = "string",
            ExtensionData = new Dictionary<string, object>
            {
                ["custom_property"] = "custom_value",
                ["another_property"] = 42
            }
        };

        // Act & Assert
        filter.ExtensionData.Should().NotBeNull();
        filter.ExtensionData!["custom_property"].Should().Be("custom_value");
        filter.ExtensionData["another_property"].Should().Be(42);
    }

    [Fact]
    public void FieldFilter_Validate_WithValidNumericRanges_ShouldNotThrow()
    {
        // Test minimum < maximum
        var validFilter = new FieldFilter
        {
            Type = "number",
            Minimum = 10,
            Maximum = 20
        };

        validFilter.Invoking(f => f.Validate()).Should().NotThrow();
    }

    [Fact]
    public void FieldFilter_Validate_WithInvalidNumericRanges_ShouldThrow()
    {
        // Test minimum > maximum
        var invalidFilter = new FieldFilter
        {
            Type = "number",
            Minimum = 20,
            Maximum = 10
        };

        invalidFilter.Invoking(f => f.Validate()).Should()
                     .Throw<InvalidOperationException>()
                     .WithMessage("Minimum cannot be greater than Maximum");
    }

    [Fact]
    public void FieldFilter_Validate_WithExclusiveRanges_ShouldValidateCorrectly()
    {
        // Valid exclusive range
        var validFilter = new FieldFilter
        {
            Type = "number",
            ExclusiveMinimum = 10,
            ExclusiveMaximum = 20
        };

        validFilter.Invoking(f => f.Validate()).Should().NotThrow();

        // Invalid exclusive range (min >= max)
        var invalidFilter = new FieldFilter
        {
            Type = "number",
            ExclusiveMinimum = 20,
            ExclusiveMaximum = 20
        };

        invalidFilter.Invoking(f => f.Validate()).Should()
                     .Throw<InvalidOperationException>()
                     .WithMessage("ExclusiveMinimum must be less than ExclusiveMaximum");
    }

    [Fact]
    public void Field_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var field = new Field();
        var paths = new[] { "$.test.path" };
        var filter = new FieldFilter { Type = "string" };

        // Act
        field.Path = paths;
        field.Id = "test-field-id";
        field.Name = "Test Field Name";
        field.Purpose = "Test Field Purpose";
        field.Filter = filter;
        field.Optional = true;
        field.IntentToRetain = true;

        // Assert
        field.Path.Should().BeEquivalentTo(paths);
        field.Id.Should().Be("test-field-id");
        field.Name.Should().Be("Test Field Name");
        field.Purpose.Should().Be("Test Field Purpose");
        field.Filter.Should().Be(filter);
        field.Optional.Should().BeTrue();
        field.IntentToRetain.Should().BeTrue();
    }

    [Fact]
    public void InputDescriptor_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var descriptor = new InputDescriptor();
        var group = new[] { "group1", "group2" };
        var format = new FormatConstraints();
        var constraints = new Constraints();

        // Act
        descriptor.Id = "test-descriptor-id";
        descriptor.Name = "Test Descriptor Name";
        descriptor.Purpose = "Test Descriptor Purpose";
        descriptor.Group = group;
        descriptor.Format = format;
        descriptor.Constraints = constraints;

        // Assert
        descriptor.Id.Should().Be("test-descriptor-id");
        descriptor.Name.Should().Be("Test Descriptor Name");
        descriptor.Purpose.Should().Be("Test Descriptor Purpose");
        descriptor.Group.Should().BeEquivalentTo(group);
        descriptor.Format.Should().Be(format);
        descriptor.Constraints.Should().Be(constraints);
    }
}
