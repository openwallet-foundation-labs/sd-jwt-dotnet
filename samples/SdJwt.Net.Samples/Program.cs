using SdJwt.Net.Samples.Beginner;
using SdJwt.Net.Samples.Intermediate;
using SdJwt.Net.Samples.Advanced;
using SdJwt.Net.Samples.UseCases.Education;
using SdJwt.Net.Samples.UseCases.Finance;
using SdJwt.Net.Samples.UseCases.Healthcare;
using SdJwt.Net.Samples.UseCases.Government;
using SdJwt.Net.Samples.UseCases.Retail;
using SdJwt.Net.Samples.UseCases.Telecom;

namespace SdJwt.Net.Samples;

/// <summary>
/// SD-JWT .NET Tutorial Launcher
///
/// Progressive learning path from basics to real-world applications.
/// Run individual tutorials or entire categories.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine("          SD-JWT .NET - Tutorial Sample Collection");
        Console.WriteLine("================================================================");
        Console.WriteLine();

        if (args.Length > 0)
        {
            // Run specific tutorial by argument
            await RunTutorialByKey(args[0]);
            return;
        }

        // Interactive menu
        await ShowMainMenu();
    }

    private static async Task ShowMainMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("================================================================");
            Console.WriteLine("                    SELECT A TUTORIAL");
            Console.WriteLine("================================================================");
            Console.WriteLine();
            Console.WriteLine("  BEGINNER (Start Here)");
            Console.WriteLine("  ----------------------");
            Console.WriteLine("  1.1  Hello SD-JWT           - First SD-JWT in 5 minutes");
            Console.WriteLine("  1.2  Selective Disclosure   - Hide/reveal claims");
            Console.WriteLine("  1.3  Holder Binding         - Prove ownership with keys");
            Console.WriteLine("  1.4  Verification Flow      - Complete verify cycle");
            Console.WriteLine();
            Console.WriteLine("  INTERMEDIATE (Build Skills)");
            Console.WriteLine("  ----------------------------");
            Console.WriteLine("  2.1  Verifiable Credentials - SD-JWT VC standard");
            Console.WriteLine("  2.2  Status List            - Revocation and suspension");
            Console.WriteLine("  2.3  OpenID4VCI             - Credential issuance");
            Console.WriteLine("  2.4  OpenID4VP              - Presentation protocol");
            Console.WriteLine("  2.5  Presentation Exchange  - DIF query language");
            Console.WriteLine();
            Console.WriteLine("  ADVANCED (Production Ready)");
            Console.WriteLine("  ----------------------------");
            Console.WriteLine("  3.1  OpenID Federation      - Trust chains");
            Console.WriteLine("  3.2  HAIP Compliance        - High assurance profiles");
            Console.WriteLine("  3.3  Multi-Credential Flow  - Combined presentations");
            Console.WriteLine("  3.4  Key Rotation           - Operational security");
            Console.WriteLine();
            Console.WriteLine("  USE CASES (Real World)");
            Console.WriteLine("  -----------------------");
            Console.WriteLine("  4.1  University Degree      - Education credentials");
            Console.WriteLine("  4.2  Loan Application       - Financial privacy");
            Console.WriteLine("  4.3  Patient Consent        - Healthcare HIPAA");
            Console.WriteLine("  4.4  Cross-Border Identity  - Government travel");
            Console.WriteLine("  4.5  Fraud-Resistant Returns- Retail receipts");
            Console.WriteLine("  4.6  eSIM Transfer          - Telecom porting");
            Console.WriteLine();
            Console.WriteLine("  BATCH RUN");
            Console.WriteLine("  ---------");
            Console.WriteLine("  B1   Run all Beginner tutorials");
            Console.WriteLine("  B2   Run all Intermediate tutorials");
            Console.WriteLine("  B3   Run all Advanced tutorials");
            Console.WriteLine("  B4   Run all Use Cases");
            Console.WriteLine("  ALL  Run everything");
            Console.WriteLine();
            Console.WriteLine("  Q    Quit");
            Console.WriteLine();
            Console.Write("  Enter choice: ");

            var input = Console.ReadLine()?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(input) || input == "Q")
                break;

            Console.WriteLine();
            await RunTutorialByKey(input);

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    private static async Task RunTutorialByKey(string key)
    {
        try
        {
            switch (key.ToUpperInvariant())
            {
                // Beginner
                case "1.1":
                    await HelloSdJwt.Run();
                    break;
                case "1.2":
                    await SelectiveDisclosure.Run();
                    break;
                case "1.3":
                    await HolderBinding.Run();
                    break;
                case "1.4":
                    await VerificationFlow.Run();
                    break;

                // Intermediate
                case "2.1":
                    await VerifiableCredentials.Run();
                    break;
                case "2.2":
                    await StatusListTutorial.Run();
                    break;
                case "2.3":
                    await OpenId4Vci.Run();
                    break;
                case "2.4":
                    await OpenId4Vp.Run();
                    break;
                case "2.5":
                    await PresentationExchangeTutorial.Run();
                    break;

                // Advanced
                case "3.1":
                    await OpenIdFederation.Run();
                    break;
                case "3.2":
                    await HaipCompliance.Run();
                    break;
                case "3.3":
                    await MultiCredentialFlow.Run();
                    break;
                case "3.4":
                    await KeyRotation.Run();
                    break;

                // Use Cases
                case "4.1":
                    await UniversityDegree.Run();
                    break;
                case "4.2":
                    await LoanApplication.Run();
                    break;
                case "4.3":
                    await PatientConsent.Run();
                    break;
                case "4.4":
                    await CrossBorderIdentity.Run();
                    break;
                case "4.5":
                    await FraudResistantReturns.Run();
                    break;
                case "4.6":
                    await EsimTransfer.Run();
                    break;

                // Batch runs
                case "B1":
                    await RunCategory("Beginner", new Func<Task>[]
                    {
                        HelloSdJwt.Run,
                        SelectiveDisclosure.Run,
                        HolderBinding.Run,
                        VerificationFlow.Run
                    });
                    break;

                case "B2":
                    await RunCategory("Intermediate", new Func<Task>[]
                    {
                        VerifiableCredentials.Run,
                        StatusListTutorial.Run,
                        OpenId4Vci.Run,
                        OpenId4Vp.Run,
                        PresentationExchangeTutorial.Run
                    });
                    break;

                case "B3":
                    await RunCategory("Advanced", new Func<Task>[]
                    {
                        OpenIdFederation.Run,
                        HaipCompliance.Run,
                        MultiCredentialFlow.Run,
                        KeyRotation.Run
                    });
                    break;

                case "B4":
                    await RunCategory("Use Cases", new Func<Task>[]
                    {
                        UniversityDegree.Run,
                        LoanApplication.Run,
                        PatientConsent.Run,
                        CrossBorderIdentity.Run,
                        FraudResistantReturns.Run,
                        EsimTransfer.Run
                    });
                    break;

                case "ALL":
                    await RunAllTutorials();
                    break;

                default:
                    Console.WriteLine($"Unknown tutorial: {key}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task RunCategory(string name, Func<Task>[] tutorials)
    {
        Console.WriteLine($"Running all {name} tutorials...");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        var count = 0;
        foreach (var tutorial in tutorials)
        {
            count++;
            await tutorial();
            Console.WriteLine();

            if (count < tutorials.Length)
            {
                Console.WriteLine("--- Next tutorial in 2 seconds ---");
                await Task.Delay(2000);
                Console.WriteLine();
            }
        }

        Console.WriteLine($"Completed {count} {name} tutorials.");
    }

    private static async Task RunAllTutorials()
    {
        var allTutorials = new (string Category, Func<Task>[] Tutorials)[]
        {
            ("Beginner", new Func<Task>[]
            {
                HelloSdJwt.Run,
                SelectiveDisclosure.Run,
                HolderBinding.Run,
                VerificationFlow.Run
            }),
            ("Intermediate", new Func<Task>[]
            {
                VerifiableCredentials.Run,
                StatusListTutorial.Run,
                OpenId4Vci.Run,
                OpenId4Vp.Run,
                PresentationExchangeTutorial.Run
            }),
            ("Advanced", new Func<Task>[]
            {
                OpenIdFederation.Run,
                HaipCompliance.Run,
                MultiCredentialFlow.Run,
                KeyRotation.Run
            }),
            ("Use Cases", new Func<Task>[]
            {
                UniversityDegree.Run,
                LoanApplication.Run,
                PatientConsent.Run,
                CrossBorderIdentity.Run,
                FraudResistantReturns.Run,
                EsimTransfer.Run
            })
        };

        var totalCount = 0;
        foreach (var (category, tutorials) in allTutorials)
        {
            Console.WriteLine();
            Console.WriteLine($"================ {category.ToUpperInvariant()} ================");
            Console.WriteLine();

            foreach (var tutorial in tutorials)
            {
                totalCount++;
                await tutorial();
                Console.WriteLine();
                await Task.Delay(1000);
            }
        }

        Console.WriteLine();
        Console.WriteLine("================================================================");
        Console.WriteLine($"               ALL {totalCount} TUTORIALS COMPLETED");
        Console.WriteLine("================================================================");
    }
}
