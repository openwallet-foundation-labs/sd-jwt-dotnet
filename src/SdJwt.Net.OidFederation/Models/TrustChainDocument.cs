using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Represents a Trust Chain encoded as the OpenID Federation <c>application/trust-chain+json</c> media type.
/// </summary>
public sealed class TrustChainDocument
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly string[] _entries;

    private TrustChainDocument(IEnumerable<string> entries)
    {
        _entries = entries.ToArray();
    }

    /// <summary>
    /// Gets the Entity Statement JWTs in Trust Chain order, starting with the subject Entity Configuration.
    /// </summary>
    public IReadOnlyList<string> Entries => _entries;

    /// <summary>
    /// Gets the number of Entity Statements in the Trust Chain.
    /// </summary>
    public int Count => _entries.Length;

    /// <summary>
    /// Creates a Trust Chain document from compact JWT entries.
    /// </summary>
    /// <param name="entries">The Trust Chain entries.</param>
    /// <returns>A Trust Chain document.</returns>
    public static TrustChainDocument Create(params string[] entries)
    {
        return Create((IEnumerable<string>)entries);
    }

    /// <summary>
    /// Creates a Trust Chain document from compact JWT entries.
    /// </summary>
    /// <param name="entries">The Trust Chain entries.</param>
    /// <returns>A Trust Chain document.</returns>
    public static TrustChainDocument Create(IEnumerable<string> entries)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(entries);
#else
        if (entries == null)
            throw new ArgumentNullException(nameof(entries));
#endif

        var document = new TrustChainDocument(entries);
        document.ValidateSyntax();
        return document;
    }

    /// <summary>
    /// Parses an <c>application/trust-chain+json</c> document.
    /// </summary>
    /// <param name="json">The JSON array containing compact Entity Statement JWTs.</param>
    /// <returns>The parsed Trust Chain document.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the document is malformed.</exception>
    public static TrustChainDocument FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("Trust Chain JSON must not be empty.");
        }

        string[]? entries;
        try
        {
            entries = JsonSerializer.Deserialize<string[]>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Trust Chain JSON must be an array of compact JWT strings: {ex.Message}", ex);
        }

        if (entries == null)
        {
            throw new InvalidOperationException("Trust Chain JSON must be an array of compact JWT strings.");
        }

        return Create(entries);
    }

    /// <summary>
    /// Serializes the Trust Chain to <c>application/trust-chain+json</c>.
    /// </summary>
    /// <returns>A JSON array containing compact Entity Statement JWTs.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(_entries, JsonOptions);
    }

    /// <summary>
    /// Validates that the document is a non-empty array of compact JWT strings.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the document is malformed.</exception>
    public void ValidateSyntax()
    {
        if (_entries.Length == 0)
        {
            throw new InvalidOperationException("Trust Chain must contain at least one Entity Statement.");
        }

        for (var i = 0; i < _entries.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(_entries[i]))
            {
                throw new InvalidOperationException($"Trust Chain entry {i} must not be empty.");
            }

            if (!LooksLikeCompactJwt(_entries[i]))
            {
                throw new InvalidOperationException($"Trust Chain entry {i} must be a compact JWT.");
            }
        }
    }

    /// <summary>
    /// Validates issuer/subject continuity for the Trust Chain.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the chain is structurally invalid.</exception>
    public void ValidateContinuity()
    {
        ValidateSyntax();

        var tokens = _entries.Select(ReadJwtToken).ToArray();
        var firstIssuer = GetRequiredClaim(tokens[0], JwtRegisteredClaimNames.Iss, 0);
        var firstSubject = GetRequiredClaim(tokens[0], JwtRegisteredClaimNames.Sub, 0);

        if (!string.Equals(firstIssuer, firstSubject, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The first Trust Chain entry must be the subject Entity Configuration.");
        }

        for (var i = 1; i < tokens.Length; i++)
        {
            var previousIssuer = GetRequiredClaim(tokens[i - 1], JwtRegisteredClaimNames.Iss, i - 1);
            var currentSubject = GetRequiredClaim(tokens[i], JwtRegisteredClaimNames.Sub, i);

            if (!string.Equals(currentSubject, previousIssuer, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Trust Chain entry {i} does not continue from the previous issuer.");
            }
        }
    }

    /// <summary>
    /// Gets the Trust Chain expiration time, defined as the earliest <c>exp</c> value in the chain.
    /// </summary>
    /// <returns>The earliest expiration time as seconds since Unix epoch.</returns>
    /// <exception cref="InvalidOperationException">Thrown when any entry does not contain a valid <c>exp</c> claim.</exception>
    public long GetExpirationTime()
    {
        ValidateSyntax();

        long? earliest = null;
        for (var i = 0; i < _entries.Length; i++)
        {
            var token = ReadJwtToken(_entries[i], i);
            var expiration = GetRequiredUnixTimeClaim(token, JwtRegisteredClaimNames.Exp, i);
            earliest = earliest.HasValue ? Math.Min(earliest.Value, expiration) : expiration;
        }

        return earliest!.Value;
    }

    /// <summary>
    /// Gets the Entity Identifiers that appear in the Trust Chain path.
    /// </summary>
    /// <returns>Entity Identifiers ordered from subject towards trust anchor.</returns>
    public string[] GetEntityIdentifiers()
    {
        ValidateSyntax();

        var identifiers = new List<string>();
        foreach (var entry in _entries)
        {
            var token = ReadJwtToken(entry);
            var subject = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var issuer = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;

            if (!string.IsNullOrWhiteSpace(subject) && !identifiers.Contains(subject, StringComparer.Ordinal))
            {
                identifiers.Add(subject);
            }

            if (!string.IsNullOrWhiteSpace(issuer) && !identifiers.Contains(issuer, StringComparer.Ordinal))
            {
                identifiers.Add(issuer);
            }
        }

        return identifiers.ToArray();
    }

    private static bool LooksLikeCompactJwt(string value)
    {
        var parts = value.Split('.');
        return parts.Length == 3 &&
            parts[0].Length > 0 &&
            parts[1].Length > 0;
    }

    private static JwtSecurityToken ReadJwtToken(string jwt, int index = -1)
    {
        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        }
        catch (Exception ex)
        {
            var label = index >= 0 ? $"Trust Chain entry {index}" : "Trust Chain entry";
            throw new InvalidOperationException($"{label} is not a readable compact JWT: {ex.Message}", ex);
        }
    }

    private static string GetRequiredClaim(JwtSecurityToken token, string claimType, int index)
    {
        var value = token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Trust Chain entry {index} is missing required '{claimType}' claim.");
        }

        return value;
    }

    private static long GetRequiredUnixTimeClaim(JwtSecurityToken token, string claimType, int index)
    {
        var value = GetRequiredClaim(token, claimType, index);
        if (!long.TryParse(value, out var unixTime))
        {
            throw new InvalidOperationException($"Trust Chain entry {index} has an invalid '{claimType}' claim.");
        }

        return unixTime;
    }
}
