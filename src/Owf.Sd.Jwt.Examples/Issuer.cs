using Jose;
using Owf.Sd.Jwt.Abstracts;

namespace Owf.Sd.Jwt.Examples;

public class Issuer : IIssuer
{
    public string Issue(Dictionary<string, object> claims, string issuerJwk)
    {
        // Create object builder
        ObjectBuilder builder = new();

        // Create a disclosure for each claims
        foreach (var claim in claims)
        {
            builder.AddDisclosure(Disclosure.Create(claim.Key, claim.Value));
        }

        var payload = builder.Build();

        // Prepare the header part of a credential JWT.
        // The header represents {"alg":"ES256","typ":"vc+sd-jwt"}.
        var headers = new Dictionary<string, object>()
        {
            { "alg", "ES256" },
            { "typ", "vc+sd-jwt" }
        };

        var privateKey = Jwk.FromJson(issuerJwk, JWT.DefaultSettings.JsonMapper);

        var token = Jose.JWT.Encode(payload, privateKey, JwsAlgorithm.ES256, extraHeaders: headers);

        return token;
    }
}
