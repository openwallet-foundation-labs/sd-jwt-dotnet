using System.Collections.Immutable;

namespace Owf.Sd.Jwt;

public class ObjectEncoder
{
    private static readonly double DECOY_MAGNIFICATION_MIN_LIMIT = 0.0;
    private static readonly double DECOY_MAGNIFICATION_MAX_LIMIT = 10.0;
    private static readonly double DECOY_MAGNIFICATION_MIN_DEFAULT = 0.5;
    private static readonly double DECOY_MAGNIFICATION_MAX_DEFAULT = 1.5;

    private readonly Random _random = new();
    private double _decoyMagnificationMin;
    private double _decoyMagnificationMax;
    private readonly ImmutableHashSet<string> _retainedClaims;
    private List<Disclosure?> _disclosures;


    public SupportHashAlgorithm HashAlgorithm { get; set; }

    public bool HashAlgorithmIncluded { get; set; }

    public ImmutableHashSet<string> RetainedClaims => _retainedClaims;

    public List<Disclosure?> Disclosures => _disclosures;

    public ObjectEncoder(SupportHashAlgorithm hashAlgorithm = SupportHashAlgorithm.SHA256) :
        this(DECOY_MAGNIFICATION_MIN_DEFAULT, DECOY_MAGNIFICATION_MAX_DEFAULT, hashAlgorithm)
    {
    }

    public ObjectEncoder(double decoyMagnificationMin, double decoyMagnificationMax) :
        this(decoyMagnificationMin, decoyMagnificationMax, SupportHashAlgorithm.SHA256)
    {
    }

    public ObjectEncoder(double decoyMagnificationMin, double decoyMagnificationMax, SupportHashAlgorithm hashAlgorithm)
    {
        if (decoyMagnificationMin > decoyMagnificationMax)
        {
            throw new ArgumentException("decoyMagnificationMin > decoyMagnificationMax");
        }

        HashAlgorithm = hashAlgorithm;
        _decoyMagnificationMin = NormalizeDecoyMagnification(decoyMagnificationMin);
        _decoyMagnificationMax = NormalizeDecoyMagnification(decoyMagnificationMax);
        HashAlgorithmIncluded = true;
        _retainedClaims = Constants.RETAINED_CLAIMS;
        _disclosures = new List<Disclosure?>();
    }

    private static double NormalizeDecoyMagnification(double magnification)
    {
        return Math.Max(DECOY_MAGNIFICATION_MIN_LIMIT, Math.Min(magnification, DECOY_MAGNIFICATION_MAX_LIMIT));
    }

    private Dictionary<string, object> EncodeMap(Dictionary<string, object> input)
    {
        return EncodeMap(input, false);
    }

    private Dictionary<string, object> EncodeMap(Dictionary<string, object> input, bool top)
    {
        ObjectBuilder builder = new(HashAlgorithm);

        // For each key-value pair in the input map.
        foreach (var entry in input)
        {
            string key = entry.Key;
            object value = entry.Value;

            // If the input map is the top-level map and the key is a
            // claim to retain.
            if (top && _retainedClaims.Contains(key))
            {
                // Add the claim without making it selectively-disclosable.
                builder.AddClaim(key, value);
            }
            else if (CollectionHelpers.IsDictionaryType(value))
            {
                // Encode the sub map.
                value = EncodeMap(CollectionHelpers.ConvertToDictionary(value));
                builder.AddClaim(key, value);
            }
            else if (CollectionHelpers.IsListType(value))
            {
                // Encode the list.
                var list = CollectionHelpers.ConvertToList(value);
                value = EncodeList(list);
                builder.AddClaim(key, value);
            }
            else
            {
                // Key-value pairs of other types are made selectively-
                // disclosable here and the digests of their disclosures
                // are added to the "_sd" array in the JSON object here.
                Disclosure disclosure = builder.AddDisclosure(Disclosure.Create(key, value));
                _disclosures.Add(disclosure);
            }
        }

        // Compute the number of decoys to insert.
        int decoyCount = ComputeDecoyCount(input.Count);

        // Insert decoys.
        builder.AddDecoyDigests(decoyCount);

        // Build an encoded map that may contain the "_sd" array.
        return builder.Build(top && HashAlgorithmIncluded);
    }

    private List<object> EncodeList(List<object> input)
    {
        // The size of the input list.
        int inputSize = input.Count;

        // Compute the number of decoys based on the size of the input list.
        int decoyCount = ComputeDecoyCount(inputSize);

        // The encoded list.
        List<object> encodedList = new(inputSize + decoyCount);

        // For each element in the input list.
        foreach (object value in input)
        {
            if (CollectionHelpers.IsDictionaryType(value))
            {
                var dictionary = CollectionHelpers.ConvertToDictionary(value);
                // Encode the sub map.
                encodedList.Add(EncodeMap(dictionary));
            }
            else if (CollectionHelpers.IsListType(value))
            {
                var list = CollectionHelpers.ConvertToList(value);
                // Encode the sub list.
                encodedList.Add(EncodeList(list));
            }
            else
            {
                // Elements of other types are made selectively-disclosable here.
                Disclosure disclosure = Disclosure.Create(value);
                _disclosures.Add(disclosure);

                // value = { "...": "<digest>" }
                encodedList.Add(disclosure.ToArrayElement(HashAlgorithm));
            }
        }

        // Repeat as many times as the number of decoys.
        for (int i = 0; i < decoyCount; i++)
        {
            // Compute the index at which a decoy is inserted.
            int bound = encodedList.Count + 1;
            int index = _random.Next(bound);

            // Insert a decoy element at the randomly-selected position.
            encodedList.Insert(index, Utilities.GenerateDecoyArrayElement(HashAlgorithmHelper.GetHashAlgorithm(HashAlgorithm)));
        }

        // The encoded list.
        return encodedList;
    }

    public Dictionary<string, object> Encode(Dictionary<string, object> input)
    {
        Reset();

        if (input == null)
        {
            return new Dictionary<string, object>();
        }

        // Encode the given map.
        return EncodeMap(input, true);
    }

    public List<object> Encode(List<object> input)
    {
        Reset();

        if (input == null)
        {
            return new List<object>();
        }

        // Encode the given list.
        return EncodeList(input);
    }

    private void Reset()
    {
        // Reset the list of disclosures.
        _disclosures = new List<Disclosure?>();
    }


    private int ComputeDecoyCount(int baseCount)
    {
        double min = _decoyMagnificationMin;
        double max = _decoyMagnificationMax;
        double d;

        if (min == max)
        {
            d = min;
        }
        else
        {
            // A random double value between the min and the max.
            d = _random.NextDouble() * (max - min) + min;
        }

        return (int)Math.Round(baseCount * d);
    }

    public double DecoyMagnificationMin
    {
        get => _decoyMagnificationMin;
        set
        {
            if (value > _decoyMagnificationMax)
            {
                throw new ArgumentException("decoyMagnificationMin > decoyMagnificationMax");
            }

            _decoyMagnificationMin = NormalizeDecoyMagnification(value);
        }
    }

    public double DecoyMagnificationMax
    {
        get => _decoyMagnificationMax;
        set
        {
            if (value < _decoyMagnificationMin)
            {
                throw new ArgumentException("decoyMagnificationMax < decoyMagnificationMin");
            }

            _decoyMagnificationMax = NormalizeDecoyMagnification(value);
        }
    }
}
