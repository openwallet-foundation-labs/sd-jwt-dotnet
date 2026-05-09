using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.SiopV2;

/// <summary>
/// Validates SIOPv2 subject-signed ID Tokens.
/// </summary>
public class SelfIssuedIdTokenValidator
{
    /// <summary>
    /// Validates a SIOPv2 subject-signed ID Token.
    /// </summary>
    /// <param name="idToken">The compact ID Token.</param>
    /// <param name="parameters">The validation parameters.</param>
    /// <returns>The validation result.</returns>
    public Task<SelfIssuedIdTokenValidationResult> ValidateAsync(
        string idToken,
        SelfIssuedIdTokenValidationParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(idToken));
        }
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }
        if (string.IsNullOrWhiteSpace(parameters.ExpectedAudience))
        {
            throw new ArgumentException("Expected audience is required.", nameof(parameters));
        }
        if (string.IsNullOrWhiteSpace(parameters.ExpectedNonce))
        {
            throw new ArgumentException("Expected nonce is required.", nameof(parameters));
        }

        var handler = new JwtSecurityTokenHandler();
        var unvalidated = handler.ReadJwtToken(idToken);

        // Determine subject syntax type from the sub claim before signature validation.
        var rawSub = unvalidated.Payload.Sub;
        bool isDidSubject = rawSub != null && rawSub.StartsWith("did:", StringComparison.Ordinal);

        SecurityKey signingKey;
        string expectedSubject;

        if (isDidSubject)
        {
            // DID subject syntax (SIOPv2 draft-13 Section 6.2): resolve key via IDidKeyResolver.
            if (parameters.DidKeyResolver == null)
            {
                throw new SecurityTokenException(
                    "Self-Issued ID Token has a DID subject but no IDidKeyResolver was provided in the validation parameters.");
            }

            signingKey = parameters.DidKeyResolver.ResolveKeyAsync(rawSub!, unvalidated.Header.Kid).GetAwaiter().GetResult();
            expectedSubject = rawSub!;
        }
        else
        {
            // JWK thumbprint subject syntax (SIOPv2 draft-13 Section 6.1): derive key from sub_jwk.
            var subJwk = ReadSubJwk(unvalidated.Payload);
            signingKey = subJwk;
            expectedSubject = SiopSubject.CreateJwkThumbprintSubject(subJwk);
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = parameters.ExpectedAudience,
            ValidateLifetime = true,
            ClockSkew = parameters.ClockSkew,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            RequireSignedTokens = true,
            RequireExpirationTime = true
        };

        handler.ValidateToken(idToken, validationParameters, out var validatedToken);
        var payload = ((JwtSecurityToken)validatedToken).Payload;
        var issuer = payload.Iss;
        var subject = payload.Sub;

        if (!FixedTimeEquals(issuer, subject))
        {
            throw new SecurityTokenException("Self-Issued ID Token validation failed: iss must equal sub.");
        }

        if (isDidSubject)
        {
            // For DID subjects iss == sub == the DID is sufficient; no thumbprint to verify.
            if (!FixedTimeEquals(subject, expectedSubject))
            {
                throw new SecurityTokenException("Self-Issued ID Token validation failed: sub does not match resolved DID.");
            }
        }
        else
        {
            if (!IsExpectedJwkThumbprintSubject(subject, expectedSubject))
            {
                throw new SecurityTokenException("Self-Issued ID Token validation failed: sub does not match sub_jwk thumbprint.");
            }
        }

        var nonce = payload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Nonce)?.Value;
        if (!FixedTimeEquals(nonce, parameters.ExpectedNonce))
        {
            throw new SecurityTokenException("Self-Issued ID Token validation failed: nonce does not match.");
        }

        return Task.FromResult(new SelfIssuedIdTokenValidationResult(subject!, payload));
    }

    private static JsonWebKey ReadSubJwk(JwtPayload payload)
    {
        if (!payload.TryGetValue(SiopConstants.Claims.SubJwk, out var rawSubJwk) || rawSubJwk == null)
        {
            throw new SecurityTokenException("Self-Issued ID Token is missing sub_jwk.");
        }

        var json = rawSubJwk switch
        {
            JsonWebKey jwk => SiopSubject.SerializePublicJwk(jwk),
            string value => value,
            JsonElement element => element.GetRawText(),
            _ => JsonSerializer.Serialize(rawSubJwk)
        };

        return new JsonWebKey(json);
    }

    private static bool FixedTimeEquals(string? left, string? right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        var leftBytes = System.Text.Encoding.UTF8.GetBytes(left);
        var rightBytes = System.Text.Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
            CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static bool IsExpectedJwkThumbprintSubject(string? subject, string expectedPrefixedSubject)
    {
        if (FixedTimeEquals(subject, expectedPrefixedSubject))
        {
            return true;
        }

        var bareThumbprint = expectedPrefixedSubject.StartsWith(
            SiopConstants.SubjectSyntaxTypes.JwkThumbprintSha256Prefix,
            StringComparison.Ordinal)
            ? expectedPrefixedSubject.Substring(SiopConstants.SubjectSyntaxTypes.JwkThumbprintSha256Prefix.Length)
            : expectedPrefixedSubject;

        return FixedTimeEquals(subject, bareThumbprint);
    }
}
