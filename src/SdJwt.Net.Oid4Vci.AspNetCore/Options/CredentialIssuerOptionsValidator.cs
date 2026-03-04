using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Options;

/// <summary>
/// Validates <see cref="CredentialIssuerOptions"/> at application startup using
/// Data Annotations so that misconfiguration is caught early rather than at runtime.
/// </summary>
internal sealed class CredentialIssuerOptionsValidator : IValidateOptions<CredentialIssuerOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, CredentialIssuerOptions options)
    {
        var errors = new List<string>();
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(options);

        if (!Validator.TryValidateObject(options, context, validationResults, validateAllProperties: true))
        {
            foreach (var result in validationResults)
            {
                errors.Add(result.ErrorMessage ?? "Unknown validation error.");
            }
        }

        if (options.EnableRateLimiting && string.IsNullOrWhiteSpace(options.RateLimiterPolicyName))
        {
            errors.Add($"{nameof(options.RateLimiterPolicyName)} must be set when {nameof(options.EnableRateLimiting)} is true.");
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
