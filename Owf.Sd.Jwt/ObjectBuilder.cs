namespace Owf.Sd.Jwt;

public class ObjectBuilder
{
    private readonly SupportHashAlgorithm _hashAlgorithm;
    private readonly DigestBuilder _digestBuilder;
    private readonly Dictionary<string, object> _claims;

    public SupportHashAlgorithm HashAlgorithm => _hashAlgorithm;

    public string HashAlgorithmName => HashAlgorithmHelper.GetHashAlgorithmName(_hashAlgorithm);

    public ObjectBuilder(SupportHashAlgorithm hashAlgorithm = SupportHashAlgorithm.SHA256)
    {
        _hashAlgorithm = hashAlgorithm;
        _digestBuilder = new DigestBuilder(_hashAlgorithm);
        _claims = new Dictionary<string, object>();
    }

    public void AddClaim(string claimName, object claimValue)
    {
        if (string.IsNullOrWhiteSpace(claimName))
        {
            throw new ArgumentException("'claimName' is missing");
        }
        if (Utilities.IsReservedKey(claimName))
        {
            throw new ArgumentException($"The claim name {claimName} is a reserved key");
        }

        // If any, remove the digest that corresponds to a _disclosure
        // whose claim name is equal to the one given to this method.
        _digestBuilder.RemoveDigestByClaimName(claimName);


        // Check if the claim name already exists and update it if it does, or add it if it's new.
        if (_claims.TryGetValue(claimName, out var exitsClaimValue))
        {
            _claims[claimName] = claimValue;
        }
        else
        {
            _claims.Add(claimName, claimValue);
        }
       
    }

    public Disclosure AddDisclosure(Disclosure disclosure)
    {
        if (disclosure == null)
        {
            throw new ArgumentException("'discloure' is missing");
        }

        if (string.IsNullOrWhiteSpace(disclosure.ClaimName))
        {
            throw new ArgumentException("The disclosure is not for an object property.");
        }

        // add the digest of the _disclosure
        _digestBuilder.ComputeAndStoreDisclosureDigest(disclosure);

        _claims.Remove(disclosure.ClaimName);

        return disclosure;
    }

    public Disclosure AddDisclosure(string claimName, string claimValue)
    {
        return AddDisclosure(Disclosure.Create(claimName, claimValue));
    }

    public Disclosure AddDisclosure(string salt, string claimName, string claimValue)
    {
        return AddDisclosure(Disclosure.Create(salt, claimName, claimValue));
    }

    public string AddDecoyDigest()
    {
        return _digestBuilder.AddDecoyDigest();
    }

    public HashSet<string> AddDecoyDigests(int count)
    {
        return _digestBuilder.AddDecoyDigests(count);
    }

    public Dictionary<string, object> Build()
    {
        return Build(false);
    }

    public Dictionary<string, object> Build(bool includeHashAlgorithm)
    {
        var output = new Dictionary<string, object>();
        output.AddRange(_claims);

        var digestList = _digestBuilder.Build();

        if (digestList.Count != 0)
        {
            output.Add(Constants.KEY_SD, digestList);
        }

        if (includeHashAlgorithm)
        {
            output.Add(Constants.KEY_SD_ALG, HashAlgorithmHelper.GetHashAlgorithmName(_hashAlgorithm));
        }

        return output;
    }
}
