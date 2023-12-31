﻿namespace Owf.Sd.Jwt;

/// <summary>
/// Represents a Disclosure used in Selective Disclosure JWTs.
/// </summary>
public class Disclosure
{
    private readonly string _salt;
    private readonly string? _claimName;
    private readonly object _claimValue;
    private readonly string _json;
    private readonly string _disclosure;

    /// <summary>
    /// Gets the _salt value.
    /// </summary>
    public string Salt => _salt;

    /// <summary>
    /// Gets the claim name.
    /// </summary>
    public string? ClaimName => _claimName;

    /// <summary>
    /// Gets the claim value.
    /// </summary>
    public object ClaimValue => _claimValue;

    /// <summary>
    /// Gets the JSON representation of the _disclosure.
    /// </summary>
    public string Json => _json;


    /// <summary>
    /// Creates a Disclosure for an object property.
    /// </summary>
    /// <param name="claimValue">The claim value.</param>
    /// <returns>A Disclosure object.</returns>
    public static Disclosure Create(object claimValue)
    {
        return new Disclosure(Utilities.GenerateSalt(), null, claimValue, null, null);
    }

    /// <summary>
    /// Creates a Disclosure for an object property.
    /// </summary>
    /// <param name="claimName">The claim name.</param>
    /// <param name="claimValue">The claim value.</param>
    /// <returns>A Disclosure object.</returns>
    public static Disclosure Create(string claimName, object claimValue)
    {
        return new Disclosure(Utilities.GenerateSalt(), claimName, claimValue, null, null);
    }

    /// <summary>
    /// Creates a Disclosure for an object property with a specified _salt.
    /// </summary>
    /// <param name="salt">The _salt value.</param>
    /// <param name="claimName">The claim name.</param>
    /// <param name="claimValue">The claim value.</param>
    /// <returns>A Disclosure object.</returns>
    public static Disclosure Create(string salt, string claimName, object claimValue)
    {
        return new Disclosure(salt, claimName, claimValue, null, null);
    }

    private Disclosure(string salt, string? claimName, object claimValue, string? json, string? disclosure)
    {
        if (string.IsNullOrWhiteSpace(salt))
        {
            throw new ArgumentException("'salt' is missing.");
        }

        json ??= GenerateJson(salt, claimName, claimValue);

        disclosure ??= Utilities.ToBase64Url(json);

        _salt = salt;
        _claimName = claimName;
        _claimValue = claimValue;
        _json = json;
        _disclosure = disclosure;
    }

    /// <summary>
    /// Gets the Disclosure digest using the specified hash algorithm.
    /// </summary>
    /// <param name="hashAlgorithm">The hash algorithm to use.</param>
    /// <returns>The Disclosure digest.</returns>
    public string Digest(SupportHashAlgorithm hashAlgorithm = SupportHashAlgorithm.SHA256)
    {
        var hal = HashAlgorithmHelper.GetHashAlgorithm(hashAlgorithm);

        return Utilities.ComputeDigest(hal, _disclosure);
    }

    /// <summary>
    /// Creates an array element for this Disclosure.
    /// </summary>
    /// <param name="hashAlgorithm">The hash algorithm to use (default is SHA256).</param>
    /// <returns>A dictionary representing the array element.</returns>
    public Dictionary<string, object> ToArrayElement(SupportHashAlgorithm hashAlgorithm = SupportHashAlgorithm.SHA256)
    {
        // If this _disclosure is for an object property.
        if (ClaimName != null)
        {
            throw new InvalidOperationException("This disclosure is not for an array element.");
        }

        // { "...": "<digest>" }
        return new Dictionary<string, object>
            {
                { Constants.KEY_THREE_DOTS, Digest(hashAlgorithm) }
            };
    }

    /// <summary>
    /// Creates a Disclosure object from a base64url-encoded string.
    /// </summary>
    /// <param name="disclosure">The base64url-encoded Disclosure.</param>
    /// <returns>A Disclosure object or null if the input is invalid.</returns>
    public static Disclosure? CreateFromBase64Url(string disclosure)
    {
        if (string.IsNullOrWhiteSpace(disclosure))
        {
            return null;
        }

        var decodedJson = Utilities.FromBase64Url(disclosure);

        var elements = JsonSerializer.Deserialize<object[]>(decodedJson);

        if (elements is null || elements.Length != 2 && elements.Length != 3)
        {
            throw new ArgumentException("Invalid disclosure content");
        }

        var mySalt = ExtractSalt(elements[0]);
        var myClaimName = ExtractClaimName(elements);
        var myClaimValue = ExtractClaimValue(elements);

        if (myClaimName != null && Utilities.IsReservedKey(myClaimName))
        {
            throw new ArgumentException($"The claim name ('{myClaimName}') is a reserved key.");
        }

        return new Disclosure(mySalt, myClaimName, myClaimValue, decodedJson, disclosure);
    }

    /// <summary>
    /// Gets the base64url-encoded Disclosure.
    /// </summary>
    /// <returns>The base64url-encoded Disclosure string.</returns>
    public string GetBase64Url()
    {
        return _disclosure;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _disclosure;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _disclosure.GetHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (this == obj)
        {
            return true;
        }

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (Disclosure)obj;
        return _disclosure.Equals(other._disclosure);
    }

    private static string ExtractSalt(object salt)
    {
        if (salt is not string && salt is not JsonElement)
        {
            throw new ArgumentException("The salt object must be of type string or JsonElement.");
        }

        if (salt is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
        {
            return jsonElement.GetString()!;
        }

        return salt?.ToString()!;
    }

    private static string? ExtractClaimName(object[] elements)
    {
        if (elements.Length == 2)
        {
            return null;
        }

        var myClaimName = elements[1];

        if (myClaimName is not string && myClaimName is not JsonElement)
        {
            throw new ArgumentException("The claimName object must be of type string or JsonElement.");
        }

        if (myClaimName is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
        {
            return jsonElement.GetString()!;
        }

        return myClaimName?.ToString()!;
    }

    private static object ExtractClaimValue(object[] elements)
    {
        return elements[^1];
    }

    private static string GenerateJson(string salt, string? claimName, object claimValue)
    {
        var elements = claimName == null
            ? ImmutableList.Create(salt, claimValue)
            : ImmutableList.Create(salt, claimName, claimValue);

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        return JsonSerializer.Serialize(elements, options);
    }
}