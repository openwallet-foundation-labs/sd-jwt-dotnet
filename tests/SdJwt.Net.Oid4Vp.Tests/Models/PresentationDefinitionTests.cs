using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Models;

public class PresentationDefinitionTests
{
    [Fact]
    public void CreateSimple_WithValidParameters_CreatesCorrectDefinition()
    {
        // Arrange
        var id = "test-definition";
        var credentialType = "UniversityDegree";
        var name = "University Degree Verification";
        var purpose = "We need to verify your university degree.";

        // Act
        var definition = PresentationDefinition.CreateSimple(id, credentialType, name, purpose);

        // Assert
        Assert.Equal(id, definition.Id);
        Assert.Equal(name, definition.Name);
        Assert.Equal(purpose, definition.Purpose);
        Assert.Single(definition.InputDescriptors);
        Assert.Equal($"{id}_input", definition.InputDescriptors[0].Id);
    }

    [Fact]
    public void WithSdJwtFormat_AddsCorrectFormatRestrictions()
    {
        // Arrange
        var definition = PresentationDefinition.CreateSimple("test", "TestCredential");
        var algorithms = new[] { "ES256", "ES384" };

        // Act
        definition.WithSdJwtFormat(algorithms);

        // Assert
        Assert.NotNull(definition.Format);
        Assert.True(definition.Format.ContainsKey(Oid4VpConstants.SdJwtVcFormat));
    }

    [Fact]
    public void WithInputDescriptor_AddsDescriptorCorrectly()
    {
        // Arrange
        var definition = PresentationDefinition.CreateSimple("test", "TestCredential");
        var newDescriptor = InputDescriptor.CreateForCredentialType("additional", "AnotherCredential");

        // Act
        definition.WithInputDescriptor(newDescriptor);

        // Assert
        Assert.Equal(2, definition.InputDescriptors.Length);
        Assert.Contains(definition.InputDescriptors, d => d.Id == "additional");
    }

    [Fact]
    public void Validate_WithValidDefinition_DoesNotThrow()
    {
        // Arrange
        var definition = PresentationDefinition.CreateSimple("test", "TestCredential");

        // Act & Assert
        var exception = Record.Exception(() => definition.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithEmptyId_ThrowsInvalidOperationException()
    {
        // Arrange
        var definition = new PresentationDefinition
        {
            Id = "",
            InputDescriptors = new[] { InputDescriptor.CreateForCredentialType("test", "TestCredential") }
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => definition.Validate());
    }

    [Fact]
    public void Validate_WithNoInputDescriptors_ThrowsInvalidOperationException()
    {
        // Arrange
        var definition = new PresentationDefinition
        {
            Id = "test",
            InputDescriptors = Array.Empty<InputDescriptor>()
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => definition.Validate());
    }
}