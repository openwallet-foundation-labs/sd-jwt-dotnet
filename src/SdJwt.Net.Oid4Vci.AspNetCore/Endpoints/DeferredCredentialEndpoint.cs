using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Services;
using SdJwt.Net.Oid4Vci.Models;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Endpoints;

/// <summary>
/// Handles deferred credential retrieval requests.
/// Implements OID4VCI 1.0 Section 9.
/// </summary>
public static class DeferredCredentialEndpoint
{
    /// <summary>
    /// Maps the deferred credential endpoint to the route builder using the path from <see cref="CredentialIssuerOptions"/>.
    /// </summary>
    /// <param name="app">The web application or endpoint route builder.</param>
    /// <returns>The route handler builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapOid4VciDeferredCredential(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.ServiceProvider.GetRequiredService<IOptions<CredentialIssuerOptions>>().Value;

        return app.MapPost(options.DeferredCredentialEndpointPath, HandleAsync)
            .WithName("Oid4VciDeferredCredential")
            .WithTags("OID4VCI")
            .Produces<CredentialResponse>(StatusCodes.Status200OK)
            .Produces<CredentialErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromBody] DeferredCredentialRequest request,
        [FromServices] IAccessTokenService tokenService,
        [FromServices] IDeferredCredentialStore deferredStore,
        [FromServices] ICredentialIssuer credentialIssuer,
        [FromServices] IOptions<CredentialIssuerOptions> optionsAccessor,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("SdJwt.Net.Oid4Vci.DeferredCredentialEndpoint");
        var options = optionsAccessor.Value;
        using var scope = BeginCorrelationScope(httpContext, options, logger);

        var bearerToken = ExtractBearerToken(httpContext);
        if (bearerToken == null)
        {
            logger.LogWarning("Deferred credential request rejected: missing Authorization header.");
            return Results.Unauthorized();
        }

        var accessToken = await tokenService.ValidateAsync(bearerToken, cancellationToken).ConfigureAwait(false);
        if (accessToken == null)
        {
            logger.LogWarning("Deferred credential request rejected: invalid or expired access token.");
            return Results.Unauthorized();
        }

        if (request == null || string.IsNullOrWhiteSpace(request.AcceptanceToken))
        {
            logger.LogWarning("Deferred credential request missing acceptance_token.");
            return Results.BadRequest(new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidRequest,
                ErrorDescription = "The 'acceptance_token' field is required."
            });
        }

        var deferred = await deferredStore.RetrieveAsync(request.AcceptanceToken, cancellationToken).ConfigureAwait(false);
        if (deferred == null)
        {
            logger.LogWarning("Deferred credential not found or already redeemed. TransactionId={TransactionId}", Truncate(request.AcceptanceToken));
            return Results.BadRequest(new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidRequest,
                ErrorDescription = "The acceptance_token is invalid or has already been redeemed."
            });
        }

        CredentialIssuanceResult result;
        try
        {
            result = await credentialIssuer.IssueAsync(
                deferred.Value.Request,
                deferred.Value.AccessToken,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Deferred credential issuance failed.");
            return Results.BadRequest(new CredentialErrorResponse
            {
                Error = Oid4VciConstants.CredentialErrorCodes.InvalidRequest,
                ErrorDescription = ex.Message
            });
        }

        logger.LogInformation("Deferred credential issued successfully.");
        return Results.Ok(result.Response);
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

    private static string Truncate(string value) =>
        value.Length > 8 ? value[..8] + "..." : value;

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
