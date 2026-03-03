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
/// Handles token requests for the OID4VCI pre-authorized code grant flow.
/// Implements OID4VCI 1.0 Section 6.
/// </summary>
public static class TokenEndpoint
{
    /// <summary>
    /// Maps the token endpoint to the route builder using the path from <see cref="CredentialIssuerOptions"/>.
    /// </summary>
    /// <param name="app">The web application or endpoint route builder.</param>
    /// <returns>The route handler builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapOid4VciToken(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.ServiceProvider.GetRequiredService<IOptions<CredentialIssuerOptions>>().Value;

        var builder = app.MapPost(options.TokenEndpointPath, HandleAsync)
            .WithName("Oid4VciToken")
            .WithTags("OID4VCI")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces<TokenErrorResponse>(StatusCodes.Status400BadRequest);

        if (options.EnableRateLimiting)
        {
            builder.RequireRateLimiting(options.RateLimiterPolicyName);
        }

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromBody] TokenRequest request,
        [FromServices] IAccessTokenService tokenService,
        [FromServices] IOptions<CredentialIssuerOptions> optionsAccessor,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("SdJwt.Net.Oid4Vci.TokenEndpoint");
        var options = optionsAccessor.Value;
        using var scope = BeginCorrelationScope(httpContext, options, logger);

        if (request == null)
        {
            logger.LogWarning("Token request body is missing.");
            return Results.BadRequest(new TokenErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = "Request body is required."
            });
        }

        try
        {
            request.Validate();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Token request validation failed.");
            return Results.BadRequest(new TokenErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message
            });
        }

        if (request.GrantType != Oid4VciConstants.GrantTypes.PreAuthorizedCode)
        {
            logger.LogWarning("Unsupported grant type requested. GrantType={GrantType}", request.GrantType);
            return Results.BadRequest(new TokenErrorResponse
            {
                Error = "unsupported_grant_type",
                ErrorDescription = $"Grant type '{request.GrantType}' is not supported."
            });
        }

        var issued = await tokenService.IssueForPreAuthorizedCodeAsync(
            request.PreAuthorizedCode!,
            request.TransactionCode,
            cancellationToken).ConfigureAwait(false);

        if (issued == null)
        {
            logger.LogWarning("Token issuance failed: invalid or used pre-authorized code.");
            return Results.BadRequest(new TokenErrorResponse
            {
                Error = "invalid_grant",
                ErrorDescription = "The pre-authorized code is invalid, expired, or already used."
            });
        }

        logger.LogInformation(
            "Token issued. Configurations={Configs} ExpiresIn={ExpiresIn}s",
            string.Join(",", issued.AuthorizedConfigurationIds),
            issued.ExpiresInSeconds);

        return Results.Ok(TokenResponse.Success(
            issued.Token,
            issued.ExpiresInSeconds,
            issued.CNonce,
            issued.CNonceExpiresInSeconds));
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
