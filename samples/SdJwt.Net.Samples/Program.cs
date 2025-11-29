using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SdJwt.Net.Samples.Examples;
using SdJwt.Net.Samples.Scenarios;

namespace SdJwt.Net.Samples;

/// <summary>
/// Comprehensive SD-JWT .NET ecosystem demonstration
/// Showcases all packages: Core, VC, StatusList, OID4VCI, OID4VP, OpenID Federation, and Presentation Exchange
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup dependency injection and logging
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    builder.AddConsole()
                           .SetMinimumLevel(LogLevel.Information);
                });
                services.AddHttpClient();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           SD-JWT .NET Ecosystem - Comprehensive Demo        ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  Demonstrating all packages in the SD-JWT .NET ecosystem    ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Get user choice for what to demonstrate
            await ShowMainMenu(host.Services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during demonstration");
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static async Task ShowMainMenu(IServiceProvider services)
    {
        while (true)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                Choose a demonstration:                   ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                    CORE FEATURES                         ║");
            Console.WriteLine("║ 1. Core SD-JWT Features (RFC 9901)                      ║");
            Console.WriteLine("║ 2. JSON Serialization (JWS JSON)                        ║");
            Console.WriteLine("║                                                          ║");
            Console.WriteLine("║                  VERIFIABLE CREDENTIALS                  ║");
            Console.WriteLine("║ 3. Verifiable Credentials (SD-JWT VC)                   ║");
            Console.WriteLine("║ 4. Status Lists & Revocation                            ║");
            Console.WriteLine("║                                                          ║");
            Console.WriteLine("║                    PROTOCOLS                             ║");
            Console.WriteLine("║ 5. OpenID4VCI Credential Issuance                       ║");
            Console.WriteLine("║ 6. OpenID4VP Presentations                              ║");
            Console.WriteLine("║ 7. OpenID Federation & Trust                            ║");
            Console.WriteLine("║ 8. Presentation Exchange (DIF)                          ║");
            Console.WriteLine("║                                                          ║");
            Console.WriteLine("║                 ADVANCED FEATURES                        ║");
            Console.WriteLine("║ 9. Comprehensive Integration                             ║");
            Console.WriteLine("║ A. Cross-Platform Features                              ║");
            Console.WriteLine("║ B. Security Features                                    ║");
            Console.WriteLine("║                                                          ║");
            Console.WriteLine("║                 REAL-WORLD SCENARIOS                     ║");
            Console.WriteLine("║ C. Real-World Scenarios                                 ║");
            Console.WriteLine("║                                                          ║");
            Console.WriteLine("║ X. Run All Examples (Full Demo)                         ║");
            Console.WriteLine("║ 0. Exit                                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.Write("\nEnter your choice (1-9, A-C, X, or 0): ");

            var choice = Console.ReadLine()?.Trim().ToUpperInvariant();

            try
            {
                switch (choice)
                {
                    case "1":
                        await CoreSdJwtExample.RunExample(services);
                        break;
                    case "2":
                        await JsonSerializationExample.RunExample();
                        break;
                    case "3":
                        await VerifiableCredentialsExample.RunExample(services);
                        break;
                    case "4":
                        await StatusListExample.RunExample(services);
                        break;
                    case "5":
                        await OpenId4VciExample.RunExample(services);
                        break;
                    case "6":
                        await OpenId4VpExample.RunExample(services);
                        break;
                    case "7":
                        await OpenIdFederationExample.RunExample(services);
                        break;
                    case "8":
                        await PresentationExchangeExample.RunExample(services);
                        break;
                    case "9":
                        await ComprehensiveIntegrationExample.RunExample(services);
                        break;
                    case "A":
                        await CrossPlatformFeaturesExample.RunExample(services);
                        break;
                    case "B":
                        await SecurityFeaturesExample.RunExample(services);
                        break;
                    case "C":
                        await RealWorldScenariosExample.RunExample(services);
                        break;
                    case "X":
                        await RunAllExamples(services);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        continue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running example: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                Console.WriteLine("\nStack trace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to return to main menu...");
            Console.ReadKey();
        }
    }

    private static async Task RunAllExamples(IServiceProvider services)
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("RUNNING COMPLETE SD-JWT .NET ECOSYSTEM DEMONSTRATION");
        Console.WriteLine("This comprehensive demo will take approximately 10-15 minutes...");
        Console.WriteLine(new string('=', 80));

        var examples = new (string Name, Func<IServiceProvider, Task> Runner)[]
        {
            ("Core SD-JWT Features", CoreSdJwtExample.RunExample),
            ("JSON Serialization", async sp => await JsonSerializationExample.RunExample()),
            ("Verifiable Credentials", VerifiableCredentialsExample.RunExample),
            ("Status Lists & Revocation", StatusListExample.RunExample),
            ("OpenID4VCI Protocol", OpenId4VciExample.RunExample),
            ("OpenID4VP Protocol", OpenId4VpExample.RunExample),
            ("OpenID Federation", OpenIdFederationExample.RunExample),
            ("Presentation Exchange", PresentationExchangeExample.RunExample),
            ("Comprehensive Integration", ComprehensiveIntegrationExample.RunExample),
            ("Cross-Platform Features", CrossPlatformFeaturesExample.RunExample),
            ("Security Features", SecurityFeaturesExample.RunExample),
            ("Real-World Scenarios", RealWorldScenariosExample.RunExample)
        };

        int current = 0;
        int total = examples.Length;

        foreach (var (name, runner) in examples)
        {
            current++;
            Console.WriteLine($"\n[{current}/{total}] Running: {name}");
            Console.WriteLine(new string('-', 60));
            
            try
            {
                await runner(services);
                Console.WriteLine($"✓ {name} completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ {name} failed: {ex.Message}");
            }
            
            if (current < total)
            {
                Console.WriteLine("\nWaiting 2 seconds before next example...");
                await Task.Delay(2000);
            }
        }

        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("🎉 COMPLETE SD-JWT .NET ECOSYSTEM DEMONSTRATION FINISHED!");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();
        Console.WriteLine("Summary of demonstrated features:");
        Console.WriteLine("✓ RFC 9901 compliant SD-JWT core functionality");
        Console.WriteLine("✓ Verifiable Credentials with selective disclosure");
        Console.WriteLine("✓ Status lists for revocation and suspension");
        Console.WriteLine("✓ OpenID4VCI credential issuance protocols");
        Console.WriteLine("✓ OpenID4VP presentation verification protocols");
        Console.WriteLine("✓ OpenID Federation trust management");
        Console.WriteLine("✓ DIF Presentation Exchange integration");
        Console.WriteLine("✓ Advanced integration patterns and workflows");
        Console.WriteLine("✓ Cross-platform compatibility features");
        Console.WriteLine("✓ Comprehensive security implementations");
        Console.WriteLine("✓ Real-world scenario demonstrations");
        Console.WriteLine();
        Console.WriteLine("The SD-JWT .NET ecosystem provides enterprise-grade");
        Console.WriteLine("selective disclosure and verifiable credential capabilities");
        Console.WriteLine("suitable for production deployment across industries.");
    }
}

































































































































































































































































