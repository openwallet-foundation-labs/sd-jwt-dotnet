namespace Owf.Sd.Jwt;

public class SDJWT
{
    private const string DELIMITER = "~";

    private readonly string _credentialJwt;
    private readonly List<Disclosure?> _disclosures;
    private readonly string? _bindingJwt;
    private readonly string _serialized;

    public static SDJWT Create(string credentialJwt, IEnumerable<Disclosure?> disclosures)
    {
        return new SDJWT(credentialJwt, disclosures, null);
    }

    public static SDJWT Create(string credentialJwt, IEnumerable<Disclosure?> disclosures, string? bindingJwt)
    {
        return new SDJWT(credentialJwt, disclosures, bindingJwt);
    }

    private SDJWT(string credentialJwt, IEnumerable<Disclosure?> disclosures, string? bindingJwt)
    {
        _credentialJwt = credentialJwt;
        _disclosures = new List<Disclosure?>(disclosures ?? Enumerable.Empty<Disclosure?>());
        _bindingJwt = bindingJwt;
        _serialized = Serialize(credentialJwt, _disclosures, bindingJwt);
    }

    public override string ToString()
    {
        return _serialized;
    }

    public string CredentialJwt => _credentialJwt;

    public List<Disclosure?> Disclosures => _disclosures;

    public string? BindingJwt => _bindingJwt;

    public static SDJWT? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string[] elements = input.Split(new[] { DELIMITER }, StringSplitOptions.None);

        int lastIndex = elements.Length - 1;

        // Make sure that all elements except the last one are not empty.
        for (int i = 0; i < lastIndex; i++)
        {
            if (string.IsNullOrWhiteSpace(elements[i]))
            {
                throw new ArgumentException("The SD-JWT is malformed.");
            }
        }

        if (elements.Length < 2)
        {
            throw new ArgumentException("The SD-JWT is malformed.");
        }

        string credentialJwt = elements[0];
        string? bindingJwt = input.EndsWith(DELIMITER) ? null : elements[lastIndex];

        List<Disclosure?> disclosures;

        try
        {
            disclosures = elements
                .Skip(1)
                .Take(lastIndex - 1)
                .Select(Disclosure.FromBase64Url)
                .ToList();
        }
        catch (Exception cause)
        {
            throw new ArgumentException("Failed to parse disclosures.", cause);
        }

        return new SDJWT(credentialJwt, disclosures, bindingJwt);
    }

    private static string Serialize(string credentialJwt, List<Disclosure?> disclosures, string? bindingJwt)
    {
        var elements = new List<string> { credentialJwt };

        elements.AddRange(disclosures.Select(d => d!.ToString()));

        if (bindingJwt != null)
        {
            elements.Add(bindingJwt);
        }
        else
        {
            // if the binding is null, add empty space to create the ~ at the end of string
            elements.Add("");
        }

        return string.Join(DELIMITER, elements);
    }
}