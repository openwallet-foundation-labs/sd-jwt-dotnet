using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Engine;
using SdJwt.Net.PresentationExchange.Services;

namespace SdJwt.Net.PresentationExchange;

/// <summary>
/// Factory for creating presentation exchange components.
/// Provides easy access to the main functionality without dependency injection.
/// </summary>
public static class PresentationExchangeFactory
{
    /// <summary>
    /// Creates a presentation exchange engine with default configuration.
    /// </summary>
    /// <param name="loggerFactory">Optional logger factory for logging</param>
    /// <returns>A configured presentation exchange engine</returns>
    public static PresentationExchangeEngine CreateEngine(ILoggerFactory? loggerFactory = null)
    {
        var services = new ServiceCollection();

        // Add logging
        if (loggerFactory != null)
        {
            services.AddSingleton(loggerFactory);
        }
        else
        {
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        }

        // Add presentation exchange services
        services.AddPresentationExchange();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<PresentationExchangeEngine>();
    }

    /// <summary>
    /// Creates a presentation exchange engine with custom services.
    /// </summary>
    /// <param name="configureServices">Action to configure additional services</param>
    /// <returns>A configured presentation exchange engine</returns>
    public static PresentationExchangeEngine CreateEngine(Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();

        // Add default logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Add presentation exchange services
        services.AddPresentationExchange();

        // Apply custom configuration
        configureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<PresentationExchangeEngine>();
    }

    /// <summary>
    /// Creates a lightweight credential selector for simple use cases.
    /// This bypasses full DI setup and provides direct access to core functionality.
    /// </summary>
    /// <param name="loggerFactory">Optional logger factory</param>
    /// <returns>A simple credential selector</returns>
    public static SimpleCredentialSelector CreateSimpleSelector(ILoggerFactory? loggerFactory = null)
    {
        var engine = CreateEngine(loggerFactory);
        return new SimpleCredentialSelector(engine);
    }
}

/// <summary>
/// Extension methods for registering presentation exchange services.
/// </summary>
public static class PresentationExchangeServiceExtensions
{
    /// <summary>
    /// Adds presentation exchange services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPresentationExchange(this IServiceCollection services)
    {
        // Core services
        services.AddSingleton<JsonPathEvaluator>();
        services.AddSingleton<FieldFilterEvaluator>();
        services.AddSingleton<ConstraintEvaluator>();
        services.AddSingleton<CredentialFormatDetector>();
        services.AddSingleton<SubmissionRequirementEvaluator>();

        // Main engine
        services.AddSingleton<PresentationExchangeEngine>();

        return services;
    }

    /// <summary>
    /// Adds presentation exchange services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPresentationExchange(
        this IServiceCollection services,
        Action<PresentationExchangeConfiguration> configure)
    {
        var configuration = new PresentationExchangeConfiguration();
        configure(configuration);

        // Register configuration
        services.AddSingleton(configuration);

        // Add core services
        services.AddPresentationExchange();

        return services;
    }
}

/// <summary>
/// Configuration options for presentation exchange services.
/// </summary>
public class PresentationExchangeConfiguration
{
    /// <summary>
    /// Gets or sets the default credential selection options.
    /// </summary>
    public Models.CredentialSelectionOptions DefaultSelectionOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enable performance monitoring.
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;

    /// <summary>
    /// Gets or sets the default timeout for constraint evaluation.
    /// </summary>
    public TimeSpan DefaultConstraintEvaluationTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets custom credential evaluation extensions.
    /// </summary>
    public List<Models.ICredentialEvaluationExtension> EvaluationExtensions { get; set; } = new();

    /// <summary>
    /// Gets or sets custom format detectors for specialized credential types.
    /// </summary>
    public List<ICredentialFormatDetector> CustomFormatDetectors { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to cache constraint evaluation results.
    /// Can improve performance when evaluating the same credentials repeatedly.
    /// </summary>
    public bool EnableConstraintEvaluationCache { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum size of the constraint evaluation cache.
    /// </summary>
    public int ConstraintEvaluationCacheSize { get; set; } = 1000;
}

/// <summary>
/// Interface for custom credential format detectors.
/// </summary>
public interface ICredentialFormatDetector
{
    /// <summary>
    /// Gets the priority of this detector (higher = evaluated first).
    /// </summary>
    int Priority
    {
        get;
    }

    /// <summary>
    /// Attempts to detect the format of a credential.
    /// </summary>
    /// <param name="credential">The credential to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Format information if detected, null otherwise</returns>
    Task<CredentialFormatInfo?> TryDetectAsync(object credential, CancellationToken cancellationToken = default);
}

/// <summary>
/// Simple credential selector for basic use cases.
/// Provides a simplified API over the full presentation exchange engine.
/// </summary>
public class SimpleCredentialSelector
{
    private readonly PresentationExchangeEngine _engine;

    /// <summary>
    /// Initializes a new instance of the SimpleCredentialSelector class.
    /// </summary>
    /// <param name="engine">The presentation exchange engine</param>
    public SimpleCredentialSelector(PresentationExchangeEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <summary>
    /// Selects credentials from a wallet based on simple criteria.
    /// </summary>
    /// <param name="wallet">The credential wallet</param>
    /// <param name="credentialType">The required credential type</param>
    /// <param name="issuer">Optional required issuer</param>
    /// <param name="fields">Optional required fields</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Selected credentials</returns>
    public async Task<object[]> SelectByTypeAsync(
        IEnumerable<object> wallet,
        string credentialType,
        string? issuer = null,
        string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        var definition = CreateSimpleDefinition(credentialType, issuer, fields);
        var result = await _engine.SelectCredentialsAsync(definition, wallet, null, cancellationToken);

        if (result.IsSuccessful)
        {
            return result.SelectedCredentials.Select(c => c.Credential).ToArray();
        }

        return Array.Empty<object>();
    }

    /// <summary>
    /// Selects SD-JWT credentials with specific disclosures.
    /// </summary>
    /// <param name="wallet">The credential wallet</param>
    /// <param name="vctType">The required VCT type</param>
    /// <param name="requiredFields">The fields that must be disclosed</param>
    /// <param name="issuer">Optional required issuer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Selected credentials with their required disclosures</returns>
    public async Task<(object credential, string[]? disclosures)[]> SelectSdJwtWithDisclosuresAsync(
        IEnumerable<object> wallet,
        string vctType,
        string[] requiredFields,
        string? issuer = null,
        CancellationToken cancellationToken = default)
    {
        var definition = CreateSdJwtDefinition(vctType, requiredFields, issuer);
        var result = await _engine.SelectCredentialsAsync(definition, wallet, null, cancellationToken);

        if (result.IsSuccessful)
        {
            return result.SelectedCredentials
                .Select(c => (c.Credential, c.Disclosures))
                .ToArray();
        }

        return Array.Empty<(object, string[]?)>();
    }

    /// <summary>
    /// Checks if a credential satisfies simple criteria.
    /// </summary>
    /// <param name="credential">The credential to check</param>
    /// <param name="credentialType">The required credential type</param>
    /// <param name="issuer">Optional required issuer</param>
    /// <param name="fields">Optional required fields</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the credential satisfies the criteria</returns>
    public async Task<bool> CheckCredentialAsync(
        object credential,
        string credentialType,
        string? issuer = null,
        string[]? fields = null,
        CancellationToken cancellationToken = default)
    {
        var definition = CreateSimpleDefinition(credentialType, issuer, fields);
        var result = await _engine.SelectCredentialsAsync(definition, new[] { credential }, null, cancellationToken);

        return result.IsSuccessful && result.SelectedCredentials.Length > 0;
    }

    /// <summary>
    /// Creates a simple presentation definition for basic credential selection.
    /// </summary>
    /// <param name="credentialType">The required credential type</param>
    /// <param name="issuer">Optional required issuer</param>
    /// <param name="fields">Optional required fields</param>
    /// <returns>A presentation definition</returns>
    private Models.PresentationDefinition CreateSimpleDefinition(
        string credentialType,
        string? issuer,
        string[]? fields)
    {
        var constraints = new List<Models.Field>();

        // Add type constraint
        constraints.Add(Models.Field.CreateForType(credentialType));

        // Add issuer constraint if specified
        if (!string.IsNullOrEmpty(issuer))
        {
            constraints.Add(Models.Field.CreateForIssuer(issuer));
        }

        // Add field constraints if specified
        if (fields != null)
        {
            foreach (var field in fields)
            {
                constraints.Add(Models.Field.CreateForExistence($"$.{field}"));
            }
        }

        var descriptor = Models.InputDescriptor.CreateWithConstraints(
            "simple-descriptor",
            Models.Constraints.Create(constraints.ToArray()),
            $"Credential of type {credentialType}");

        return Models.PresentationDefinition.Create(
            "simple-definition",
            new[] { descriptor },
            "Simple credential selection");
    }

    /// <summary>
    /// Creates a presentation definition specifically for SD-JWT credentials.
    /// </summary>
    /// <param name="vctType">The required VCT type</param>
    /// <param name="requiredFields">The fields that must be available</param>
    /// <param name="issuer">Optional required issuer</param>
    /// <returns>A presentation definition for SD-JWT</returns>
    private Models.PresentationDefinition CreateSdJwtDefinition(
        string vctType,
        string[] requiredFields,
        string? issuer)
    {
        var constraints = new List<Models.Field>
                {
            // VCT type constraint
            Models.Field.CreateForValue(Models.PresentationExchangeConstants.CommonJsonPaths.VctType, vctType)
        };

        // Add issuer constraint if specified
        if (!string.IsNullOrEmpty(issuer))
        {
            constraints.Add(Models.Field.CreateForIssuer(issuer));
        }

        // Add field existence constraints
        foreach (var field in requiredFields)
        {
            constraints.Add(Models.Field.CreateForExistence($"$.{field}"));
        }

        var descriptor = Models.InputDescriptor.CreateForSdJwt(
            "sdjwt-descriptor",
            vctType,
            $"SD-JWT credential of type {vctType}");

        descriptor.Constraints = Models.Constraints.CreateWithSelectiveDisclosure(constraints.ToArray());

        return Models.PresentationDefinition.Create(
            "sdjwt-definition",
            new[] { descriptor },
            "SD-JWT credential selection");
    }
}
