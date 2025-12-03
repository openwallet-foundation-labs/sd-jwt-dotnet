using SdJwt.Net.Vc.Models;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

public class SdJwtVcModelsTests
{
    [Fact]
    public void CredentialSubject_Serialization_RoundTrip()
    {
        var subject = new CredentialSubject
        {
            GivenName = "John",
            FamilyName = "Doe"
        };

        var json = JsonSerializer.Serialize(subject);
        var deserialized = JsonSerializer.Deserialize<CredentialSubject>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("John", deserialized.GivenName);
        Assert.Equal("Doe", deserialized.FamilyName);
    }

    [Fact]
    public void DisplayMetadata_Serialization_RoundTrip()
    {
        var display = new DisplayMetadata
        {
            Name = "Test Credential",
            Locale = "en-US",
            Description = "A test credential",
            Label = "Test Label"
        };

        var json = JsonSerializer.Serialize(display);
        var deserialized = JsonSerializer.Deserialize<DisplayMetadata>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("Test Credential", deserialized.Name);
        Assert.Equal("en-US", deserialized.Locale);
        Assert.Equal("A test credential", deserialized.Description);
        Assert.Equal("Test Label", deserialized.Label);
    }

    [Fact]
    public void TypeMetadata_Serialization_RoundTrip()
    {
        var typeMetadata = new TypeMetadata
        {
            Vct = "https://example.com/vct",
            Name = "Test Type",
            Description = "Test Description",
            Extends = "https://example.com/base",
            ExtendsIntegrity = "sha256-hash",
            Claims = new[]
            {
                new ClaimMetadata { Mandatory = true, Path = new[] { "name" } }
            }
        };

        var json = JsonSerializer.Serialize(typeMetadata);
        var deserialized = JsonSerializer.Deserialize<TypeMetadata>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("https://example.com/vct", deserialized.Vct);
        Assert.Equal("Test Type", deserialized.Name);
        Assert.Equal("Test Description", deserialized.Description);
        Assert.Equal("https://example.com/base", deserialized.Extends);
        Assert.Equal("sha256-hash", deserialized.ExtendsIntegrity);
        Assert.NotNull(deserialized.Claims);
        Assert.True(deserialized.Claims[0].Mandatory);
    }

    [Fact]
    public void JwkSet_Serialization_RoundTrip()
    {
        var jwkSet = new JwkSet
        {
            Keys = new[]
            {
                new JsonWebKey { KeyType = "EC", KeyId = "key1", Curve = "P-256", X = "x", Y = "y" }
            }
        };

        var json = JsonSerializer.Serialize(jwkSet);
        var deserialized = JsonSerializer.Deserialize<JwkSet>(json);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Keys);
        Assert.Single(deserialized.Keys);
        Assert.Equal("key1", deserialized.Keys[0].KeyId);
    }

    [Fact]
    public void Address_Serialization_RoundTrip()
    {
        var address = new Address
        {
            StreetAddress = "123 Main St",
            Locality = "City",
            Region = "State",
            PostalCode = "12345",
            Country = "US"
        };

        var json = JsonSerializer.Serialize(address);
        var deserialized = JsonSerializer.Deserialize<Address>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("123 Main St", deserialized.StreetAddress);
        Assert.Equal("City", deserialized.Locality);
        Assert.Equal("State", deserialized.Region);
        Assert.Equal("12345", deserialized.PostalCode);
        Assert.Equal("US", deserialized.Country);
    }
}
