using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Owf.Sd.Jwt.Tests;

public class ObjectEncoderTests
{
    [Fact]
    public void ObjectEncoder_Map()
    {
        var sublist = new List<string>
        {
             "element-1", "element-2"
        };

        Dictionary<string, string> submap = new()
        {
            { "sub-key-1", "sub-value-1" },
            { "sub-key-2", "sub-value-2" }
        };

        Dictionary<string, object> originalMap = new()
        {
            {  "key-1", "value-1" },
            {  "key-2", sublist },
            {  "key-3", submap }
        };

        // Encoder
        ObjectEncoder encoder = new();

        // Encode
        Dictionary<string, object> encodedMap = encoder.Encode(originalMap);
        var disclosures = encoder.Disclosures;

        // "_sd_alg" should be contained (cf. setHashAlgorithmIncluded(boolean))
        Assert.True(encodedMap.ContainsKey(Constants.KEY_SD_ALG));

        // "_sd" should be contained.
        Assert.True(encodedMap.ContainsKey(Constants.KEY_SD));

        // "key-1" should not be contained.
        Assert.False(encodedMap.ContainsKey("key-1"));

        // "key-2" should be contained.
        Assert.True(encodedMap.ContainsKey("key-2"));

        // "key-3" should be contained.
        Assert.True(encodedMap.ContainsKey("key-3"));

        // Work variables
        int count;
        Disclosure disclosure;

        // The number of disclosures.
        count = 1 + sublist.Count + submap.Count;
        Assert.Equal(count, disclosures.Count);


        // Disclosure: key-1
        disclosure = Find(disclosures!, d => "key-1".Equals(d.ClaimName));
        Assert.NotNull(disclosure);
        Assert.Equal("value-1", disclosure.ClaimValue);

        // Disclosure: element-1
        disclosure = Find(disclosures!, d => "element-1".Equals(d.ClaimValue));
        Assert.NotNull(disclosure);
        Assert.Null(disclosure.ClaimName);


        // Disclosure: element-2
        disclosure = Find(disclosures!, d => "element-2".Equals(d.ClaimValue));
        Assert.NotNull(disclosure);
        Assert.Null(disclosure.ClaimName);

        // Disclosure: sub-key-1
        disclosure = Find(disclosures!, d => "sub-key-1".Equals(d.ClaimName));
        Assert.NotNull(disclosure);
        Assert.Equal("sub-value-1", disclosure.ClaimValue);

        // Disclosure: sub-key-2
        disclosure = Find(disclosures!, d => "sub-key-2".Equals(d.ClaimName));
        Assert.NotNull(disclosure);
        Assert.Equal("sub-value-2", disclosure.ClaimValue);
    }

    [Fact]
    public void ObjectEncoder_List()
    {
        var sublist = new List<string>
        {
             "sub-element-1", "sub-element-2"
        };

        Dictionary<string, string> submap = new()
        {
            { "sub-key-1", "sub-value-1" },
            { "sub-key-2", "sub-value-2" }
        };

        var originalList = new List<object> {
            "element-1",
             sublist,
             submap
        };


        // Encoder
        ObjectEncoder encoder = new(0.0, 0.0);

        // Encode
        var encodedList = encoder.Encode(originalList);
        var disclosures = encoder.Disclosures;

        // Work variables
        object element;
        string digest;
        Disclosure disclosure;

        //
        // Element at index 0
        //
        //     { "...": "<digest>" }
        //
        element = encodedList[0];

        digest = ExtractDigest(element);
        disclosure = FindByDigest(disclosures!, digest);

        Assert.NotNull(disclosure);
        Assert.Null(disclosure.ClaimName);
        Assert.Equal("element-1", disclosure.ClaimValue);

        //
        // Element at index 1
        //
        //     [ { "...": "<digest>" }, { "...": "<digest>" } ]
        //
        element = encodedList[1];
        Assert.True(CollectionHelpers.IsListType(element));
        var list = CollectionHelpers.ConvertToList(element);
        Assert.Equal(2, list.Count);


        digest = ExtractDigest(CollectionHelpers.ConvertToList(element)[0]);
        disclosure = FindByDigest(disclosures, digest);

        Assert.NotNull(disclosure);
        Assert.Null(disclosure.ClaimName);
        Assert.Equal("sub-element-1", disclosure.ClaimValue);

        digest = ExtractDigest(list[1]);
        disclosure = FindByDigest(disclosures!, digest);

        Assert.NotNull(disclosure);
        Assert.Null(disclosure.ClaimName);
        Assert.Equal("sub-element-2", disclosure.ClaimValue);


        ////
        //// Element at index 2
        ////
        ////     {
        ////       "_sd": [
        ////         "<digest>",
        ////         "<digest>"
        ////       ]
        ////     }
        ////
        element = encodedList[2];

        Assert.True(CollectionHelpers.IsDictionaryType(element));
        var map = CollectionHelpers.ConvertToDictionary(element);

        Assert.True(map!.ContainsKey(Constants.KEY_SD));
        Assert.True(CollectionHelpers.IsListType(map[Constants.KEY_SD]));
        list = CollectionHelpers.ConvertToList(map[Constants.KEY_SD]);
        Assert.Equal(2, list.Count);

        var disclosureA = FindByDigest(disclosures!, list[0].ToString());
        var disclosureB = FindByDigest(disclosures!, list[1].ToString());

        Assert.NotNull(disclosureA);
        Assert.NotNull(disclosureB);

        Disclosure disclosure1;
        Disclosure disclosure2;

        if ("sub-key-1".Equals(disclosureA.ClaimName))
        {
            disclosure1 = disclosureA;
            disclosure2 = disclosureB;
        }
        else
        {
            disclosure1 = disclosureB;
            disclosure2 = disclosureA;
        }

        Assert.Equal("sub-key-1", disclosure1.ClaimName);
        Assert.Equal("sub-value-1", disclosure1.ClaimValue);

        Assert.Equal("sub-key-2", disclosure2.ClaimName);
        Assert.Equal("sub-value-2", disclosure2.ClaimValue);
    }

    [Fact]
    public void ObjectEncoder_Retained_Claims()
    {
        var originalMap = new Dictionary<string, object>
        {
            { "iss", "issuer" },
            { "iat", 123 },
            { "custom-key", "custom-value" }
        };

        // Encoder
        ObjectEncoder encoder = new ObjectEncoder();

        // Encode
        var encodedMap = encoder.Encode(originalMap);

        // "iss" and "iat" should be retained.
        Assert.True(encodedMap.ContainsKey("iss"));
        Assert.True(encodedMap.ContainsKey("iat"));

        // "custom-key" should not be retained.
        Assert.False(encodedMap.ContainsKey("custom-key"));

        // Adjust the set of retained claims.
        encoder.RetainedClaims.Remove("iat");
        encoder.RetainedClaims.Add("custom-key");

        // Encode again with the new settings.
        encodedMap = encoder.Encode(originalMap);

        // "iss" should be retained.
        Assert.True(encodedMap.ContainsKey("iss"));

        // "iat" should not be retained.
        Assert.False(encodedMap.ContainsKey("iat"));

        // "custom-key" should be retained.
        Assert.True(encodedMap.ContainsKey("custom-key"));
    }

    private static Disclosure Find(List<Disclosure> disclosures, Predicate<Disclosure> predicate)
    {
        return disclosures.FirstOrDefault(disclosure => predicate(disclosure));
    }

    private static Disclosure FindByDigest(List<Disclosure> disclosures, string digest)
    {
        return Find(disclosures, d => d.Digest().Equals(digest));
    }

    private static string ExtractDigest(object element)
    {
        // { "...": "<digest>" }

        Debug.Assert(element is Dictionary<string, object>);
        Dictionary<string, object> map = (Dictionary<string, object>)element;

        Debug.Assert(map.ContainsKey(Constants.KEY_THREE_DOTS));
        Debug.Assert(map[Constants.KEY_THREE_DOTS] is string);
        string digest = map[Constants.KEY_THREE_DOTS].ToString();

        Debug.Assert(digest != null);

        return digest;
    }
}
