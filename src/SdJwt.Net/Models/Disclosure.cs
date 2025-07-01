using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;

namespace SdJwt.Net.Models;

/// <summary>
/// Represents a single disclosure containing a salt, claim name, and claim value.
/// The disclosure is serialized into the format: [salt, claim_name, claim_value]
/// and then Base64Url encoded.
/// </summary>
public record Disclosure
{
    /// <summary>
    /// A random value used to uniquely hash the disclosure.
    /// </summary>
    public string Salt { get; }

    /// <summary>
    /// The name of the claim being disclosed. For array elements, this is "...".
    /// </summary>
    public string ClaimName { get; }

    /// <summary>
    /// The value of the claim being disclosed.
    /// </summary>
    public object ClaimValue { get; }

    /// <summary>
    /// The Base64Url encoded representation of this disclosure. This is the value
    /// that is appended to the SD-JWT string.
    /// </summary>
    public string EncodedValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Disclosure"/> class.
    /// </summary>
    /// <param name="salt">The salt for the disclosure.</param>
    /// <param name="claimName">The name of the claim.</param>
    /// <param name="claimValue">The value of the claim.</param>
    public Disclosure(string salt, string claimName, object claimValue)
    {
        Salt = salt;
        ClaimName = claimName;
        ClaimValue = claimValue;

        var disclosureArray = new object[] { Salt, ClaimName, ClaimValue };
        var json = JsonSerializer.Serialize(disclosureArray, SdJwtConstants.DefaultJsonSerializerOptions);
        EncodedValue = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Parses a Base64Url encoded disclosure string into a Disclosure object.
    /// </summary>
    /// <param name="encodedDisclosure">The encoded disclosure string.</param>
    /// <returns>A new <see cref="Disclosure"/> object.</returns>
    /// <exception cref="JsonException">Thrown if the disclosure format is invalid.</exception>
    public static Disclosure Parse(string encodedDisclosure)
    {
        var jsonBytes = Base64UrlEncoder.DecodeBytes(encodedDisclosure);
        var disclosureArray = JsonSerializer.Deserialize<JsonElement[]>(jsonBytes, SdJwtConstants.DefaultJsonSerializerOptions);

        if (disclosureArray is not { Length: 3 })
        {
            throw new JsonException("Disclosure JSON must be an array of 3 elements [salt, name, value].");
        }

        var salt = disclosureArray[0].GetString() ?? throw new JsonException("Disclosure salt cannot be null.");
        var claimName = disclosureArray[1].GetString() ?? throw new JsonException("Disclosure claim name cannot be null.");
        var claimValue = SdJwtUtils.ConvertJsonElement(disclosureArray[2]);

        // Note: We are creating a new disclosure object which re-encodes the value.
        // This is acceptable as the input values will deterministically produce the same encoded output.
        return new Disclosure(salt, claimName, claimValue);
    }
}