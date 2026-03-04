using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Services;
using SdJwt.Net.Oid4Vci.Issuer;
using SdJwt.Net.Oid4Vci.Models;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Endpoints;

/// <summary>
/// Handles credential requests authenticated via Bearer access token.
/// Implements OID4VCI 1.0 Section 7.
/// </summary>
public static class CredentialEndpoint
{
    /// <summary>
    /// Maps the credential endpoint to the route builder using the path from <see cref="CredentialIssuerOptions"/>.
    /// </summary>
    /// <param name="app">The web application or endpoint route builder.</param>
    /// <returns>The route handler builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapOid4VciCredential(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.ServiceProvider.GetRequiredService<IOptions<CredentialIssuerOptions>>().Value;

        var builder = app.MapPost(options.CredentialEndpointPath, HandleAsync)
            .WithName("Oid4VciCredential")
            .WithTags("OID4VCI")
            .Produces<CredentialResponse>(StatusCodes.Status200OK)
            .Produces<CredentialErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        if (options.EnableRateLimiting)
        {
            builder.RequireRateLimiting(options.RateLimiterPolicyName);
        }

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromBody] CredentialRequest request,
        [FromServices] IAccessTokenService tokenService,
        [FromServices] ICredentialIssuer credentialIssuer,
        [FromServices] IDeferredCredentialStore deferredStore,
        [FromServices] IOptions<CredentialIssuerOptions> optionsAccessor,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("SdJwt.Net.Oid4Vci.CredentialEndpoint");
        var options = optionsAccessor.Value;
        using var scope = BeginCorrelationScope(httpContext, options, logger);

        var bearerToken = ExtractBearerToken(httpContext);
        if (bearerToken == null)
        {
            logger.LogWarning("Credential request rejected: missing or malformed Authorization header.");
            return Results.Unauthorized();
        }

        var accessToken = await tokenService.ValidateAsync(bearerToken, cancellationToken).ConfigureAwait(false);
        if (accessToken == null)
        {
            logger.LogWarning("Credential request rejected: invalid or expired access token.");
            return Results.Unauthorized();
        }

        if (request == null)
        {
            logger.LogWarning("Credential request body is missing.");
            return Results.BadRequest(new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidRequest,
                ErrorDescription = "Request body is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Format))
        {
            logger.LogWarning("Credential request missing 'format' field.");
            return Results.BadRequest(new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidRequest,
                ErrorDescription = "The 'format' field is required."
            });
        }

        if (request.Proof != null)
        {
            var validationError = ValidateProof(request.Proof, accessToken.CNonce, options);
            if (validationError != null)
            {
                logger.LogWarning("Proof validation failed. Error={Error}", validationError.Error);
                return Results.BadRequest(validationError);
            }
        }

        CredentialIssuanceResult result;
        try
        {
            result = await credentialIssuer.IssueAsync(request, bearerToken, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Credential issuance failed. Format={Format}", request.Format);
            return Results.BadRequest(new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidRequest,
                ErrorDescription = ex.Message
            });
        }

        if (result.IsDeferred && result.Response.AcceptanceToken != null)
        {
            await deferredStore.SaveAsync(
                result.Response.AcceptanceToken,
                request,
                bearerToken,
                cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Credential issuance deferred. Format={Format}", request.Format);
        }
        else
        {
            logger.LogInformation("Credential issued immediately. Format={Format}", request.Format);
        }

        return Results.Ok(result.Response);
    }

    private static CredentialErrorResponse? ValidateProof(
        CredentialProof proof,
        string expectedCNonce,
        CredentialIssuerOptions options)
    {
        if (proof.ProofType != Oid4VciConstants.ProofTypes.Jwt)
        {
            return new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidProof,
                ErrorDescription = $"Proof type '{proof.ProofType}' is not supported."
            };
        }

        if (string.IsNullOrWhiteSpace(proof.Jwt))
        {
            return new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidOrMissingProof,
                ErrorDescription = "JWT proof is required when proof_type is 'jwt'."
            };
        }

        try
        {
            CNonceValidator.ValidateProof(proof.Jwt, expectedCNonce, options.IssuerUrl);
        }
        catch (ProofValidationException ex)
        {
            return new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidProof,
                ErrorDescription = ex.Message,
                CNonce = expectedCNonce
            };
        }

        return null;
    }

    private static string? ExtractBearerToken(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }

    private static IDisposable? BeginCorrelationScope(
        HttpContext httpContext,
        CredentialIssuerOptions options,
        ILogger logger)
    {
        if (!options.EnableCorrelationId)
        {
            return null;
        }

        var correlationId = httpContext.Request.Headers.TryGetValue(
            options.CorrelationIdHeaderName, out var values)
            ? values.ToString()
            : httpContext.TraceIdentifier;

        return logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });
    }
}
