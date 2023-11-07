namespace Owf.Sd.Jwt;

public static class ObjectDecoder
{
    public static Dictionary<string, object> Decode(Dictionary<string, object> encodedMap, List<Disclosure> disclosures)
    {
        if (encodedMap == null)
        {
            return new();
        }

        // Determine the hash algorithm.
        var hashAlgorithm = DetermineHashAlgorithm(encodedMap);

        // Create mappings from a disclosure digest to a disclosure.
        Dictionary<string, Disclosure> digestMap = CreateDigestMap(hashAlgorithm, disclosures);

        // Decode the encoded map.
        return DecodeMap(digestMap, encodedMap);
    }

    public static List<object> Decode(List<object> encodedList, List<Disclosure> disclosures, SupportHashAlgorithm hashAlgorithm = SupportHashAlgorithm.SHA256)
    {
        if (encodedList == null)
        {
            return new();
        }

        // Create mappings from a disclosure digest to a disclosure.
        Dictionary<string, Disclosure> digestMap = CreateDigestMap(hashAlgorithm, disclosures);

        // Decode the encoded list.
        return DecodeList(digestMap, encodedList);
    }

    private static SupportHashAlgorithm DetermineHashAlgorithm(Dictionary<string, object> encodedMap)
    {
        // If the map does not contain "_sd_alg".
        if (!encodedMap.ContainsKey(Constants.KEY_SD_ALG))
        {
            // Use the default hash algorithm.
            return SupportHashAlgorithm.SHA256;
        }

        // The value of "_sd_alg".
        object alg = encodedMap[Constants.KEY_SD_ALG];

        // If the value of "_sd_alg" is not a string.
        if (alg is not string)
        {
            throw new ArgumentException("The value of '_sd_alg' is not a string.");
        }

        // The hash algorithm specified by "_sd_alg".
        return HashAlgorithmHelper.GetSupportHashAlgorithm((string)alg);
    }

    private static Dictionary<string, Disclosure> CreateDigestMap(SupportHashAlgorithm hashAlgorithm, List<Disclosure> disclosures)
    {
        // Mappings from a disclosure digest to a disclosure.
        Dictionary<string, Disclosure> map = new();

        if (disclosures == null)
        {
            // Return an empty map.
            return map;
        }

        // For each disclosure.
        foreach (Disclosure disclosure in disclosures)
        {
            // Ignore null disclosures.
            if (disclosure == null)
            {
                continue;
            }

            // Compute the digest of the disclosure with the hash algorithm.
            // The digest is used as a key.
            string key = disclosure.Digest(hashAlgorithm);

            // Add a mapping from the disclosure digest to the disclosure.
            map.Add(key, disclosure);
        }

        // Mappings from a disclosure digest to a disclosure.
        return map;
    }

    private static Dictionary<string, object> DecodeMap(Dictionary<string, Disclosure> digestMap,
        Dictionary<string, object> encodedMap)
    {
        // A map that holds decoded key-value pairs.
        Dictionary<string, object> decodedMap = new();

        // For each key-value pair in the encoded map.
        foreach (var entry in encodedMap)
        {
            string key = entry.Key;
            object value = entry.Value;

            // Decode the key-value pair.
            DecodeMapEntry(digestMap, key, value, decodedMap);
        }

        // A map that holds decoded key-value pairs.
        return decodedMap;
    }

    private static void DecodeMapEntry(Dictionary<string, Disclosure> digestMap, string key, object value, Dictionary<string, object> decodedMap)
    {
        // If the key is "_sd_alg".
        if (Constants.KEY_SD_ALG.Equals(key))
        {
            // "_sd_alg" does not appear in the decoded map.
            return;
        }

        // If the key is "_sd".
        if (Constants.KEY_SD.Equals(key))
        {
            // Process the "_sd" array.
            DecodeSD(digestMap, value, decodedMap);
            return;
        }

        // If the value is a map.
        if (CollectionHelpers.IsDictionaryType(value))
        {
            var dictionary = CollectionHelpers.ConvertToDictionary(value);
            // Decode the nested map.
            value = DecodeMap(digestMap, dictionary!);
        }
        // If the value is a list.
        else if (CollectionHelpers.IsListType(value))
        {
            var list = CollectionHelpers.ConvertToList(value);
            // Decode the list.
            value = DecodeList(digestMap, list);
        }

        decodedMap.Add(key, value);
    }

    private static void DecodeSD(Dictionary<string, Disclosure> digestMap, object sd, Dictionary<string, object> decodedMap)
    {
        // If the value of "_sd" is null.
        if (sd == null)
        {
            // Ignore.
            return;
        }

        // If the value of "_sd" is not a list.
        if (!CollectionHelpers.IsListType(sd))
        {
            throw new ArgumentException("The value of '_sd' is not an array.");
        }

        var sdList = CollectionHelpers.ConvertToList(sd);

        // For each element in the "_sd" array.
        foreach (object element in sdList)
        {
            // If the element is null.
            if (element == null)
            {
                // Ignore.
                continue;
            }

            // If the element is not a string.
            if (element is not string)
            {
                throw new ArgumentException("An element in the '_sd' array is not a string.");
            }

            // The value of the element should be the digest of a disclosure.
            string digest = (string)element;

            // Process the digest.
            DecodeSDElement(digestMap, digest, decodedMap);
        }
    }

    private static void DecodeSDElement(Dictionary<string, Disclosure> digestMap, string digest, Dictionary<string, object> decodedMap)
    {
        // Get a disclosure that corresponds to the digest.
        _ = digestMap.TryGetValue(digest, out Disclosure? disclosure);

        // If the disclosure that corresponds to the digest is not found.
        if (disclosure is null)
        {
            // There are two possibilities.
            //
            // 1. The claim corresponding to the digest is not disclosed.
            // 2. The digest is a decoy digest.
            //
            // In either case, no key-value pair is added to the decoded map.
            return;
        }

        // The key-value pair that the disclosure holds.
        var claimName = disclosure.ClaimName;
        var claimValue = disclosure.ClaimValue;

        // If the claim name is null.
        if (string.IsNullOrWhiteSpace(claimName))
        {
            // That the claim name of a disclosure is null means that the
            // disclosure is for an array element, not for an object property.
            throw new ArgumentException("The digest of a disclosure for an array element is found in the '_sd' array.");
        }

        // Add the disclosed key-value pair.
        decodedMap.Add(claimName, claimValue);
    }

    private static List<object> DecodeList(Dictionary<string, Disclosure> digestMap, List<object> encodedList)
    {
        // A list that holds decoded elements.
        List<object> decodedList = new();

        // For each element in the encoded list.
        foreach (object element in encodedList)
        {
            // Process the element.
            DecodeListElement(digestMap, element, decodedList);
        }

        // A list that holds decoded elements.
        return decodedList;
    }

    private static void DecodeListElement(Dictionary<string, Disclosure> digestMap,
        object element,
        List<object> decodedList)
    {
        if (CollectionHelpers.IsDictionaryType(element))
        {
            var map = CollectionHelpers.ConvertToDictionary(element);

            // If the map contains the key "..." (three dots).
            if (map!.ContainsKey(Constants.KEY_THREE_DOTS))
            {
                // The map represents a selectively-disclosable array element.
                DecodeListElementMap(digestMap, map, decodedList);
                return;
            }
            else
            {
                // Decode the encoded map.
                element = DecodeMap(digestMap, map);
            }
        }
        else if (CollectionHelpers.IsListType(element))
        {
            var list = CollectionHelpers.ConvertToList(element);
            // Decode the encoded list.
            element = DecodeList(digestMap, list);
        }

        // Add the element to the decoded list.
        decodedList.Add(element);
    }

    private static void DecodeListElementMap(Dictionary<string, Disclosure> digestMap,
        Dictionary<string, object> element,
        List<object> decodedList)
    {
        // If the map contains other keys than "...".
        if (element.Count != 1)
        {
            throw new ArgumentException("An object containing the three-dot key ('...') must not contain other keys.");
        }

        // The value of "...".
        object dots = element[Constants.KEY_THREE_DOTS];

        // If the value of "..." is null.
        if (dots == null)
        {
            // Ignore.
            return;
        }

        // If the value of "..." is not a string.
        if (dots is not string)
        {
            throw new ArgumentException("The value of the three-dot key ('...') is not a string.");
        }

        // The value of "..." should be the digest of a disclosure.
        string digest = (string)dots;

        // Process the digest.
        DecodeDots(digestMap, digest, decodedList);
    }

    private static void DecodeDots(Dictionary<string, Disclosure> digestMap, string digest, List<object> decodedList)
    {
        // Get a disclosure that corresponds to the digest.
        _ = digestMap.TryGetValue(digest, out Disclosure? disclosure);

        // If the disclosure that corresponds to the digest is not found.
        if (disclosure is null)
        {
            // There are two possibilities.
            //
            // 1. The array element corresponding to the digest is not disclosed.
            // 2. The digest is a decoy digest.
            //
            // In either case, no element is added to the decoded list.
            return;
        }

        // If the disclosure has a claim name.
        if (!string.IsNullOrWhiteSpace(disclosure.ClaimName))
        {
            // That the claim name of a disclosure is not null means that the
            // disclosure is for an object property, not for an array element.
            throw new ArgumentException("The digest of a disclosure for an object property is specified by '...'");
        }

        // Add the disclosed array element.
        decodedList.Add(disclosure.ClaimValue);
    }

}
