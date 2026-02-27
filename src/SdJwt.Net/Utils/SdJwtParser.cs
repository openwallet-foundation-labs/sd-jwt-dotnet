using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SdJwt.Net.Utils;

/// <summary>
/// A utility class for parsing and inspecting raw SD-JWT strings and related artifacts
/// without performing cryptographic verification. This is useful for debugging and interoperability testing.
/// </summary>
public static class SdJwtParser
{
    /// <summary>
    /// Parses a combined SD-JWT issuance string (e.g., from a file or QR code).
    /// </summary>
    /// <param name="issuance">The full issuance string, including the SD-JWT and disclosures, separated by '~'.</param>
    /// <returns>A <see cref="ParsedSdJwt"/> object containing the structured, unverified data.</returns>
    public static ParsedSdJwt ParseIssuance(string issuance)
    {
        if (string.IsNullOrWhiteSpace(issuance))
        {
            throw new ArgumentException("The issuance string cannot be null or whitespace.", nameof(issuance));
        }

        var parts = issuance.Split(SdJwtConstants.DisclosureSeparator, StringSplitOptions.None);
        if (parts.Length < 2 || parts[^1] != string.Empty)
        {
            throw new FormatException("Invalid SD-JWT issuance format: missing required trailing separator.");
        }

        var sdJwt = parts[0];

        var disclosures = parts.Skip(1)
                                .Take(parts.Length - 2)
                                .Select(p => string.IsNullOrWhiteSpace(p)
                                    ? throw new FormatException("Invalid SD-JWT issuance format: disclosure entries must not be empty.")
                                    : Disclosure.Parse(p))
                                .ToList();

        var unverifiedJwt = new JwtSecurityToken(sdJwt);
        return new ParsedSdJwt(sdJwt, unverifiedJwt, disclosures);
    }

    /// <summary>
    /// Parses an SD-JWT presentation containing an SD-JWT, key-binding JWT, and disclosures.
    /// </summary>
    /// <param name="presentation">The complete presentation string to parse.</param>
    /// <returns>A parsed presentation object containing the SD-JWT, key-binding JWT, and disclosures.</returns>
    /// <exception cref="ArgumentNullException">Thrown when presentation is null or empty.</exception>
    /// <exception cref="FormatException">Thrown when the presentation format is invalid.</exception>
    public static ParsedPresentation ParsePresentation(string presentation)
    {

        if (string.IsNullOrWhiteSpace(presentation))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(presentation));
        }


        var parts = presentation.Split([SdJwtConstants.DisclosureSeparator], StringSplitOptions.None);

        if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]))
        {
            throw new ArgumentException("Presentation string is empty or invalid.", nameof(presentation));
        }

        // The first part is always the SD-JWT.
        var sdJwt = parts[0];
        var unverifiedSdJwt = new JwtSecurityToken(sdJwt);

        var disclosures = new List<Disclosure>();
        string? kbJwt = null;
        JwtSecurityToken? unverifiedKbJwt = null;
        bool isKeyBindingPresentation;

        if (parts[^1] == string.Empty)
        {
            isKeyBindingPresentation = false;
            for (int i = 1; i < parts.Length - 1; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                {
                    throw new FormatException("Invalid SD-JWT presentation format: disclosure entries must not be empty.");
                }
                disclosures.Add(Disclosure.Parse(parts[i]));
            }
        }
        else
        {
            isKeyBindingPresentation = true;
            kbJwt = parts[^1];
            if (!IsKeyBindingJwt(kbJwt))
            {
                throw new FormatException("Invalid SD-JWT+KB presentation format: final component is not a valid KB-JWT.");
            }
            unverifiedKbJwt = new JwtSecurityToken(kbJwt);

            for (int i = 1; i < parts.Length - 1; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                {
                    throw new FormatException("Invalid SD-JWT+KB presentation format: disclosure entries must not be empty.");
                }
                disclosures.Add(Disclosure.Parse(parts[i]));
            }
        }

        var compactParts = new List<string> { sdJwt };
        compactParts.AddRange(disclosures.Select(d => d.EncodedValue));
        var compactSdJwt = string.Join(SdJwtConstants.DisclosureSeparator, compactParts) + SdJwtConstants.DisclosureSeparator;

        return new ParsedPresentation(sdJwt, unverifiedSdJwt, disclosures, kbJwt, unverifiedKbJwt, compactSdJwt, isKeyBindingPresentation);
    }

    /// <summary>
    /// A helper method to heuristically determine if a string is a Key Binding JWT.
    /// </summary>
    private static bool IsKeyBindingJwt(string potentialKbJwt)
    {
        if (string.IsNullOrWhiteSpace(potentialKbJwt) || potentialKbJwt.Count(c => c == '.') != 2)
        {
            return false;
        }

        try
        {
            var headerPart = potentialKbJwt.Split('.')[0];
            var headerJson = Base64UrlEncoder.Decode(headerPart);
            using var jsonDoc = JsonDocument.Parse(headerJson);

            if (jsonDoc.RootElement.TryGetProperty("typ", out var typElement))
            {
                var typ = typElement.GetString();
                // RFC 9901 Section 4.3: The Key Binding JWT MUST have a typ (Type) Header Parameter with the value kb+jwt.
                return typ == SdJwtConstants.KbJwtHeaderType;
            }
            return false;
        }
        catch (Exception)
        {
            // If any part of the parsing fails, it's not a well-formed JWT.
            return false;
        }
    }

    /// <summary>
    /// Parses a JSON file from the filesystem, expecting it to contain a specific data structure.
    /// This is useful for loading test cases or interoperability profiles.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="filePath">The absolute or relative path to the JSON file.</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T? ParseJsonFile<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified JSON file was not found.", filePath);
        }

        var jsonContent = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<T>(jsonContent, SdJwtConstants.DefaultJsonSerializerOptions);
    }

    /// <summary>
    /// Parses a JSON content, expecting it to contain a specific data structure.
    /// This is useful for loading test cases or interoperability profiles.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="jsonContent">The JSON contents.</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T? ParseJson<T>(string jsonContent)
    {
        return JsonSerializer.Deserialize<T>(jsonContent, SdJwtConstants.DefaultJsonSerializerOptions);
    }
}
