using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents constraints applied to credential selection as defined in DIF Presentation Exchange 2.1.1.
/// Defines requirements for field values, limitDisclosure behavior, and subject requirements.
/// </summary>
public class Constraints
{
    /// <summary>
    /// Gets or sets the fields constraints that specify requirements for specific credential fields.
    /// Optional. Each field constraint defines a JSON path and validation criteria.
    /// </summary>
    [JsonPropertyName("fields")]
    public Field[]? Fields
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether the credential must support selective disclosure.
    /// Optional. Applies to formats that support selective disclosure (e.g., SD-JWT).
    /// </summary>
    [JsonPropertyName("limit_disclosure")]
    public string? LimitDisclosure
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the subject requirements for the credential.
    /// Optional. Defines constraints on the credential subject.
    /// </summary>
    [JsonPropertyName("subject_is_issuer")]
    public string? SubjectIsIssuer
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets holder-binding directives for fields.
    /// </summary>
    [JsonPropertyName("is_holder")]
    public object[]? IsHolder
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets same-subject directives for fields.
    /// </summary>
    [JsonPropertyName("same_subject")]
    public object[]? SameSubject
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets credential status constraints.
    /// </summary>
    [JsonPropertyName("statuses")]
    public StatusConstraints? Statuses
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets additional constraint properties not defined in the base specification.
    /// Optional. Allows for extension properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData
    {
        get; set;
    }

    /// <summary>
    /// Validates the constraints according to DIF PEX 2.1.1 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
    public void Validate()
    {
        // Validate field constraints if present
        if (Fields != null)
        {
            foreach (var field in Fields)
            {
                field?.Validate();
            }

            // Check for duplicate field paths
            var paths = Fields.Where(f => f?.Path != null)
                             .SelectMany(f => f.Path!)
                             .GroupBy(p => p)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key);

            if (paths.Any())
            {
                var duplicatePaths = string.Join(", ", paths);
                throw new InvalidOperationException($"Duplicate field paths found: {duplicatePaths}");
            }
        }

        // Validate limit disclosure value
        if (!string.IsNullOrEmpty(LimitDisclosure))
        {
            var validValues = new[] { "required", "preferred" };
            if (!validValues.Contains(LimitDisclosure))
            {
                throw new InvalidOperationException($"LimitDisclosure must be one of: {string.Join(", ", validValues)}");
            }
        }

        // Validate subject is issuer value
        if (!string.IsNullOrEmpty(SubjectIsIssuer))
        {
            var validValues = new[] { "required", "preferred" };
            if (!validValues.Contains(SubjectIsIssuer))
            {
                throw new InvalidOperationException($"SubjectIsIssuer must be one of: {string.Join(", ", validValues)}");
            }
        }

        ValidateHolderDirectives(IsHolder, nameof(IsHolder));
        ValidateHolderDirectives(SameSubject, nameof(SameSubject));
        Statuses?.Validate();
    }

    /// <summary>
    /// Gets a value indicating whether the constraints object contains at least one PEX-defined property or extension.
    /// </summary>
    /// <returns><see langword="true"/> when any constraint property is present.</returns>
    public bool HasAnyConstraintProperty()
    {
        return (Fields != null && Fields.Length > 0) ||
               !string.IsNullOrWhiteSpace(LimitDisclosure) ||
               !string.IsNullOrWhiteSpace(SubjectIsIssuer) ||
               (IsHolder != null && IsHolder.Length > 0) ||
               (SameSubject != null && SameSubject.Length > 0) ||
               Statuses != null ||
               (ExtensionData != null && ExtensionData.Count > 0);
    }

    private static void ValidateHolderDirectives(object[]? directives, string propertyName)
    {
        if (directives == null)
        {
            return;
        }

        foreach (var directive in directives)
        {
            if (directive == null)
            {
                throw new InvalidOperationException($"{propertyName} entries cannot be null");
            }

            if (directive is HolderDirective holderDirective)
            {
                holderDirective.Validate();
                continue;
            }

            if (directive is string)
            {
                throw new InvalidOperationException($"{propertyName} entries must be objects with field_id and directive");
            }

            var json = System.Text.Json.JsonSerializer.Serialize(directive);
            var parsed = System.Text.Json.JsonSerializer.Deserialize<HolderDirective>(json);
            if (parsed == null)
            {
                throw new InvalidOperationException($"{propertyName} entries must be objects with field_id and directive");
            }

            parsed.Validate();
        }
    }

    /// <summary>
    /// Creates basic constraints with field requirements.
    /// </summary>
    /// <param name="fields">The field constraints to apply</param>
    /// <returns>A new Constraints instance</returns>
    public static Constraints Create(params Field[] fields)
    {
        return new Constraints
        {
            Fields = fields
        };
    }

    /// <summary>
    /// Creates constraints that require selective disclosure.
    /// </summary>
    /// <param name="fields">The field constraints to apply</param>
    /// <returns>A new Constraints instance with selective disclosure required</returns>
    public static Constraints CreateWithSelectiveDisclosure(params Field[] fields)
    {
        return new Constraints
        {
            Fields = fields,
            LimitDisclosure = "required"
        };
    }

    /// <summary>
    /// Creates constraints for a specific issuer.
    /// </summary>
    /// <param name="issuerUrl">The required issuer URL</param>
    /// <returns>A new Constraints instance that requires the specified issuer</returns>
    public static Constraints CreateForIssuer(string issuerUrl)
    {
        if (string.IsNullOrWhiteSpace(issuerUrl))
            throw new ArgumentException("Issuer URL cannot be null or empty", nameof(issuerUrl));

        return new Constraints
        {
            Fields = new[]
            {
                new Field
                {
                    Path = new[] { PresentationExchangeConstants.CommonJsonPaths.Issuer },
                    Filter = new FieldFilter
                    {
                        Type = "string",
                        Const = issuerUrl
                    }
                }
            }
        };
    }

    /// <summary>
    /// Creates constraints for multiple allowed issuers.
    /// </summary>
    /// <param name="issuerUrls">The allowed issuer URLs</param>
    /// <returns>A new Constraints instance that allows any of the specified issuers</returns>
    public static Constraints CreateForIssuers(params string[] issuerUrls)
    {
        if (issuerUrls == null || issuerUrls.Length == 0)
            throw new ArgumentException("At least one issuer URL is required", nameof(issuerUrls));

        return new Constraints
        {
            Fields = new[]
            {
                new Field
                {
                    Path = new[] { PresentationExchangeConstants.CommonJsonPaths.Issuer },
                    Filter = new FieldFilter
                    {
                        Type = "string",
                        Enum = issuerUrls
                    }
                }
            }
        };
    }

    /// <summary>
    /// Creates constraints for a specific credential type.
    /// </summary>
    /// <param name="credentialType">The required credential type</param>
    /// <param name="isVc">Whether this is a verifiable credential (uses vc.type path)</param>
    /// <returns>A new Constraints instance that requires the specified type</returns>
    public static Constraints CreateForType(string credentialType, bool isVc = false)
    {
        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("Credential type cannot be null or empty", nameof(credentialType));

        var path = isVc
            ? PresentationExchangeConstants.CommonJsonPaths.VcType
            : PresentationExchangeConstants.CommonJsonPaths.VctType;

        return new Constraints
        {
            Fields = new[]
            {
                new Field
                {
                    Path = new[] { path },
                    Filter = new FieldFilter
                    {
                        Type = isVc ? "array" : "string",
                        Contains = isVc ? new { Const = credentialType } : null,
                        Const = !isVc ? credentialType : null
                    }
                }
            }
        };
    }

    /// <summary>
    /// Adds a field constraint to these constraints.
    /// </summary>
    /// <param name="field">The field constraint to add</param>
    public void AddField(Field field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var fields = Fields?.ToList() ?? new List<Field>();
        fields.Add(field);
        Fields = fields.ToArray();
    }

    /// <summary>
    /// Gets all JSON paths referenced by field constraints.
    /// </summary>
    /// <returns>Array of JSON paths</returns>
    public string[] GetReferencedPaths()
    {
        if (Fields == null)
            return Array.Empty<string>();

        return Fields.Where(f => f?.Path != null)
                    .SelectMany(f => f.Path!)
                    .Distinct()
                    .ToArray();
    }

    /// <summary>
    /// Checks if these constraints require selective disclosure.
    /// </summary>
    /// <returns>True if selective disclosure is required</returns>
    public bool RequiresSelectiveDisclosure()
    {
        return LimitDisclosure == "required";
    }

    /// <summary>
    /// Checks if these constraints prefer selective disclosure.
    /// </summary>
    /// <returns>True if selective disclosure is preferred</returns>
    public bool PrefersSelectiveDisclosure()
    {
        return LimitDisclosure == "preferred";
    }
}

/// <summary>
/// Represents an is_holder or same_subject relational constraint directive.
/// </summary>
public class HolderDirective
{
    /// <summary>
    /// Gets or sets the field identifiers to which the directive applies.
    /// </summary>
    [JsonPropertyName("field_id")]
    public string[] FieldId { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the directive value. Valid values are required or preferred.
    /// </summary>
    [JsonPropertyName("directive")]
    public string Directive { get; set; } = string.Empty;

    /// <summary>
    /// Validates the directive.
    /// </summary>
    public void Validate()
    {
        if (FieldId == null || FieldId.Length == 0 || FieldId.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("field_id must contain at least one non-empty value");

        if (Directive != "required" && Directive != "preferred")
            throw new InvalidOperationException("directive must be 'required' or 'preferred'");
    }
}

/// <summary>
/// Represents PEX credential status constraints.
/// </summary>
public class StatusConstraints
{
    /// <summary>
    /// Gets or sets the active status directive.
    /// </summary>
    [JsonPropertyName("active")]
    public StatusDirective? Active
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the suspended status directive.
    /// </summary>
    [JsonPropertyName("suspended")]
    public StatusDirective? Suspended
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the revoked status directive.
    /// </summary>
    [JsonPropertyName("revoked")]
    public StatusDirective? Revoked
    {
        get; set;
    }

    /// <summary>
    /// Validates the status constraints.
    /// </summary>
    public void Validate()
    {
        if (Active == null && Suspended == null && Revoked == null)
            throw new InvalidOperationException("statuses must include at least one status property");

        Active?.Validate();
        Suspended?.Validate();
        Revoked?.Validate();
    }
}

/// <summary>
/// Represents a credential status directive.
/// </summary>
public class StatusDirective
{
    /// <summary>
    /// Gets or sets the directive value. Valid values are required, allowed, or disallowed.
    /// </summary>
    [JsonPropertyName("directive")]
    public string Directive { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets status type identifiers supported by this directive.
    /// </summary>
    [JsonPropertyName("type")]
    public string[]? Type
    {
        get; set;
    }

    /// <summary>
    /// Validates the status directive.
    /// </summary>
    public void Validate()
    {
        if (Directive != "required" && Directive != "allowed" && Directive != "disallowed")
            throw new InvalidOperationException("status directive must be 'required', 'allowed', or 'disallowed'");

        if (Type != null && Type.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("status type values cannot be null or empty");
    }
}
