using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Coverage;

/// <summary>
/// Tests for <see cref="PresentationSubmission"/> class, covering creation,
/// validation, fluent builder methods, and JSON serialization.
/// </summary>
public class PresentationSubmissionTests
{
    /// <summary>
    /// Tests that CreateSingle() returns a valid instance with all parameters.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithValidParams_ReturnsInstance()
    {
        // Act
        var result = PresentationSubmission.CreateSingle(
            "sub-1", "def-1", "desc-1", "vc+sd-jwt", "$");

        // Assert
        result.Id.Should().Be("sub-1");
        result.DefinitionId.Should().Be("def-1");
        result.DescriptorMap.Should().HaveCount(1);
        result.DescriptorMap[0].Id.Should().Be("desc-1");
        result.DescriptorMap[0].Format.Should().Be("vc+sd-jwt");
        result.DescriptorMap[0].Path.Should().Be("$");
    }

    /// <summary>
    /// Tests that CreateSingle() uses default path when not provided.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithDefaultPath_UsesRootPath()
    {
        // Act
        var result = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Assert
        result.DescriptorMap[0].Path.Should().Be("$");
    }

    /// <summary>
    /// Tests that CreateSingle() throws ArgumentException when id is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateSingle(null!, "def-1", "desc-1", "vc+sd-jwt");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateSingle() throws ArgumentException when id is empty.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateSingle("", "def-1", "desc-1", "vc+sd-jwt");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateSingle() throws ArgumentException when definitionId is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithNullDefinitionId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateSingle("sub-1", null!, "desc-1", "vc+sd-jwt");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateSingle() throws ArgumentException when definitionId is empty.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithEmptyDefinitionId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateSingle("sub-1", "", "desc-1", "vc+sd-jwt");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateSingle() throws ArgumentException when inputDescriptorId is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithNullInputDescriptorId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateSingle("sub-1", "def-1", null!, "vc+sd-jwt");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateSingle() throws ArgumentException when format is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithNullFormat_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateSingle() throws ArgumentException when path is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateSingle_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt", null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateMultiple() returns a valid instance with multiple mappings.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateMultiple_WithValidParams_ReturnsInstance()
    {
        // Arrange
        var mapping1 = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$[0]");
        var mapping2 = InputDescriptorMapping.Create("desc-2", "vc+sd-jwt", "$[1]");

        // Act
        var result = PresentationSubmission.CreateMultiple("sub-1", "def-1", mapping1, mapping2);

        // Assert
        result.Id.Should().Be("sub-1");
        result.DefinitionId.Should().Be("def-1");
        result.DescriptorMap.Should().HaveCount(2);
        result.DescriptorMap[0].Id.Should().Be("desc-1");
        result.DescriptorMap[1].Id.Should().Be("desc-2");
    }

    /// <summary>
    /// Tests that CreateMultiple() throws ArgumentException when id is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateMultiple_WithNullId_ThrowsArgumentException()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");

        // Act & Assert
        var act = () => PresentationSubmission.CreateMultiple(null!, "def-1", mapping);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateMultiple() throws ArgumentException when definitionId is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateMultiple_WithNullDefinitionId_ThrowsArgumentException()
    {
        // Arrange
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");

        // Act & Assert
        var act = () => PresentationSubmission.CreateMultiple("sub-1", null!, mapping);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateMultiple() throws ArgumentNullException when mappings is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateMultiple_WithNullMappings_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateMultiple("sub-1", "def-1", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that CreateMultiple() throws ArgumentException when mappings is empty.
    /// </summary>
    [Fact]
    public void PresentationSubmission_CreateMultiple_WithEmptyMappings_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => PresentationSubmission.CreateMultiple("sub-1", "def-1");
        act.Should().Throw<ArgumentException>().WithMessage("*At least one mapping*");
    }

    /// <summary>
    /// Tests that WithMapping() adds a mapping to an existing submission.
    /// </summary>
    [Fact]
    public void PresentationSubmission_WithMapping_AddsMapping()
    {
        // Arrange
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");
        var newMapping = InputDescriptorMapping.Create("desc-2", "vc+sd-jwt", "$[1]");

        // Act
        var result = submission.WithMapping(newMapping);

        // Assert
        result.Should().BeSameAs(submission);
        result.DescriptorMap.Should().HaveCount(2);
        result.DescriptorMap[1].Id.Should().Be("desc-2");
    }

    /// <summary>
    /// Tests that WithMapping() throws ArgumentNullException when mapping is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_WithMapping_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act & Assert
        var act = () => submission.WithMapping(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that WithMapping() works when DescriptorMap is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_WithMapping_WhenDescriptorMapNull_InitializesArray()
    {
        // Arrange
        var submission = new PresentationSubmission
        {
            Id = "sub-1",
            DefinitionId = "def-1",
            DescriptorMap = null!
        };
        var mapping = InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$");

        // Act
        var result = submission.WithMapping(mapping);

        // Assert
        result.DescriptorMap.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid submission.
    /// </summary>
    [Fact]
    public void PresentationSubmission_Validate_WithValidSubmission_DoesNotThrow()
    {
        // Arrange
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act & Assert
        var act = () => submission.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when Id is empty.
    /// </summary>
    [Fact]
    public void PresentationSubmission_Validate_WithEmptyId_ThrowsInvalidOperationException()
    {
        // Arrange
        var submission = new PresentationSubmission
        {
            Id = "",
            DefinitionId = "def-1",
            DescriptorMap = new[] { InputDescriptorMapping.Create("desc-1", "vc+sd-jwt") }
        };

        // Act & Assert
        var act = () => submission.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*id*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when DefinitionId is empty.
    /// </summary>
    [Fact]
    public void PresentationSubmission_Validate_WithEmptyDefinitionId_ThrowsInvalidOperationException()
    {
        // Arrange
        var submission = new PresentationSubmission
        {
            Id = "sub-1",
            DefinitionId = "",
            DescriptorMap = new[] { InputDescriptorMapping.Create("desc-1", "vc+sd-jwt") }
        };

        // Act & Assert
        var act = () => submission.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*definition_id*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when DescriptorMap is null.
    /// </summary>
    [Fact]
    public void PresentationSubmission_Validate_WithNullDescriptorMap_ThrowsInvalidOperationException()
    {
        // Arrange
        var submission = new PresentationSubmission
        {
            Id = "sub-1",
            DefinitionId = "def-1",
            DescriptorMap = null!
        };

        // Act & Assert
        var act = () => submission.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*descriptor mapping*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when DescriptorMap is empty.
    /// </summary>
    [Fact]
    public void PresentationSubmission_Validate_WithEmptyDescriptorMap_ThrowsInvalidOperationException()
    {
        // Arrange
        var submission = new PresentationSubmission
        {
            Id = "sub-1",
            DefinitionId = "def-1",
            DescriptorMap = Array.Empty<InputDescriptorMapping>()
        };

        // Act & Assert
        var act = () => submission.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*descriptor mapping*required*");
    }

    /// <summary>
    /// Tests that Validate() cascades validation to descriptor mappings.
    /// </summary>
    [Fact]
    public void PresentationSubmission_Validate_CascadesToDescriptorMappings()
    {
        // Arrange
        var submission = new PresentationSubmission
        {
            Id = "sub-1",
            DefinitionId = "def-1",
            DescriptorMap = new[]
            {
                new InputDescriptorMapping { Id = "", Format = "vc+sd-jwt", Path = "$" }  // Invalid
            }
        };

        // Act & Assert
        var act = () => submission.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that Validate() detects duplicate input descriptor IDs.
    /// </summary>
    [Fact]
    public void PresentationSubmission_Validate_WithDuplicateIds_ThrowsInvalidOperationException()
    {
        // Arrange
        var submission = PresentationSubmission.CreateMultiple(
            "sub-1", "def-1",
            InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$[0]"),
            InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$[1]")  // Duplicate ID
        );

        // Act & Assert
        var act = () => submission.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Duplicate*desc-1*");
    }

    /// <summary>
    /// Tests JSON serialization of PresentationSubmission.
    /// </summary>
    [Fact]
    public void PresentationSubmission_JsonSerialization_RoundTripsCorrectly()
    {
        // Arrange
        var submission = PresentationSubmission.CreateMultiple(
            "sub-1", "def-1",
            InputDescriptorMapping.Create("desc-1", "vc+sd-jwt", "$[0]"),
            InputDescriptorMapping.Create("desc-2", "dc+sd-jwt", "$[1]")
        );

        // Act
        var json = JsonSerializer.Serialize(submission);
        var deserialized = JsonSerializer.Deserialize<PresentationSubmission>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be("sub-1");
        deserialized.DefinitionId.Should().Be("def-1");
        deserialized.DescriptorMap.Should().HaveCount(2);
    }
}
