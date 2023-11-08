using Jose;
using Owf.Sd.Jwt.Abstracts;

namespace Owf.Sd.Jwt.Examples;

public class Issuer : IIssuer
{
    public string Issue(Dictionary<string, object> claims, string issuerJwk)
    {
        // Prepare the header part of a credential JWT.
        // The header represents {"alg":"ES256","typ":"vc+sd-jwt"}.
        var headers = new Dictionary<string, object>()
        {
            { "alg", "ES256" },
            { "typ", "vc+sd-jwt" }
        };

        var privateKey = Jwk.FromJson(issuerJwk, JWT.DefaultSettings.JsonMapper);

        var token = Jose.JWT.Encode(claims, privateKey, JwsAlgorithm.ES256, extraHeaders: headers);

        return token;
    }
}
