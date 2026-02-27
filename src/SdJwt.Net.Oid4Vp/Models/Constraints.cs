using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents Constraints according to DIF Presentation Exchange v2.0.0.
/// Contains requirements the Verifier has for the Credential.
/// </summary>
public class Constraints
{
    /// <summary>
    /// Gets or sets the constraint fields.
    /// OPTIONAL. Array of field constraints that must be satisfied.
    /// </summary>
    [JsonPropertyName("fields")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Field[]? Fields
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the limit disclosure policy.
    /// OPTIONAL. Controls selective disclosure behavior.
    /// Values: "required" (default) or "preferred".
    /// </summary>
    [JsonPropertyName("limit_disclosure")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? LimitDisclosure
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the status constraints.
    /// OPTIONAL. Object containing status constraints for the credential.
    /// </summary>
    [JsonPropertyName("statuses")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public StatusConstraints? Statuses
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the subject issuer constraint.
    /// OPTIONAL. Constraint on which entity may have issued the Subject's Identifier.
    /// </summary>
    [JsonPropertyName("subject_is_issuer")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? SubjectIsIssuer
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the same subject constraint.
    /// OPTIONAL. Constraint that requires all credentials to have the same subject.
    /// </summary>
    [JsonPropertyName("same_subject")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? SameSubject
    {
        get; set;
    }

    /// <summary>
    /// Creates constraints with required selective disclosure.
    /// </summary>
    /// <param name="fields">Array of field constraints</param>
    /// <returns>A new Constraints instance</returns>
    public static Constraints CreateWithRequiredDisclosure(params Field[] fields)
    {
        return new Constraints
        {
            Fields = fields,
            LimitDisclosure = "required"
        };
    }

    /// <summary>
    /// Creates constraints with preferred selective disclosure.
    /// </summary>
    /// <param name="fields">Array of field constraints</param>
    /// <returns>A new Constraints instance</returns>
    public static Constraints CreateWithPreferredDisclosure(params Field[] fields)
    {
        return new Constraints
        {
            Fields = fields,
            LimitDisclosure = "preferred"
        };
    }

    /// <summary>
    /// Validates this constraints object according to DIF Presentation Exchange requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
    public void Validate()
    {
        // Validate limit disclosure value if present
        if (!string.IsNullOrWhiteSpace(LimitDisclosure) &&
            LimitDisclosure != "required" &&
            LimitDisclosure != "preferred")
        {
            throw new InvalidOperationException("limit_disclosure must be 'required' or 'preferred'");
        }

        // Validate fields if present
        if (Fields != null)
        {
            foreach (var field in Fields)
            {
                field?.Validate();
            }
        }

        // Validate status constraints if present
        Statuses?.Validate();
    }
}

/// <summary>
/// Represents Status Constraints according to DIF Presentation Exchange v2.0.0.
/// Controls credential status requirements.
/// </summary>
public class StatusConstraints
{
    /// <summary>
    /// Gets or sets the active status constraint.
    /// OPTIONAL. Requires credential to have active status.
    /// </summary>
    [JsonPropertyName("active")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public StatusDirective? Active
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the suspended status constraint.
    /// OPTIONAL. Controls suspended status requirement.
    /// </summary>
    [JsonPropertyName("suspended")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public StatusDirective? Suspended
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the revoked status constraint.
    /// OPTIONAL. Controls revoked status requirement.
    /// </summary>
    [JsonPropertyName("revoked")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public StatusDirective? Revoked
    {
        get; set;
    }

    /// <summary>
    /// Validates this status constraints object.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the constraints are invalid</exception>
    public void Validate()
    {
        Active?.Validate();
        Suspended?.Validate();
        Revoked?.Validate();
    }
}

/// <summary>
/// Represents a Status Directive according to DIF Presentation Exchange v2.0.0.
/// Specifies requirements for credential status.
/// </summary>
public class StatusDirective
{
    /// <summary>
    /// Gets or sets the directive value.
    /// REQUIRED. Must be "required", "allowed", or "disallowed".
    /// </summary>
    [JsonPropertyName("directive")]
    public string Directive { get; set; } = string.Empty;

    /// <summary>
    /// Creates a status directive that requires the status.
    /// </summary>
    /// <returns>A new StatusDirective instance</returns>
    public static StatusDirective Required()
    {
        return new StatusDirective { Directive = "required" };
    }

    /// <summary>
    /// Creates a status directive that allows the status.
    /// </summary>
    /// <returns>A new StatusDirective instance</returns>
    public static StatusDirective Allowed()
    {
        return new StatusDirective { Directive = "allowed" };
    }

    /// <summary>
    /// Creates a status directive that disallows the status.
    /// </summary>
    /// <returns>A new StatusDirective instance</returns>
    public static StatusDirective Disallowed()
    {
        return new StatusDirective { Directive = "disallowed" };
    }

    /// <summary>
    /// Validates this status directive.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the directive is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Directive))
        {
            throw new InvalidOperationException("Status directive is required");
        }

        if (Directive != "required" &&
            Directive != "allowed" &&
            Directive != "disallowed")
        {
            throw new InvalidOperationException("Status directive must be 'required', 'allowed', or 'disallowed'");
        }
    }
}
