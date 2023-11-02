using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Owf.Sd.Jwt;

public class ObjectBuilder
{
    private readonly SupportedHashAlgorithm _hashAlgorithm;
    private readonly DigestBuilder _digestBuilder;
    private readonly Dictionary<string, object> _claims;

    public SupportedHashAlgorithm HashAlgorithm => _hashAlgorithm;

    public ObjectBuilder(SupportedHashAlgorithm hashAlgorithm = SupportedHashAlgorithm.SHA256)
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

        // If any, remove the digest that corresponds to a disclosure
        // whose claim name is equal to the one given to this method.
        _digestBuilder.RemoveDigestByClaimName(claimName);

        // Add claim and its value
        _claims.Add(claimName, claimValue);
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

        // add the digest of the disclosure
        _digestBuilder.AddDisclosureDigest(disclosure);

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
            output.Add(Constants.KEY_SD_ALG, HashAlgorithmExtension.GetHashAlgorithmName(_hashAlgorithm));
        }

        return output;
    }
}
