namespace Owf.Sd.Jwt;

/// <summary>
/// Helper class for building and managing disclosure and decoy digests.
/// </summary>
public sealed class DigestBuilder
{
    private readonly Dictionary<string, string> _claimNameToDigestMap;
    private readonly HashSet<string> _decoyDigestSet;
    private readonly SupportHashAlgorithm _hashAlgorithm;

    /// <summary>
    /// Initializes a new instance of the <see cref="DigestBuilder"/> class.
    /// </summary>
    /// <param name="algorithm">The hashing algorithm to use. If not specified, SHA256 is used by default.</param>
    public DigestBuilder(SupportHashAlgorithm algorithm = SupportHashAlgorithm.SHA256)
    {
        _hashAlgorithm = algorithm;
        _claimNameToDigestMap = new Dictionary<string, string>();
        _decoyDigestSet = new HashSet<string>();
    }

    /// <summary>
    /// Checks if a digest exists for a claim name.
    /// </summary>
    /// <param name="claimName">The claim name to check.</param>
    /// <returns>True if a digest exists for the specified claim name; otherwise, false.</returns>
    public bool DigestExists(string claimName) => _claimNameToDigestMap.ContainsKey(claimName);

    /// <summary>
    /// Clears all digests from the builder.
    /// </summary>
    public void Clear()
    {
        _claimNameToDigestMap.Clear();
        _decoyDigestSet.Clear();
    }

    /// <summary>
    /// Gets all digests added to the builder.
    /// </summary>
    /// <returns>An enumerable collection of digests.</returns>
    public IEnumerable<string> GetDigests() => _claimNameToDigestMap.Values.Concat(_decoyDigestSet);

    /// <summary>
    /// Adds a disclosure digest to the builder.
    /// </summary>
    /// <param name="disclosure">The disclosure object to add a digest for.</param>
    /// <returns>The added digest.</returns>
    public string ComputeAndStoreDisclosureDigest(Disclosure disclosure)
    {
        if (string.IsNullOrEmpty(disclosure.ClaimName))
        {
            throw new ArgumentException("Claim name should not be null or empty.");
        }

        var claimName = disclosure.ClaimName;
        var digest = disclosure.Digest(_hashAlgorithm);

        // Check if the claim name already exists and update it if it does, or add it if it's new.
        if (_claimNameToDigestMap.TryGetValue(claimName, out var existingDigest))
        {
            _claimNameToDigestMap[claimName] = digest;
        }
        else
        {
            _claimNameToDigestMap.Add(claimName, digest);
        }

        return digest;
    }

    /// <summary>
    /// Adds a decoy digest to the builder.
    /// </summary>
    /// <returns>The added decoy digest.</returns>
    public string AddDecoyDigest()
    {
        // Generate a random digest value.
        var digest = Utilities.GenerateRandomDigest(HashAlgorithmHelper.GetHashAlgorithm(_hashAlgorithm));

        _decoyDigestSet.Add(digest);

        return digest;
    }

    /// <summary>
    /// Adds multiple decoy digests to the builder.
    /// </summary>
    /// <param name="count">The number of decoy digests to add.</param>
    /// <returns>A list of added decoy digests.</returns>
    public HashSet<string> AddDecoyDigests(int count)
    {
        // A list of decoy digest values.
        HashSet<string> digestSet = new();

        for (int i = 0; i < count; i++)
        {
            // Add one decoy digest value.
            var digest = AddDecoyDigest();

            digestSet.Add(digest);
        }

        return digestSet;
    }

    /// <summary>
    /// Builds the list of digests with an order that hides the original order of claims.
    /// </summary>
    /// <returns>An enumerable collection of digests with a hidden order.</returns>
    public List<string> Build()
    {
        var digests = _claimNameToDigestMap.Values.Concat(_decoyDigestSet);

        // From the SD-JWT specification:
        // The Issuer MUST hide the original order of the claims in the array.
        // To ensure this, it is RECOMMENDED to shuffle the array of hashes,
        // e.g., by sorting it alphanumerically or randomly, after potentially
        // adding decoy digests as described in Section 5.6. The precise
        // method does not matter as long as it does not depend on the
        // original order of elements.
        return digests.OrderBy(x => x).ToList();
    }

    /// <summary>
    /// Removes a digest by claim name.
    /// </summary>
    /// <param name="claimName">The claim name for which to remove the digest.</param>
    /// <returns>True if the digest was successfully removed; otherwise, false.</returns>
    public bool RemoveDigestByClaimName(string claimName) => _claimNameToDigestMap.Remove(claimName);
}
