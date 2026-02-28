using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Coverage;

/// <summary>
/// Tests for <see cref="InputDescriptorMapping"/> and <see cref="PathNestedDescriptor"/> classes,
/// covering creation, validation, and JSON serialization.
/// </summary>
public class InputDescriptorMappingTests
{
    #region InputDescriptorMapping Tests

    /// <summary>
    /// Tests that Create() returns a valid instance with the provided parameters.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithValidParams_ReturnsInstance()
    {
        // Act
        var result = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");

        // Assert
        result.Id.Should().Be("desc-1");
        result.Format.Should().Be("vc+sd-jwt");
        result.Path.Should().Be("$");
        result.PathNested.Should().BeNull();
    }

    /// <summary>
    /// Tests that Create() uses default path when not specified.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithDefaultPath_ReturnsInstanceWithDollarPath()
    {
        // Act
        var result = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt");

        // Assert
        result.Path.Should().Be("$");
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when id is null.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create(null!, "vc+sd-jwt", "$");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when id is empty.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("", "vc+sd-jwt", "$");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when id is whitespace.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithWhitespaceId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("   ", "vc+sd-jwt", "$");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when format is null.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithNullFormat_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("desc-1", null!, "$");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when format is empty.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithEmptyFormat_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("desc-1", "", "$");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when format is whitespace.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithWhitespaceFormat_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("desc-1", "   ", "$");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when path is null.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when path is empty.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when path is whitespace.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Create_WithWhitespacePath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "   ");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateForSdJwt() with default index returns a valid instance.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_CreateForSdJwt_WithDefaultIndex_ReturnsInstance()
    {
        // Act
        var result = InputDescriptorMapping.CreateForSdJwt("input-1");

        // Assert
        result.Id.Should().Be("input-1");
        result.Format.Should().Be(Oid4VpConstants.SdJwtVcFormat);
        result.Path.Should().Be("$");
    }

    /// <summary>
    /// Tests that CreateForSdJwt() with index 0 returns path "$".
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_CreateForSdJwt_WithIndexZero_ReturnsRootPath()
    {
        // Act
        var result = InputDescriptorMapping.CreateForSdJwt("input-1", 0);

        // Assert
        result.Path.Should().Be("$");
    }

    /// <summary>
    /// Tests that CreateForSdJwt() with index 1 returns path "$[1]".
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_CreateForSdJwt_WithIndex1_ReturnsArrayPath()
    {
        // Act
        var result = InputDescriptorMapping.CreateForSdJwt("input-1", 1);

        // Assert
        result.Path.Should().Be("$[1]");
    }

    /// <summary>
    /// Tests that CreateForSdJwt() with various index values produces correct paths.
    /// </summary>
    [Theory]
    [InlineData(2, "$[2]")]
    [InlineData(5, "$[5]")]
    [InlineData(10, "$[10]")]
    [InlineData(100, "$[100]")]
    public void InputDescriptorMapping_CreateForSdJwt_WithVariousIndices_ReturnsCorrectPath(int index, string expectedPath)
    {
        // Act
        var result = InputDescriptorMapping.CreateForSdJwt("input-1", index);

        // Assert
        result.Path.Should().Be(expectedPath);
    }

    /// <summary>
    /// Tests that CreateForSdJwt() throws ArgumentException when inputDescriptorId is null.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_CreateForSdJwt_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.CreateForSdJwt(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateForSdJwt() throws ArgumentException when inputDescriptorId is empty.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_CreateForSdJwt_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.CreateForSdJwt("");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateForSdJwt() throws ArgumentException when index is negative.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_CreateForSdJwt_WithNegativeIndex_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.CreateForSdJwt("input-1", -1);
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    /// <summary>
    /// Tests that CreateForSdJwt() throws ArgumentException when index is -5.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_CreateForSdJwt_WithLargeNegativeIndex_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InputDescriptorMapping.CreateForSdJwt("input-1", -5);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that WithPathNested() adds a nested descriptor and returns this for chaining.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_WithPathNested_WithValidDescriptor_SetsPathNestedAndReturnsThis()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");
        var nested = PathNestedDescriptor.Create("$.vc");

        // Act
        var result = mapping.WithPathNested(nested);

        // Assert
        result.Should().BeSameAs(mapping);
        result.PathNested.Should().BeSameAs(nested);
    }

    /// <summary>
    /// Tests that WithPathNested() throws ArgumentNullException when descriptor is null.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_WithPathNested_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");

        // Act & Assert
        var act = () => mapping.WithPathNested(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Validate() succeeds with a valid mapping.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Validate_WithValidMapping_DoesNotThrow()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");

        // Act & Assert
        var act = () => mapping.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when Id is empty.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Validate_WithEmptyId_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapping = new InputDescriptorMapping
        {
            Id = "",
            Format = "vc+sd-jwt",
            Path = "$"
        };

        // Act & Assert
        var act = () => mapping.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*id*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when Format is empty.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Validate_WithEmptyFormat_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapping = new InputDescriptorMapping
        {
            Id = "desc-1",
            Format = "",
            Path = "$"
        };

        // Act & Assert
        var act = () => mapping.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*format*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when Path is empty.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Validate_WithEmptyPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapping = new InputDescriptorMapping
        {
            Id = "desc-1",
            Format = "vc+sd-jwt",
            Path = ""
        };

        // Act & Assert
        var act = () => mapping.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*path*required*");
    }

    /// <summary>
    /// Tests that Validate() cascades validation to PathNested.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Validate_WithInvalidPathNested_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");
        mapping.PathNested = new PathNestedDescriptor { Path = "" };  // Invalid nested

        // Act & Assert
        var act = () => mapping.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*path*required*");
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid PathNested.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_Validate_WithValidPathNested_DoesNotThrow()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$")
            .WithPathNested(PathNestedDescriptor.Create("$.vc"));

        // Act & Assert
        var act = () => mapping.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests JSON serialization of InputDescriptorMapping.
    /// </summary>
    [Fact]
    public void InputDescriptorMapping_JsonSerialization_RoundTripsCorrectly()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$[0]")
            .WithPathNested(PathNestedDescriptor.Create("$.vc", "jwt_vc"));

        // Act
        var json = JsonSerializer.Serialize(mapping);
        var deserialized = JsonSerializer.Deserialize<InputDescriptorMapping>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be("desc-1");
        deserialized.Format.Should().Be("vc+sd-jwt");
        deserialized.Path.Should().Be("$[0]");
        deserialized.PathNested.Should().NotBeNull();
        deserialized.PathNested!.Path.Should().Be("$.vc");
        deserialized.PathNested.Format.Should().Be("jwt_vc");
    }

    #endregion

    #region PathNestedDescriptor Tests

    /// <summary>
    /// Tests that Create() returns a valid instance with path only.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Create_WithPathOnly_ReturnsInstance()
    {
        // Act
        var result = PathNestedDescriptor.Create("$.vc");

        // Assert
        result.Path.Should().Be("$.vc");
        result.Format.Should().BeNull();
        result.PathNested.Should().BeNull();
    }

    /// <summary>
    /// Tests that Create() returns a valid instance with path and format.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Create_WithPathAndFormat_ReturnsInstance()
    {
        // Act
        var result = PathNestedDescriptor.Create("$.vc", "jwt_vc");

        // Assert
        result.Path.Should().Be("$.vc");
        result.Format.Should().Be("jwt_vc");
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when path is null.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Create_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PathNestedDescriptor.Create(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when path is empty.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Create_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PathNestedDescriptor.Create("");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create() throws ArgumentException when path is whitespace.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Create_WithWhitespacePath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PathNestedDescriptor.Create("   ");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that WithPathNested() allows chaining nested descriptors.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_WithPathNested_ChainsCorrectly()
    {
        // Arrange
        var outer = PathNestedDescriptor.Create("$.vc");
        var inner = PathNestedDescriptor.Create("$.inner");

        // Act
        var result = outer.WithPathNested(inner);

        // Assert
        result.Should().BeSameAs(outer);
        result.PathNested.Should().BeSameAs(inner);
    }

    /// <summary>
    /// Tests that WithPathNested() throws ArgumentNullException when nested is null.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_WithPathNested_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var descriptor = PathNestedDescriptor.Create("$.vc");

        // Act & Assert
        var act = () => descriptor.WithPathNested(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid descriptor.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Validate_WithValidDescriptor_DoesNotThrow()
    {
        // Arrange
        var descriptor = PathNestedDescriptor.Create("$.vc");

        // Act & Assert
        var act = () => descriptor.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when Path is empty.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Validate_WithEmptyPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var descriptor = new PathNestedDescriptor { Path = "" };

        // Act & Assert
        var act = () => descriptor.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*path*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when Path is whitespace.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Validate_WithWhitespacePath_ThrowsInvalidOperationException()
    {
        // Arrange
        var descriptor = new PathNestedDescriptor { Path = "   " };

        // Act & Assert
        var act = () => descriptor.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*path*required*");
    }

    /// <summary>
    /// Tests that Validate() cascades to nested PathNested.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Validate_CascadesToNestedPathNested()
    {
        // Arrange
        var outer = PathNestedDescriptor.Create("$.vc");
        outer.PathNested = new PathNestedDescriptor { Path = "" };  // Invalid

        // Act & Assert
        var act = () => outer.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*path*required*");
    }

    /// <summary>
    /// Tests that Validate() succeeds with deeply nested valid descriptors.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_Validate_WithDeeplyNestedValid_DoesNotThrow()
    {
        // Arrange
        var descriptor = PathNestedDescriptor.Create("$.level1")
            .WithPathNested(PathNestedDescriptor.Create("$.level2")
                .WithPathNested(PathNestedDescriptor.Create("$.level3")));

        // Act & Assert
        var act = () => descriptor.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests JSON serialization of PathNestedDescriptor.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_JsonSerialization_RoundTripsCorrectly()
    {
        // Arrange
        var descriptor = PathNestedDescriptor.Create("$.vc", "jwt_vc")
            .WithPathNested(PathNestedDescriptor.Create("$.inner"));

        // Act
        var json = JsonSerializer.Serialize(descriptor);
        var deserialized = JsonSerializer.Deserialize<PathNestedDescriptor>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Path.Should().Be("$.vc");
        deserialized.Format.Should().Be("jwt_vc");
        deserialized.PathNested.Should().NotBeNull();
        deserialized.PathNested!.Path.Should().Be("$.inner");
    }

    /// <summary>
    /// Tests JSON serialization omits null Format and PathNested.
    /// </summary>
    [Fact]
    public void PathNestedDescriptor_JsonSerialization_OmitsNullProperties()
    {
        // Arrange
        var descriptor = PathNestedDescriptor.Create("$.vc");

        // Act
        var json = JsonSerializer.Serialize(descriptor);

        // Assert
        json.Should().Contain("\"path\"");
        json.Should().NotContain("path_nested");
    }

    #endregion
}
