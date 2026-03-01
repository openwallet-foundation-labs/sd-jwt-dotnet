namespace SdJwt.Net.Eudiw.TrustFramework;

/// <summary>
/// Resolves and validates issuers via EU Trust Lists.
/// </summary>
public class EuTrustListResolver
{
    private static readonly HashSet<string> EuMemberStates = new(
        EudiwConstants.MemberStates.All,
        StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, ListOfTrustedLists> _cache = new();
    private DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;

    /// <summary>
    /// Gets the LOTL URL for XML format.
    /// </summary>
    public string LotlUrl => EudiwConstants.TrustList.LotlUrl;

    /// <summary>
    /// Gets the LOTL URL for JSON format.
    /// </summary>
    public string LotlJsonUrl => EudiwConstants.TrustList.LotlJsonUrl;

    /// <summary>
    /// Gets or sets the cache timeout duration.
    /// </summary>
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Gets whether the cache is currently empty.
    /// </summary>
    public bool IsCacheEmpty => _cache.Count == 0 || DateTimeOffset.UtcNow > _cacheExpiry;

    /// <summary>
    /// Validates if a country code is a valid EU member state.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code.</param>
    /// <returns>True if valid EU member state.</returns>
    public bool IsValidMemberState(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return false;
        }

        return EuMemberStates.Contains(countryCode);
    }

    /// <summary>
    /// Gets all supported EU member state codes.
    /// </summary>
    /// <returns>Collection of ISO 3166-1 alpha-2 country codes.</returns>
    public IReadOnlyCollection<string> GetSupportedMemberStates()
    {
        return EuMemberStates;
    }

    /// <summary>
    /// Clears the internal cache.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        _cacheExpiry = DateTimeOffset.MinValue;
    }
}
