using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.SiopV2;

/// <summary>
/// Provides subject identifier helpers for SIOPv2.
/// </summary>
public static class SiopSubject
{
    /// <summary>
    /// Creates the JWK thumbprint subject value for the supplied public key.
    /// </summary>
    /// <param name="publicJwk">The subject public JWK.</param>
    /// <returns>The SIOPv2 SHA-256 JWK thumbprint subject identifier.</returns>
    public static string CreateJwkThumbprintSubject(JsonWebKey publicJwk)
    {
        return SiopConstants.SubjectSyntaxTypes.JwkThumbprintSha256Prefix + CreateJwkThumbprint(publicJwk);
    }

    /// <summary>
    /// Creates the bare RFC 7638 SHA-256 JWK thumbprint for the supplied public key.
    /// </summary>
    /// <param name="publicJwk">The subject public JWK.</param>
    /// <returns>The base64url-encoded SHA-256 JWK thumbprint.</returns>
    public static string CreateJwkThumbprint(JsonWebKey publicJwk)
    {
        if (publicJwk == null)
        {
            throw new ArgumentNullException(nameof(publicJwk));
        }

        var canonicalJson = CreateCanonicalJwkThumbprintJson(publicJwk);
        var bytes = Encoding.UTF8.GetBytes(canonicalJson);
#if NET6_0_OR_GREATER
        return Base64UrlEncoder.Encode(SHA256.HashData(bytes));
#else
        using var sha256 = SHA256.Create();
        return Base64UrlEncoder.Encode(sha256.ComputeHash(bytes));
#endif
    }

    /// <summary>
    /// Creates the canonical JSON object used as RFC 7638 JWK thumbprint input.
    /// </summary>
    /// <param name="publicJwk">The public JWK.</param>
    /// <returns>Canonical UTF-8 JSON with lexicographically ordered required members.</returns>
    public static string CreateCanonicalJwkThumbprintJson(JsonWebKey publicJwk)
    {
        if (publicJwk == null)
        {
            throw new ArgumentNullException(nameof(publicJwk));
        }

        return publicJwk.Kty switch
        {
            JsonWebAlgorithmsKeyTypes.EllipticCurve => SerializeRequiredMembers(new SortedDictionary<string, string>
            {
                ["crv"] = Require(publicJwk.Crv, "crv"),
                ["kty"] = Require(publicJwk.Kty, "kty"),
                ["x"] = Require(publicJwk.X, "x"),
                ["y"] = Require(publicJwk.Y, "y")
            }),
            JsonWebAlgorithmsKeyTypes.RSA => SerializeRequiredMembers(new SortedDictionary<string, string>
            {
                ["e"] = Require(publicJwk.E, "e"),
                ["kty"] = Require(publicJwk.Kty, "kty"),
                ["n"] = Require(publicJwk.N, "n")
            }),
            "OKP" => SerializeRequiredMembers(new SortedDictionary<string, string>
            {
                ["crv"] = Require(publicJwk.Crv, "crv"),
                ["kty"] = Require(publicJwk.Kty, "kty"),
                ["x"] = Require(publicJwk.X, "x")
            }),
            _ => throw new NotSupportedException($"JWK key type '{publicJwk.Kty}' is not supported for SIOPv2 JWK thumbprint subjects.")
        };
    }

    /// <summary>
    /// Creates a JSON-serializable public JWK object suitable for the SIOPv2 <c>sub_jwk</c> claim.
    /// </summary>
    /// <param name="publicJwk">The public JWK.</param>
    /// <returns>A dictionary containing the public JWK members required to verify the subject-signed ID Token.</returns>
    public static IReadOnlyDictionary<string, string> CreatePublicJwk(JsonWebKey publicJwk)
    {
        if (publicJwk == null)
        {
            throw new ArgumentNullException(nameof(publicJwk));
        }

        return publicJwk.Kty switch
        {
            JsonWebAlgorithmsKeyTypes.EllipticCurve => new SortedDictionary<string, string>
            {
                ["crv"] = Require(publicJwk.Crv, "crv"),
                ["kty"] = Require(publicJwk.Kty, "kty"),
                ["x"] = Require(publicJwk.X, "x"),
                ["y"] = Require(publicJwk.Y, "y")
            },
            JsonWebAlgorithmsKeyTypes.RSA => new SortedDictionary<string, string>
            {
                ["e"] = Require(publicJwk.E, "e"),
                ["kty"] = Require(publicJwk.Kty, "kty"),
                ["n"] = Require(publicJwk.N, "n")
            },
            "OKP" => new SortedDictionary<string, string>
            {
                ["crv"] = Require(publicJwk.Crv, "crv"),
                ["kty"] = Require(publicJwk.Kty, "kty"),
                ["x"] = Require(publicJwk.X, "x")
            },
            _ => throw new NotSupportedException($"JWK key type '{publicJwk.Kty}' is not supported for SIOPv2 subject-signed ID Tokens.")
        };
    }

    /// <summary>
    /// Serializes a public JWK object suitable for the SIOPv2 <c>sub_jwk</c> claim.
    /// </summary>
    /// <param name="publicJwk">The public JWK.</param>
    /// <returns>A JSON object string containing public JWK members.</returns>
    public static string SerializePublicJwk(JsonWebKey publicJwk)
    {
        return SerializeRequiredMembers(CreatePublicJwk(publicJwk));
    }

    private static string Require(string? value, string memberName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"JWK member '{memberName}' is required for thumbprint computation.");
        }

        return value;
    }

    private static string SerializeRequiredMembers(IReadOnlyDictionary<string, string> members)
    {
        return JsonSerializer.Serialize(members, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
