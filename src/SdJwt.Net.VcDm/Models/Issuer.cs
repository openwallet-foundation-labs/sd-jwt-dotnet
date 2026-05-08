using System.Text.Json.Serialization;
using SdJwt.Net.VcDm.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Represents the W3C VCDM 2.0 <c>issuer</c> property, which may be either a URL string
/// or an object containing an <c>id</c> URL plus optional human-readable metadata.
/// Serialized/deserialized via <see cref="IssuerJsonConverter"/>.
/// </summary>
[JsonConverter(typeof(IssuerJsonConverter))]
public sealed class Issuer
{
    /// <summary>
    /// The URL identifying the issuer. REQUIRED.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Optional human-readable name for the issuer (may be a language map).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional human-readable description for the issuer.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Additional properties present in the issuer object. Populated during deserialization
    /// of issuer objects with extension fields.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties { get; init; }

    /// <summary>Initializes an <see cref="Issuer"/> with the given URL.</summary>
    public Issuer(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Issuer id must not be null or empty.", nameof(id));
        Id = id;
    }

    /// <summary>
    /// Returns true when the issuer was originally serialized as a plain string URL
    /// (i.e., has no Name, Description, or additional properties).
    /// </summary>
    public bool IsSimpleUrl => Name is null && Description is null &&
                               (AdditionalProperties is null || AdditionalProperties.Count == 0);

    /// <summary>Returns the issuer's URL identifier.</summary>
    public override string ToString() => Id;

    /// <summary>Implicitly converts a URL string to an <see cref="Issuer"/>.</summary>
    public static implicit operator Issuer(string url) => new(url);
}
