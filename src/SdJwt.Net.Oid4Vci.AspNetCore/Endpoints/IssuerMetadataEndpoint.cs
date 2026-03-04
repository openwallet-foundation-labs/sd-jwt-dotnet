using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Models;
using SdJwt.Net.Oid4Vci.AspNetCore.Options;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Endpoints;

/// <summary>
/// Serves the OpenID Credential Issuer metadata document at
/// <c>/.well-known/openid-credential-issuer</c>. Supports in-process memory caching.
/// Implements OID4VCI 1.0 Section 11.2.
/// </summary>
public static class IssuerMetadataEndpoint
{
    private const string MetadataCacheKey = "oid4vci:issuer_metadata";

    /// <summary>
    /// Maps the issuer metadata endpoint to the route builder.
    /// </summary>
    /// <param name="app">The web application or endpoint route builder.</param>
    /// <returns>The route handler builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapOid4VciMetadata(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.MapGet("/.well-known/openid-credential-issuer", HandleAsync)
            .WithName("Oid4VciIssuerMetadata")
            .WithTags("OID4VCI")
            .Produces<CredentialIssuerMetadata>(StatusCodes.Status200OK)
            .CacheOutput(); // ASP.NET Core Output Cache (no-op if not registered)
    }

    private static IResult HandleAsync(
        HttpContext httpContext,
        [FromServices] IOptions<CredentialIssuerOptions> optionsAccessor,
        [FromServices] IMemoryCache cache,
        [FromServices] ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("SdJwt.Net.Oid4Vci.IssuerMetadataEndpoint");

        var options = optionsAccessor.Value;
        using var scope = BeginCorrelationScope(httpContext, options, logger);

        if (options.CacheMetadataSeconds > 0 &&
            cache.TryGetValue(MetadataCacheKey, out CredentialIssuerMetadata? cached) &&
            cached is not null)
        {
            logger.LogDebug("Serving issuer metadata from cache.");
            return Results.Ok(cached);
        }

        CredentialIssuerMetadata metadata;
        try
        {
            metadata = options.BuildMetadata();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Issuer metadata could not be built due to misconfiguration.");
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Issuer misconfiguration");
        }

        if (options.CacheMetadataSeconds > 0)
        {
            cache.Set(MetadataCacheKey, metadata, TimeSpan.FromSeconds(options.CacheMetadataSeconds));
            logger.LogDebug("Issuer metadata cached for {Seconds}s.", options.CacheMetadataSeconds);
        }

        logger.LogInformation("Serving issuer metadata. IssuerUrl={IssuerUrl}", options.IssuerUrl);
        return Results.Ok(metadata);
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
