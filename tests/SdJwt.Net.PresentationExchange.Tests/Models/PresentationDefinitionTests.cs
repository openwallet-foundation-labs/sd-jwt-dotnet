using FluentAssertions;
using SdJwt.Net.PresentationExchange.Models;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Models;

/// <summary>
/// Tests for the PresentationDefinition model class.
/// </summary>
public class PresentationDefinitionTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateValidDefinition()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id", "Test Descriptor");
        
        // Act
        var definition = PresentationDefinition.Create(
            "test-def",
            new[] { descriptor },
            "Test Definition",
            "Test purpose");

        // Assert
        definition.Should().NotBeNull();
        definition.Id.Should().Be("test-def");
        definition.Name.Should().Be("Test Definition");
        definition.Purpose.Should().Be("Test purpose");
        definition.InputDescriptors.Should().HaveCount(1);
        definition.InputDescriptors[0].Should().Be(descriptor);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PresentationDefinition.Create("", new[] { descriptor }));
    }

    [Fact]
    public void Create_WithNullDescriptors_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PresentationDefinition.Create("test-def", null!));
    }

    [Fact]
    public void Create_WithEmptyDescriptors_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PresentationDefinition.Create("test-def", Array.Empty<InputDescriptor>()));
    }

    [Fact]
    public void Validate_WithValidDefinition_ShouldNotThrow()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id", "Test Descriptor");
        var definition = PresentationDefinition.Create("test-def", new[] { descriptor });

        // Act & Assert
        definition.Invoking(d => d.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var definition = new PresentationDefinition
        {
            Id = "",
            InputDescriptors = new[] { InputDescriptor.Create("test-id") }
        };

        // Act & Assert
        definition.Invoking(d => d.Validate()).Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Presentation definition ID is required");
    }

    [Fact]
    public void Validate_WithNoInputDescriptors_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var definition = new PresentationDefinition
        {
            Id = "test-def",
            InputDescriptors = Array.Empty<InputDescriptor>()
        };

        // Act & Assert
        definition.Invoking(d => d.Validate()).Should()
            .Throw<InvalidOperationException>()
            .WithMessage("At least one input descriptor is required");
    }

    [Fact]
    public void Validate_WithSubmissionRequirements_ShouldValidateReferences()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");
        var requirement = SubmissionRequirement.CreateAll("test-id");
        
        var definition = new PresentationDefinition
        {
            Id = "test-def",
            InputDescriptors = new[] { descriptor },
            SubmissionRequirements = new[] { requirement }
        };

        // Act & Assert
        definition.Invoking(d => d.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithInvalidSubmissionRequirementReference_ShouldThrow()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");
        var requirement = SubmissionRequirement.CreateAll("non-existent-id");
        
        var definition = new PresentationDefinition
        {
            Id = "test-def",
            InputDescriptors = new[] { descriptor },
            SubmissionRequirements = new[] { requirement }
        };

        // Act & Assert
        definition.Invoking(d => d.Validate()).Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Submission requirement references unknown input descriptor: non-existent-id");
    }

    [Fact]
    public void GetReferencedDescriptorIds_WithoutSubmissionRequirements_ShouldReturnAllDescriptors()
    {
        // Arrange
        var descriptor1 = InputDescriptor.Create("id-1");
        var descriptor2 = InputDescriptor.Create("id-2");
        var definition = PresentationDefinition.Create("test-def", new[] { descriptor1, descriptor2 });

        // Act
        var referencedIds = definition.GetReferencedDescriptorIds();

        // Assert
        referencedIds.Should().HaveCount(2);
        referencedIds.Should().Contain("id-1");
        referencedIds.Should().Contain("id-2");
    }

    [Fact]
    public void GetReferencedDescriptorIds_WithSubmissionRequirements_ShouldReturnReferencedOnly()
    {
        // Arrange
        var descriptor1 = InputDescriptor.Create("id-1");
        var descriptor2 = InputDescriptor.Create("id-2");
        var requirement = SubmissionRequirement.CreateAll("id-1");
        
        var definition = new PresentationDefinition
        {
            Id = "test-def",
            InputDescriptors = new[] { descriptor1, descriptor2 },
            SubmissionRequirements = new[] { requirement }
        };

        // Act
        var referencedIds = definition.GetReferencedDescriptorIds();

        // Assert
        referencedIds.Should().HaveCount(1);
        referencedIds.Should().Contain("id-1");
        referencedIds.Should().NotContain("id-2");
    }

    [Fact]
    public void GetInputDescriptor_WithValidId_ShouldReturnDescriptor()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id", "Test Descriptor");
        var definition = PresentationDefinition.Create("test-def", new[] { descriptor });

        // Act
        var result = definition.GetInputDescriptor("test-id");

        // Assert
        result.Should().Be(descriptor);
    }

    [Fact]
    public void GetInputDescriptor_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");
        var definition = PresentationDefinition.Create("test-def", new[] { descriptor });

        // Act
        var result = definition.GetInputDescriptor("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RequiresAllDescriptors_WithoutSubmissionRequirements_ShouldReturnTrue()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");
        var definition = PresentationDefinition.Create("test-def", new[] { descriptor });

        // Act
        var result = definition.RequiresAllDescriptors();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RequiresAllDescriptors_WithAllRule_ShouldReturnTrue()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");
        var requirement = SubmissionRequirement.CreateAll("test-id");
        
        var definition = new PresentationDefinition
        {
            Id = "test-def",
            InputDescriptors = new[] { descriptor },
            SubmissionRequirements = new[] { requirement }
        };

        // Act
        var result = definition.RequiresAllDescriptors();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RequiresAllDescriptors_WithPickRule_ShouldReturnFalse()
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");
        var requirement = SubmissionRequirement.CreatePick("test-id", 1);
        
        var definition = new PresentationDefinition
        {
            Id = "test-def",
            InputDescriptors = new[] { descriptor },
            SubmissionRequirements = new[] { requirement }
        };

        // Act
        var result = definition.RequiresAllDescriptors();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("test-def-1")]
    [InlineData("complex_definition_id")]
    [InlineData("def-with-numbers-123")]
    public void Id_Property_ShouldAcceptValidIds(string validId)
    {
        // Arrange
        var descriptor = InputDescriptor.Create("test-id");
        
        // Act
        var definition = PresentationDefinition.Create(validId, new[] { descriptor });

        // Assert
        definition.Id.Should().Be(validId);
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var definition = new PresentationDefinition();
        
        // Act
        definition.Id = "test-id";
        definition.Name = "Test Name";
        definition.Purpose = "Test Purpose";

        // Assert
        definition.Id.Should().Be("test-id");
        definition.Name.Should().Be("Test Name");
        definition.Purpose.Should().Be("Test Purpose");
    }
}