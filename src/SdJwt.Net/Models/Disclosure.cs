using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.Models;

/// <summary>
/// Represents a single disclosure containing a salt, claim name, and claim value.
/// The disclosure is serialized into the format: [salt, claim_name, claim_value]
/// and then Base64Url encoded.
/// </summary>
/// <summary>
/// Represents a single disclosure containing a salt, claim name, and claim value.
/// </summary>
public record Disclosure
{
    public string Salt { get; }
    public string ClaimName { get; }
    public object ClaimValue { get; }
    public string EncodedValue { get; }

    /// <summary>
    /// Initializes a new instance for the Issuer.
    /// </summary>
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
    /// Private constructor for the Parser to preserve the original encoded value.
    /// </summary>
    private Disclosure(string salt, string claimName, object claimValue, string encodedValue)
    {
        Salt = salt;
        ClaimName = claimName;
        ClaimValue = claimValue;
        EncodedValue = encodedValue;
    }

    /// <summary>
    /// Parses a Base64Url encoded disclosure string into a Disclosure object
    /// while preserving the original encoded value to ensure digest consistency.
    /// </summary>
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

        return new Disclosure(salt, claimName, claimValue, encodedDisclosure);
    }
}