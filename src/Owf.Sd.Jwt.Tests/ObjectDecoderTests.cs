namespace Owf.Sd.Jwt.Tests;

public class ObjectDecoderTests
{
    [Fact]
    public void ObjectDecoder_Map()
    {
        var sublist = new List<string> {
            "element-1",
            "element-2"
        };

        var submap = new Dictionary<string, string> {
            { "sub-key-1", "sub-value-1" },
            { "sub-key-2", "sub-value-2" }
        };

        var originalMap = new Dictionary<string, object> {
            { "key-1", "value-1" },
            { "key-2", sublist },
            { "key-3", submap }
        };

        // Prepare an encoded map and accompanying disclosures.
        ObjectEncoder encoder = new();
        var encodedMap = encoder.Encode(originalMap);
        var disclosures = encoder.Disclosures;

        // A decoder and work variables.
        List<Disclosure> disclosed;
        Dictionary<string, object> decodedMap;
        Dictionary<string, object> expectedMap;

        // Disclose all. The original map should be restored.
        disclosed = disclosures!;
        decodedMap = ObjectDecoder.Decode(encodedMap, disclosed);
        expectedMap = originalMap;
        Assert.Equal(expectedMap, decodedMap);

        //// Disclose none. Only empty "key-2" and "key-3" should be contained.
        disclosed = new();
        decodedMap = ObjectDecoder.Decode(encodedMap, disclosed);
        expectedMap = new Dictionary<string, object>()
        {
            { "key-2", new List<object>() },
            { "key-3", new Dictionary<string, object>() }
        };
        Assert.Equal(expectedMap, decodedMap);

        //// Disclose "key-1" only.
        disclosed = Filter(disclosures!, d => "key-1".Equals(d.ClaimName));
        decodedMap = ObjectDecoder.Decode(encodedMap, disclosed);
        expectedMap = new Dictionary<string, object>()
        {
            {"key-1", "value-1" },
            { "key-2", new List<object>() },
            { "key-3", new Dictionary<string, object>() }
        };
        Assert.Equal(expectedMap, decodedMap);

        //// Disclose array elements only.
        disclosed = Filter(disclosures!, d => d.ClaimName is null);
        decodedMap = ObjectDecoder.Decode(encodedMap, disclosed);
        expectedMap = new Dictionary<string, object>()
        {
            { "key-2", sublist },
            { "key-3", new Dictionary<string, object>() }
        };
        Assert.Equal(expectedMap, decodedMap);

        //// Disclose key-value pairs in the sub map only.
        disclosed = Filter(disclosures!, d => d.ClaimName is not null && d.ClaimName.StartsWith("sub-key-"));
        decodedMap = ObjectDecoder.Decode(encodedMap, disclosed);
        expectedMap = new Dictionary<string, object>()
        {
            { "key-2", new List<object>() },
            { "key-3", submap }
        };
        Assert.Equal(expectedMap, decodedMap);
    }

    [Fact]
    public void ObjectDecoder_List()
    {
        List<string> sublist = new() { "sub-element-1", "sub-element-2" };

        Dictionary<string, string> submap = new()
        {
            { "sub-key-1", "sub-value-1" },
            { "sub-key-2", "sub-value-2" }
        };

        List<object> originalList = new()
        {
            "element-1",
            sublist,
            submap
        };

        // Prepare an encoded list and accompanying disclosures.
        ObjectEncoder encoder = new();
        var encodedList = encoder.Encode(originalList);
        var disclosures = encoder.Disclosures;

        // A decoder and work variables.
        List<Disclosure> disclosed;
        List<object> decodedList;
        List<object> expectedList;

        // Disclose all. The original list should be restored.
        disclosed = disclosures!;
        decodedList = ObjectDecoder.Decode(encodedList, disclosed);
        expectedList = originalList;
        Assert.Equal(expectedList, decodedList);

        // Disclose none. Only an empty list and an empty map should be restored.
        disclosed = new();
        decodedList = ObjectDecoder.Decode(encodedList, disclosed);
        expectedList = new List<object>() { new List<object>(), new Dictionary<string, object>() };
        Assert.Equal(expectedList, decodedList);

        // Disclose "element-1" only.
        disclosed = Filter(disclosures!, d => d.ClaimValue.Equals("element-1"));
        decodedList = ObjectDecoder.Decode(encodedList, disclosed);
        expectedList = new List<object>() { "element-1", new List<object>(), new Dictionary<string, object>() };
        Assert.Equal(expectedList, decodedList);

        // Disclose elements in the sub array only.
        disclosed = Filter(disclosures!, d => ((string)d.ClaimValue).StartsWith("sub-element-"));
        decodedList = ObjectDecoder.Decode(encodedList, disclosed);
        expectedList = new List<object>() { sublist, new Dictionary<string, object>() };
        Assert.Equal(expectedList, decodedList);

        // Disclose key-value pairs in the sub map only.
        disclosed = Filter(disclosures!, d =>d.ClaimName is not null && d.ClaimName.StartsWith("sub-key-"));
        decodedList = ObjectDecoder.Decode(encodedList, disclosed);
        expectedList = new List<object>() { new List<object>(), submap };
        Assert.Equal(expectedList, decodedList);
    }

    private static List<Disclosure> Filter(List<Disclosure> disclosures, Predicate<Disclosure> predicate)
    {
        return disclosures.FindAll(predicate);
    }
}
