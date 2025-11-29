using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RichardSzalay.MockHttp;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Samples;
using SdJwt.Net.Verifier;
using SdJwt.Net.Utils;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

// --- Logger Setup ---
// A simple console logger factory to demonstrate structured logging from the SDK.
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options => { options.SingleLine = true; options.TimestampFormat = "HH:mm:ss "; })
           .SetMinimumLevel(LogLevel.Debug); // Set to Debug to see all logs from the SDK.
});

Console.WriteLine("==================================================");
Console.WriteLine("  SD-JWT-VC Full Lifecycle Demo (with Revocation)");
Console.WriteLine("==================================================\n");

// --- 1. SETUP: Keys, constants, and a mock HTTP server for the Status List ---
var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerSigningKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-key-1" };
const string issuerSigningAlgorithm = SecurityAlgorithms.EcdsaSha256;

// 1. Create a single ECDsa instance that holds both private and public keys.
using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

// 2. The private key for signing is a wrapper around the full instance.
var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };

// 3. The public key for validation is ALSO a wrapper around the same full instance.
//    The `EcdsaSecurityKey` implementation knows to only use the public part for validation.
//    This guarantees the keys match.
var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };

// 4. The JWK is created from the public key.
var holderPublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);
const string holderSigningAlgorithm = SecurityAlgorithms.EcdsaSha256;


// --- Common constants and Mock HTTP Server Setup (unchanged) ---
const string issuerName = "https://issuer.example.com";
const string verifierAudience = "https://verifier.example.com";
const string statusListUrl = "https://issuer.example.com/status/1";
var statusListManager = new StatusListManager(issuerSigningKey, issuerSigningAlgorithm);
var statusBits = new BitArray(100, false) { [42] = true };
var statusListCredentialJwt = await statusListManager.CreateStatusListTokenFromBitArrayAsync(issuerName, statusBits);
var mockHttp = new MockHttpMessageHandler();
mockHttp.When(statusListUrl).Respond("application/jwt", statusListCredentialJwt);
var httpClient = new HttpClient(mockHttp);
Console.WriteLine("Setup complete. Mock HTTP endpoint for Status List is ready.");

// --- 2. ISSUER: Creates the SD-JWT-VC ---
Console.WriteLine("\n--- Issuer's Turn ---");
var vcIssuer = new SdJwtVcIssuer(
    issuerSigningKey,
    issuerSigningAlgorithm,
    logger: loggerFactory.CreateLogger<SdIssuer>()
);
var vcPayload = new SdJwtVcPayload
{
    Issuer = issuerName,
    Subject = "did:example:holder123",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Status = new { status_list = new StatusListReference { Index = 42, Uri = statusListUrl } }, // This VC's status is at index 42, which we know is revoked.
    AdditionalData = new Dictionary<string, object>
    {
        { "given_name", "Jane" }, 
        { "family_name", "Doe" }, 
        { "email", "janedoe@example.com" }
    }
};
var issuanceOptions = new SdIssuanceOptions { DisclosureStructure = new { email = true } };
var issuerOutput = vcIssuer.Issue("https://example.com/credentials/verified-employee", vcPayload, issuanceOptions, holderPublicJwk);
Console.WriteLine($"Issuer created a VC of type 'verified-employee', linked to status list index 42.");


// --- 3. HOLDER: Creates a presentation ---
Console.WriteLine("\n--- Holder's Turn ---");
var holder = new SdJwtHolder(issuerOutput.Issuance, loggerFactory.CreateLogger<SdJwtHolder>());
var presentation = holder.CreatePresentation(
    disclosureSelector: disclosure => disclosure.ClaimName.Contains("email"),
    kbJwtPayload: new JwtPayload { { "aud", verifierAudience }, { "nonce", "123-abc-456" } },
    kbJwtSigningKey: holderPrivateKey,
    kbJwtSigningAlgorithm: holderSigningAlgorithm
);
Console.WriteLine("Holder created a presentation disclosing only 'email'.");


// --- 4. VERIFIER: Verifies the (revoked) presentation ---
Console.WriteLine("\n--- Verifier's Turn ---");
var issuerKeyProvider = (JwtSecurityToken token) => Task.FromResult<SecurityKey>(issuerSigningKey);
var vcVerifier = new SdJwtVcVerifier(issuerKeyProvider, loggerFactory.CreateLogger<SdJwtVcVerifier>());

// The validation parameters for the KB-JWT are still needed.
// This will now work because holderPublicKey correctly corresponds to holderPrivateKey.
var validationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = issuerName,
    ValidateAudience = false,
    ValidateLifetime = true
};

var kbJwtValidationParams = new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidAudience = verifierAudience,
    ValidateAudience = true,
    IssuerSigningKey = holderPublicKey
};

Console.WriteLine("\nAttempting to verify a REVOKED credential...");
try
{
    await vcVerifier.VerifyAsync(presentation, validationParams, kbJwtValidationParams);
}
catch (SecurityTokenException ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\nSUCCESS: Verification correctly failed as expected.");
    Console.WriteLine($"Reason: {ex.Message}");

    if (!ex.Message.Contains("is marked as invalid/revoked", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Note: This might be a different validation error, not specifically revocation-related.");
    }

    Console.ResetColor();
}

// --- 5. VERIFIER: Verifies a VALID presentation ---
Console.WriteLine("\nAttempting to verify a VALID credential...");
vcPayload.Status = new { status_list = new StatusListReference { Index = 10, Uri = statusListUrl } }; // Change status to a valid index (10)
var validIssuerOutput = vcIssuer.Issue("https://example.com/credentials/verified-employee", vcPayload, issuanceOptions, holderPublicJwk);
var validHolder = new SdJwtHolder(validIssuerOutput.Issuance);
var validPresentation = validHolder.CreatePresentation(
    disclosureSelector: disclosure => disclosure.ClaimName.Contains("email"),
    kbJwtPayload: new JwtPayload { { "aud", verifierAudience }, { "nonce", "789-def-012" } },
    kbJwtSigningKey: holderPrivateKey,
    kbJwtSigningAlgorithm: holderSigningAlgorithm
);

try
{
    var result = await vcVerifier.VerifyAsync(validPresentation, validationParams, kbJwtValidationParams);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\nSUCCESS: Verification passed for the valid credential.");
    Console.WriteLine($"Key Binding Verified: {result.KeyBindingVerified}");
    Console.WriteLine($"Verified Issuer: {result.SdJwtVcPayload.Issuer}");
    Console.WriteLine($"Credential Type: {result.VerifiableCredentialType}");
    Console.WriteLine("Verified Credential Subject (rehydrated):");
    foreach (var (key, value) in result.SdJwtVcPayload.AdditionalData ?? new Dictionary<string, object>())
    {
        Console.WriteLine($"  - {key}: {JsonSerializer.Serialize(value)}");
    }
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nFAILURE: Verification unexpectedly failed: {ex.Message}");
    Console.ResetColor();
}


Console.WriteLine("\n--- CORE SD-JWT DEMONSTRATION ---");

// --- 6. CORE SD-JWT DEMONSTRATION ---
Console.WriteLine("\n--- Core SD-JWT Demo (Non-VC) ---");

var coreIssuer = new SdIssuer(issuerSigningKey, issuerSigningAlgorithm);
var coreClaims = new JwtPayload
{
    { "iss", issuerName },
    { "sub", "user_42" },
    { "given_name", "John" },
    { "family_name", "Doe" },
    { "email", "john.doe@example.com" },
    { "address", new { street = "123 Main St", city = "Anytown", state = "TX" } }
};

var coreOptions = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        family_name = true,
        email = true,
        address = new { city = true, state = true }
    }
};

var coreIssuerOutput = coreIssuer.Issue(coreClaims, coreOptions, holderPublicJwk);
var coreHolder = new SdJwtHolder(coreIssuerOutput.Issuance);
var corePresentation = coreHolder.CreatePresentation(
    disclosure => disclosure.ClaimName == "email" || disclosure.ClaimName == "city",
    new JwtPayload { { "aud", verifierAudience }, { "nonce", "core-demo-nonce" } },
    holderPrivateKey,
    holderSigningAlgorithm
);

var coreVerifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(issuerSigningKey));
var coreValidationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = issuerName,
    ValidateAudience = false,
    ValidateLifetime = true
};

var coreResult = await coreVerifier.VerifyAsync(corePresentation, coreValidationParams, kbJwtValidationParams);
Console.WriteLine($"Core SD-JWT verified successfully. Key binding: {coreResult.KeyBindingVerified}");
Console.WriteLine("Disclosed claims:");
foreach (var claim in coreResult.ClaimsPrincipal.Claims.Where(c => !c.Type.StartsWith("_sd") && c.Type != "cnf"))
{
    Console.WriteLine($"  - {claim.Type}: {claim.Value}");
}

// Show confirmation claim separately without exposing private key
var cnfClaim = coreResult.ClaimsPrincipal.FindFirst("cnf")?.Value;
if (!string.IsNullOrEmpty(cnfClaim))
{
    Console.WriteLine("  - cnf: [Key binding configuration present - private key details hidden for security]");
}

try
{
    var sampleFile = SdJwtParser.ParseJsonFile<SampleIssuanceFile>("sample-issuance.json");
    if (sampleFile != null)
    {
        Console.WriteLine("\nSuccessfully parsed sample JSON file.");
        var parsedIssuance = SdJwtParser.ParseIssuance(sampleFile.SdJwtVc);
        Console.WriteLine($"Parsed SD-JWT. Found {parsedIssuance.Disclosures.Count} disclosures.");
        Console.WriteLine("Unverified SD-JWT Payload:");
        Console.WriteLine(JsonSerializer.Serialize(parsedIssuance.UnverifiedSdJwt.Payload, new JsonSerializerOptions { WriteIndented = true }));
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nFailed to parse sample file: {ex.Message}");
    Console.ResetColor();
}

































































































































































































































































