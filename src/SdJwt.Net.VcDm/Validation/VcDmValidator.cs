using SdJwt.Net.VcDm.Models;

namespace SdJwt.Net.VcDm.Validation;

/// <summary>
/// Structural validator for W3C VCDM 2.0 <see cref="VerifiableCredential"/> and
/// <see cref="VerifiablePresentation"/> documents.
/// Does NOT perform cryptographic verification or JSON-LD context expansion.
/// </summary>
public sealed class VcDmValidator
{
    /// <summary>
    /// Validates the required structure of a <see cref="VerifiableCredential"/>.
    /// Checks: @context[0], type contains "VerifiableCredential", issuer present,
    /// credentialSubject present, status entries have type, schema entries have id+type.
    /// </summary>
    public VcDmValidationResult Validate(VerifiableCredential credential)
    {
        var errors = new List<string>();

        ValidateContext(credential.Context, errors);
        ValidateTypeArray(credential.Type, "VerifiableCredential", errors);

        if (credential.Issuer is null)
            errors.Add("issuer is required.");

        if (credential.CredentialSubject is null || credential.CredentialSubject.Length == 0)
            errors.Add("credentialSubject is required and must not be empty.");

        if (credential.CredentialStatus is not null)
        {
            foreach (var (status, i) in credential.CredentialStatus.Select((s, i) => (s, i)))
            {
                if (string.IsNullOrWhiteSpace(status.Type))
                    errors.Add($"credentialStatus[{i}].type is required.");
            }
        }

        if (credential.CredentialSchema is not null)
        {
            foreach (var (schema, i) in credential.CredentialSchema.Select((s, i) => (s, i)))
            {
                if (string.IsNullOrWhiteSpace(schema.Id))
                    errors.Add($"credentialSchema[{i}].id is required.");
                if (string.IsNullOrWhiteSpace(schema.Type))
                    errors.Add($"credentialSchema[{i}].type is required.");
            }
        }

        if (credential.TermsOfUse is not null)
        {
            foreach (var (tou, i) in credential.TermsOfUse.Select((t, i) => (t, i)))
            {
                if (string.IsNullOrWhiteSpace(tou.Type))
                    errors.Add($"termsOfUse[{i}].type is required.");
            }
        }

        if (credential.Evidence is not null)
        {
            foreach (var (ev, i) in credential.Evidence.Select((e, i) => (e, i)))
            {
                if (ev.Type is null || ev.Type.Length == 0)
                    errors.Add($"evidence[{i}].type is required.");
            }
        }

        if (credential.ValidFrom.HasValue && credential.ValidUntil.HasValue &&
            credential.ValidFrom > credential.ValidUntil)
        {
            errors.Add("validFrom must not be later than validUntil.");
        }

        return errors.Count == 0
            ? VcDmValidationResult.Success()
            : VcDmValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates the required structure of a <see cref="VerifiablePresentation"/>.
    /// </summary>
    public VcDmValidationResult Validate(VerifiablePresentation presentation)
    {
        var errors = new List<string>();

        ValidateContext(presentation.Context, errors);
        ValidateTypeArray(presentation.Type, "VerifiablePresentation", errors);

        if (presentation.TermsOfUse is not null)
        {
            foreach (var (tou, i) in presentation.TermsOfUse.Select((t, i) => (t, i)))
            {
                if (string.IsNullOrWhiteSpace(tou.Type))
                    errors.Add($"termsOfUse[{i}].type is required.");
            }
        }

        return errors.Count == 0
            ? VcDmValidationResult.Success()
            : VcDmValidationResult.Failure(errors);
    }

    private static void ValidateContext(string[]? context, List<string> errors)
    {
        if (context is null || context.Length == 0)
        {
            errors.Add("@context is required.");
            return;
        }

        if (!VcDmContexts.IsAcceptedBaseContext(context[0]))
        {
            errors.Add(
                $"@context[0] must be '{VcDmContexts.V2}' (or '{VcDmContexts.V1}' for VCDM 1.1 backward compatibility). " +
                $"Found: '{context[0]}'.");
        }
    }

    private static void ValidateTypeArray(string[]? type, string requiredType, List<string> errors)
    {
        if (type is null || type.Length == 0)
        {
            errors.Add($"type is required and must contain \"{requiredType}\".");
            return;
        }

        if (!type.Contains(requiredType))
            errors.Add($"type must contain \"{requiredType}\".");
    }
}
