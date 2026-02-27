using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using SdJwt.Net.Utils;
using System.Text.Json;

namespace SdJwt.Net.Serialization;

/// <summary>
/// Provides methods to convert between SD-JWT formats and JWS JSON Serialization
/// as defined in RFC 9901 Section 8
/// </summary>
public static class SdJwtJsonSerializer
{
    /// <summary>
    /// Converts an SD-JWT compact serialization to JWS Flattened JSON Serialization
    /// </summary>
    /// <param name="sdJwtCompact">The SD-JWT in compact format</param>
    /// <returns>SD-JWT in JWS Flattened JSON Serialization format</returns>
    /// <exception cref="ArgumentException">Thrown when the SD-JWT format is invalid</exception>
    public static SdJwtJsonSerialization ToFlattenedJsonSerialization(string sdJwtCompact)
    {
        if (string.IsNullOrEmpty(sdJwtCompact))
            throw new ArgumentException("SD-JWT cannot be null or empty", nameof(sdJwtCompact));

        var parsed = SdJwtParser.ParsePresentation(sdJwtCompact);

        // Split the JWT into its components
        var jwtParts = parsed.RawSdJwt.Split('.');
        if (jwtParts.Length != 3)
            throw new ArgumentException("Invalid JWT format", nameof(sdJwtCompact));

        var result = new SdJwtJsonSerialization
        {
            Protected = jwtParts[0], // header
            Payload = jwtParts[1],   // payload
            Signature = jwtParts[2], // signature
            Header = new SdJwtUnprotectedHeader
            {
                Disclosures = parsed.Disclosures.Select(d => d.EncodedValue).ToArray(),
                KbJwt = parsed.RawKeyBindingJwt
            }
        };

        return result;
    }

    /// <summary>
    /// Converts an SD-JWT compact serialization to JWS General JSON Serialization
    /// </summary>
    /// <param name="sdJwtCompact">The SD-JWT in compact format</param>
    /// <param name="additionalSignatures">Optional additional signatures for multi-signature scenarios</param>
    /// <returns>SD-JWT in JWS General JSON Serialization format</returns>
    public static SdJwtGeneralJsonSerialization ToGeneralJsonSerialization(
        string sdJwtCompact,
        SdJwtSignature[]? additionalSignatures = null)
    {
        if (string.IsNullOrEmpty(sdJwtCompact))
            throw new ArgumentException("SD-JWT cannot be null or empty", nameof(sdJwtCompact));

        var parsed = SdJwtParser.ParsePresentation(sdJwtCompact);

        // Split the JWT into its components
        var jwtParts = parsed.RawSdJwt.Split('.');
        if (jwtParts.Length != 3)
            throw new ArgumentException("Invalid JWT format", nameof(sdJwtCompact));

        var signatures = new List<SdJwtSignature>
                {
            // First signature contains disclosures and kb_jwt
            new SdJwtSignature
            {
                Protected = jwtParts[0],
                Signature = jwtParts[2],
                Header = new SdJwtUnprotectedHeader
                {
                    Disclosures = parsed.Disclosures.Select(d => d.EncodedValue).ToArray(),
                    KbJwt = parsed.RawKeyBindingJwt
                }
            }
        };

        // Add any additional signatures (they MUST NOT contain disclosures or kb_jwt)
        if (additionalSignatures != null)
        {
            foreach (var additionalSig in additionalSignatures)
            {
                if (additionalSig.Header.Disclosures.Length > 0 || !string.IsNullOrEmpty(additionalSig.Header.KbJwt))
                    throw new ArgumentException("Additional signatures MUST NOT contain disclosures or kb_jwt");

                signatures.Add(additionalSig);
            }
        }

        return new SdJwtGeneralJsonSerialization
        {
            Payload = jwtParts[1],
            Signatures = signatures.ToArray()
        };
    }

    /// <summary>
    /// Converts JWS Flattened JSON Serialization to SD-JWT compact format
    /// </summary>
    /// <param name="jsonSerialization">The SD-JWT in JWS Flattened JSON Serialization format</param>
    /// <returns>SD-JWT in compact format</returns>
    public static string FromFlattenedJsonSerialization(SdJwtJsonSerialization jsonSerialization)
    {
        if (jsonSerialization == null)
            throw new ArgumentNullException(nameof(jsonSerialization));

        // Reconstruct the JWT
        var jwt = $"{jsonSerialization.Protected}.{jsonSerialization.Payload}.{jsonSerialization.Signature}";

        // Build the SD-JWT
        var parts = new List<string> { jwt };
        parts.AddRange(jsonSerialization.Header.Disclosures);

        if (!string.IsNullOrEmpty(jsonSerialization.Header.KbJwt))
        {
            parts.Add(jsonSerialization.Header.KbJwt);
        }
        else
        {
            parts.Add(string.Empty); // Empty string for SD-JWT without KB-JWT
        }

        return string.Join("~", parts);
    }

    /// <summary>
    /// Converts JWS General JSON Serialization to SD-JWT compact format
    /// </summary>
    /// <param name="generalSerialization">The SD-JWT in JWS General JSON Serialization format</param>
    /// <returns>SD-JWT in compact format (using the first signature)</returns>
    public static string FromGeneralJsonSerialization(SdJwtGeneralJsonSerialization generalSerialization)
    {
        if (generalSerialization == null)
            throw new ArgumentNullException(nameof(generalSerialization));

        if (generalSerialization.Signatures.Length == 0)
            throw new ArgumentException("At least one signature is required", nameof(generalSerialization));

        // Use the first signature (which should contain disclosures and kb_jwt)
        var firstSignature = generalSerialization.Signatures[0];

        // Reconstruct the JWT
        var jwt = $"{firstSignature.Protected}.{generalSerialization.Payload}.{firstSignature.Signature}";

        // Build the SD-JWT
        var parts = new List<string> { jwt };
        parts.AddRange(firstSignature.Header.Disclosures);

        if (!string.IsNullOrEmpty(firstSignature.Header.KbJwt))
        {
            parts.Add(firstSignature.Header.KbJwt);
        }
        else
        {
            parts.Add(string.Empty); // Empty string for SD-JWT without KB-JWT
        }

        return string.Join("~", parts);
    }

    /// <summary>
    /// Calculates the sd_hash for Key Binding JWT when using JWS JSON Serialization
    /// As per RFC 9901 Section 8.1, the digest must be computed over the equivalent compact representation
    /// </summary>
    /// <param name="jsonSerialization">The SD-JWT in JWS JSON Serialization format</param>
    /// <param name="hashAlgorithm">The hash algorithm to use</param>
    /// <returns>Base64url-encoded hash of the SD-JWT compact representation</returns>
    public static string CalculateSdHashForJsonSerialization(SdJwtJsonSerialization jsonSerialization, string hashAlgorithm = "sha-256")
    {
        if (jsonSerialization == null)
        {
            throw new ArgumentNullException(nameof(jsonSerialization));
        }

        var jwt = $"{jsonSerialization.Protected}.{jsonSerialization.Payload}.{jsonSerialization.Signature}";
        var compactParts = new List<string> { jwt };
        compactParts.AddRange(jsonSerialization.Header.Disclosures);
        var sdJwtPart = string.Join(SdJwtConstants.DisclosureSeparator, compactParts) + SdJwtConstants.DisclosureSeparator;

        return SdJwtUtils.CreateDigest(hashAlgorithm, sdJwtPart);
    }

    /// <summary>
    /// Validates a JWS JSON Serialization SD-JWT format
    /// </summary>
    /// <param name="json">JSON string to validate</param>
    /// <returns>True if the format is valid</returns>
    public static bool IsValidJsonSerialization(string json)
    {
        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            // Try to parse as Flattened JSON Serialization first
            var flattened = JsonSerializer.Deserialize<SdJwtJsonSerialization>(json, SdJwtConstants.DefaultJsonSerializerOptions);
            if (flattened != null && !string.IsNullOrEmpty(flattened.Payload) &&
                !string.IsNullOrEmpty(flattened.Signature) && !string.IsNullOrEmpty(flattened.Protected))
            {
                return true;
            }
        }
        catch
        {
            // Fall through to try General JSON Serialization
        }

        try
        {
            // Try to parse as General JSON Serialization
            var general = JsonSerializer.Deserialize<SdJwtGeneralJsonSerialization>(json, SdJwtConstants.DefaultJsonSerializerOptions);
            if (general != null && !string.IsNullOrEmpty(general.Payload) &&
                general.Signatures != null && general.Signatures.Length > 0)
            {
                // Validate that first signature has required fields
                var firstSig = general.Signatures[0];
                return !string.IsNullOrEmpty(firstSig.Protected) && !string.IsNullOrEmpty(firstSig.Signature);
            }
        }
        catch
        {
            // Fall through to return false
        }

        return false;
    }
}
