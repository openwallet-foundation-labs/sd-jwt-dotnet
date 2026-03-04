using SdJwt.Net.Eudiw.Arf;

namespace SdJwt.Net.Eudiw.Credentials;

/// <summary>
/// Handles PID (Person Identification Data) credential processing per EUDIW ARF.
/// </summary>
public class PidCredentialHandler
{
    private readonly ArfProfileValidator _arfValidator = new();

    /// <summary>
    /// Extracts the family name from PID claims.
    /// </summary>
    /// <param name="claims">The PID claims.</param>
    /// <returns>The family name, or null if not present.</returns>
    public string? ExtractFamilyName(IDictionary<string, object> claims)
    {
        return claims.TryGetValue("family_name", out var value) ? value?.ToString() : null;
    }

    /// <summary>
    /// Extracts the given name from PID claims.
    /// </summary>
    /// <param name="claims">The PID claims.</param>
    /// <returns>The given name, or null if not present.</returns>
    public string? ExtractGivenName(IDictionary<string, object> claims)
    {
        return claims.TryGetValue("given_name", out var value) ? value?.ToString() : null;
    }

    /// <summary>
    /// Extracts the birth date from PID claims.
    /// </summary>
    /// <param name="claims">The PID claims.</param>
    /// <returns>The birth date, or null if not present or invalid.</returns>
    public DateTime? ExtractBirthDate(IDictionary<string, object> claims)
    {
        if (claims.TryGetValue("birth_date", out var value) && value != null)
        {
            if (DateTime.TryParse(value.ToString(), out var date))
            {
                return date.Date;
            }
        }
        return null;
    }

    /// <summary>
    /// Extracts the issuing country from PID claims.
    /// </summary>
    /// <param name="claims">The PID claims.</param>
    /// <returns>The issuing country code, or null if not present.</returns>
    public string? ExtractIssuingCountry(IDictionary<string, object> claims)
    {
        return claims.TryGetValue("issuing_country", out var value) ? value?.ToString() : null;
    }

    /// <summary>
    /// Extracts the age_over_18 attribute from PID claims.
    /// </summary>
    /// <param name="claims">The PID claims.</param>
    /// <returns>True/false if disclosed, null if not present.</returns>
    public bool? ExtractAgeOver18(IDictionary<string, object> claims)
    {
        if (claims.TryGetValue("age_over_18", out var value))
        {
            if (value is bool b)
                return b;
            if (bool.TryParse(value?.ToString(), out var parsed))
                return parsed;
        }
        return null;
    }

    /// <summary>
    /// Validates PID claims per ARF requirements.
    /// </summary>
    /// <param name="claims">The PID claims.</param>
    /// <returns>Validation result.</returns>
    public PidValidationResult Validate(IDictionary<string, object>? claims)
    {
        if (claims == null)
        {
            return PidValidationResult.Failure("Claims cannot be null");
        }

        var errors = new List<string>();

        // Check mandatory claims
        foreach (var mandatory in EudiwConstants.Pid.MandatoryClaims)
        {
            if (!claims.ContainsKey(mandatory))
            {
                errors.Add($"Missing mandatory claim: {mandatory}");
            }
        }

        // Check expiry
        if (claims.TryGetValue("expiry_date", out var expiry) && expiry != null)
        {
            if (DateTime.TryParse(expiry.ToString(), out var expiryDate))
            {
                if (expiryDate.Date < DateTime.UtcNow.Date)
                {
                    errors.Add("PID credential has expired");
                }
            }
        }

        // Check issuing country is EU member state
        if (claims.TryGetValue("issuing_country", out var country) && country != null)
        {
            if (!_arfValidator.ValidateMemberState(country.ToString()))
            {
                errors.Add($"Issuing country '{country}' is not a valid EU member state");
            }
        }

        return errors.Count > 0
            ? PidValidationResult.Failure(errors.ToArray())
            : PidValidationResult.Success();
    }

    /// <summary>
    /// Converts PID claims to a typed PidCredential model.
    /// </summary>
    /// <param name="claims">The PID claims.</param>
    /// <returns>A PidCredential instance.</returns>
    /// <exception cref="PidValidationException">Thrown if claims are invalid.</exception>
    public PidCredential ToPidCredential(IDictionary<string, object> claims)
    {
        var validation = Validate(claims);
        if (!validation.IsValid)
        {
            throw new PidValidationException(
                $"Invalid PID claims: {string.Join("; ", validation.Errors)}");
        }

        return new PidCredential
        {
            FamilyName = ExtractFamilyName(claims) ?? string.Empty,
            GivenName = ExtractGivenName(claims) ?? string.Empty,
            BirthDate = ExtractBirthDate(claims) ?? default,
            IssuanceDate = ParseDate(claims, "issuance_date") ?? default,
            ExpiryDate = ParseDate(claims, "expiry_date") ?? default,
            IssuingAuthority = claims.TryGetValue("issuing_authority", out var auth)
                ? auth?.ToString() ?? string.Empty
                : string.Empty,
            IssuingCountry = ExtractIssuingCountry(claims) ?? string.Empty,
            AgeOver18 = ExtractAgeOver18(claims),
            AgeOver21 = ExtractBool(claims, "age_over_21"),
            Nationality = claims.TryGetValue("nationality", out var nat) ? nat?.ToString() : null,
            ResidentAddress = claims.TryGetValue("resident_address", out var addr) ? addr?.ToString() : null
        };
    }

    private static DateTime? ParseDate(IDictionary<string, object> claims, string key)
    {
        if (claims.TryGetValue(key, out var value) && value != null)
        {
            if (DateTime.TryParse(value.ToString(), out var date))
            {
                return date.Date;
            }
        }
        return null;
    }

    private static bool? ExtractBool(IDictionary<string, object> claims, string key)
    {
        if (claims.TryGetValue(key, out var value))
        {
            if (value is bool b)
                return b;
            if (bool.TryParse(value?.ToString(), out var parsed))
                return parsed;
        }
        return null;
    }
}
