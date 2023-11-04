using System.Reflection.Metadata;

namespace Owf.Sd.Jwt.Tests;

public class ObjectBuilderTests
{
    [Fact]
    public void ObjectBuilder_SD_NonSD()
    {
        // The following values are from the SD-JWT specification.
        var disclosure = Disclosure.FromBase64Url("WyI2cU1RdlJMNWhhaiIsICJmYW1pbHlfbmFtZSIsICJNw7ZiaXVzIl0");
        var expectedDigest = "uutlBuYeMDyjLLTpf6Jxi7yNkEF35jdyWMn9U7b_RYY";

        // Create an ObjectBuilder instance with the default hash algorithm "sha-256".
        var builder = new ObjectBuilder();

        // Add the digest of the disclosure.
        builder.AddDisclosure(disclosure!);

        // Add an arbitrary claim.
        var claimName = "my_claim_name";
        var claimValue = "my_claim_value";
        builder.AddClaim(claimName, claimValue);

        // Build a map that represents a JSON object.
        var map = builder.Build();

        // 1: "my_claim_name": "my_claim_value"
        // 2: "_sd": [ "uutlBuYeMDyjLLTpf6Jxi7yNkEF35jdyWMn9U7b_RYY" ]
        Assert.Equal(2, map.Count);

        // "my_claim_name": "my_claim_value"

        Assert.True(map.ContainsKey(claimName));
        Assert.Equal(claimValue, map[claimName]);

        // "_sd": [ "uutlBuYeMDyjLLTpf6Jxi7yNkEF35jdyWMn9U7b_RYY" ]
        Assert.True(map.ContainsKey(Constants.KEY_SD));
        var sd = map[Constants.KEY_SD];

        Assert.True(sd is List<string>);
        var digestList = sd as List<string>;
        Assert.Single(digestList!);
        Assert.Equal(expectedDigest, digestList![0]);
    }

    [Fact]
    public void ObjectBuilder_SD_SdAlg()
    {
        // Create an SDObjectBuilder instance with the default hash algorithm "sha-256".
        var builder = new ObjectBuilder();

        // Add a selectively disclosable claim.
        var claimName = "my_claim_name";
        var claimValue = "my_claim_value";
        var disclosure = builder.AddDisclosure(claimName, claimValue);

        // Build a map that represents a JSON object which contains "_sd_alg"
        // in addition to "_sd".
        var map = builder.Build(/*hashAlgorithmIncluded*/true);

        // 1: "_sd"
        // 2: "_sd_alg"
        Assert.Equal(2, map.Count);

        // "_sd"
        Assert.True(map.ContainsKey(Constants.KEY_SD));

        var sd = map[Constants.KEY_SD];
        Assert.True(sd is List<string>);

        var digestList = sd as List<string>;

        // Digest of the disclosure with the default hash algorithm "sha-256".
        var expectedDigest = disclosure.Digest();

        Assert.Single(digestList!);
        Assert.Equal(expectedDigest, digestList![0]);

        // "_sd_alg"
        Assert.True(map.ContainsKey(Constants.KEY_SD_ALG));
        var sdAlg = map[Constants.KEY_SD_ALG];

        Assert.True(sdAlg is string);
        var algorithm = sdAlg.ToString();

        Assert.Equal(builder.HashAlgorithmName, algorithm);
    }

    [Fact]
    public void ObjectBuilder_Duplicate_Claim_Names()
    {
        var claimName = "my_claim_name";
        var claimValueA = "A";
        var claimValueB = "B";

        // Create two disclosures with the same claim name.
        Disclosure disclosureA = Disclosure.Create(claimName, claimValueA);
        Disclosure disclosureB = Disclosure.Create(claimName, claimValueB);

        // Create an SDObjectBuilder instance with the default hash algorithm "sha-256".
        ObjectBuilder builder = new();

        // Try to put digests of the two disclosures.
        // (But the first digest will be overwritten by the second one.)
        builder.AddDisclosure(disclosureA);
        builder.AddDisclosure(disclosureB);

        // Build a map that contains "_sd".
        var map = builder.Build();

        // "_sd"
        var digestList = map[Constants.KEY_SD] as List<string>;

        // The number of elements in the "_sd" array should be 1 because
        // the digest value of disclosureA should be overwritten by the
        // digest value of disclosureB.
        Assert.Single(digestList!);
        Assert.Equal(disclosureB.Digest(), digestList![0]);
    }

    [Fact]
    public void ObjectBuilder_Duplicate_Claim_Names_02()
    {
        var claimName = "my_claim_name";
        var claimValueA = "A";
        var claimValueB = "B";

        // Create an SDObjectBuilder instance with the default hash algorithm "sha-256".
        ObjectBuilder builder = new();

        builder.AddClaim(claimName, claimValueA);
        builder.AddClaim(claimName, claimValueB);

        // Build a map that contains "_sd".
        var map = builder.Build();

        // The map should not contain the "_sd" array because the array is empty.
        Assert.False(map.ContainsKey(Constants.KEY_SD));

        // The map should contain a normal claim whose key is equal to claimName.
        Assert.True(map.ContainsKey(claimName));
        Assert.Equal(claimValueB, map[claimName]);
    }

    [Fact]
    public void ObjectBuilder_Duplicate_Claim_Names_03()
    {
        var claimName = "my_claim_name";
        var claimValueA = "A";
        var claimValueB = "B";

        // Create an SDObjectBuilder instance with the default hash algorithm "sha-256".
        ObjectBuilder builder = new();

        builder.AddClaim(claimName, claimValueA);
        builder.AddDisclosure(claimName, claimValueB);

        // Build a map that contains "_sd".
        var map = builder.Build();

        // The map should contain the "_sd"
        Assert.True(map.ContainsKey(Constants.KEY_SD));
        Assert.Single(map);
    }

    [Fact]
    public void ObjectBuilder_Use_SHA_512()
    {
        // Create a Disclosure.
        var salt = "_26bc4LT-ac6q2KI6cBW5es";
        var claimName = "family_name";
        var claimValue = "Möbius";
        Disclosure disclosure = Disclosure.Create(salt, claimName, claimValue);

        // The hash algorithm to use.
        var hashAlgorithm = SupportHashAlgorithm.SHA512;

        // Create an ObjectBuilder with the hash algorithm to use.
        ObjectBuilder builder = new ObjectBuilder(hashAlgorithm);

        // Put a digest of the Disclosure.
        builder.AddDisclosure(disclosure);

        // Create a Map instance with the "_sd_alg" claim included.
        var map = builder.Build(true);

        // "_sd_alg"
        Assert.True(map.ContainsKey(Constants.KEY_SD_ALG));
        Assert.Equal(HashAlgorithmHelper.GetHashAlgorithmName(hashAlgorithm), map[Constants.KEY_SD_ALG]);
        

        // The digest value.
        Assert.True(map.ContainsKey(Constants.KEY_SD));
        var digestList = map[Constants.KEY_SD] as List<string>;
        var digest = digestList![0];

        // The length of the base64url-encoded digest value computed with
        // "sha-512" should be 86.
        Assert.Equal(86, digest.Length);
    }
}
