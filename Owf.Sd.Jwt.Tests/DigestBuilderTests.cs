namespace Owf.Sd.Jwt.Tests;

public class DigestBuilderTests
{
    [Fact]
    public void DigestBuilder_Disclosure()
    {
        // The following values are from the SD-JWT specification.
        var disclosure = Disclosure.FromBase64Url("WyI2cU1RdlJMNWhhaiIsICJmYW1pbHlfbmFtZSIsICJNw7ZiaXVzIl0");
        var expectedDigest = "uutlBuYeMDyjLLTpf6Jxi7yNkEF35jdyWMn9U7b_RYY";

        // Create a DigestListBuilder instance with the default hash algorithm "sha-256".
        DigestBuilder builder = new();

        // Add the digest of the disclosure. The addDisclosureDigest method
        // returns the digest value of the given disclosure which was computed
        // with the hash algorithm.
        var actualDigest = builder.ComputeAndStoreDisclosureDigest(disclosure!);

        Assert.Equal(expectedDigest, actualDigest);

        // Build a list of digests.
        var digestList = builder.Build();

        Assert.Equal(1, digestList?.Count);
        Assert.Equal(expectedDigest, digestList?.FirstOrDefault());
    }

    [Fact]
    public void DigestBuilder_Multiple_Disclosures()
    {
        // The following values are from the SD-JWT specification.
        var streetAddressDisclosure = Disclosure.FromBase64Url("WyI0d3dqUzlyMm4tblBxdzNpTHR0TkFBIiwgInN0cmVldF9hZGRyZXNzIiwgIlNjaHVsc3RyLiAxMiJd");
        var streetAddressDigest = "pEtkKwoFK_JHN7yNby0Lc_Jc10BAxCm5yXJjDbVehvU";
        var localityDisclosure = Disclosure.FromBase64Url("WyJXcEtIQmVTa3A5U2MyNVV4a1F1RmNRIiwgImxvY2FsaXR5IiwgIlNjaHVscGZvcnRhIl0");
        var localityDigest = "nTzPZ3Q68z1Ko_9ao9LK0mSYXY5gY6UG6KEkQ_BdqU0";
        var regionDisclosure = Disclosure.FromBase64Url("WyIzSl9xWGctdUwxYzdtN1FoT0hUNTJnIiwgInJlZ2lvbiIsICJTYWNoc2VuLUFuaGFsdCJd");
        var regionDigest = "9-VdSnvRTZNDo-4Bxcp3X-V9VtLOCRUkR6oLWZQl81I";
        var countryDisclosure = Disclosure.FromBase64Url("WyIwN2U3bWY2YWpTUDJjZkQ3NmJCZE93IiwgImNvdW50cnkiLCAiREUiXQ");
        var countryDigest = "7pHe1uQ5uSClgAxXdG0E6dKnBgXcxEO1zvoQO9E5Lr4";

        // Create a DigestListBuilder instance with the default hash algorithm "sha-256".
        DigestBuilder builder = new();

        // Add digests of the disclosures.
        builder.ComputeAndStoreDisclosureDigest(streetAddressDisclosure!);
        builder.ComputeAndStoreDisclosureDigest(localityDisclosure!);
        builder.ComputeAndStoreDisclosureDigest(regionDisclosure!);
        builder.ComputeAndStoreDisclosureDigest(countryDisclosure!);

        // Build a list of digests.
        var digestList = builder.Build();

        Assert.Equal(4, digestList?.Count);

        // Note that the elements in the list are sorted.
        Assert.Equal(countryDigest, digestList?[0]);
        Assert.Equal(regionDigest, digestList?[1]);
        Assert.Equal(localityDigest, digestList?[2]);
        Assert.Equal(streetAddressDigest, digestList?[3]);
    }

    [Fact]
    public void DigestBuilder_Disclosure_Decoys()
    {
        // The following values are from the SD-JWT specification.
        var disclosure = Disclosure.FromBase64Url("WyI2cU1RdlJMNWhhaiIsICJmYW1pbHlfbmFtZSIsICJNw7ZiaXVzIl0");

        // Create a DigestListBuilder instance with the default hash algorithm "sha-256".
        DigestBuilder builder = new();

        // Add a disclosure digest and 2 decoy digests.
        builder.ComputeAndStoreDisclosureDigest(disclosure!);
        builder.AddDecoyDigests(2);

        // Build a list of digests.
        var digestList = builder.Build();

        Assert.Equal(3, digestList?.Count);
    }
}
