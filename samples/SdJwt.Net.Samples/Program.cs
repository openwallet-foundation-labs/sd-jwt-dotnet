using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RichardSzalay.MockHttp;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Utils;
using SdJwt.Net.Verifier;
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

// The Issuer uses an ECDSA key (ES256), which is compatible with .NET Standard 2.0.
var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerSigningKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-key-1" };
const string issuerSigningAlgorithm = SecurityAlgorithms.EcdsaSha256;

// The Holder also uses an ECDSA key pair to sign the Key Binding JWT.
using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };


var holderPublicKey = new ECDsaSecurityKey(ECDsa.Create(holderEcdsa.ExportParameters(false))) { KeyId = "holder-key-1" };
var holderPublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);
const string holderSigningAlgorithm = SecurityAlgorithms.EcdsaSha256;

// Common constants
const string issuerName = "https://issuer.example.com";
const string verifierAudience = "https://verifier.example.com";
const string statusListUrl = "https://issuer.example.com/status/1";

// Create a Status List where the credential at index 42 is REVOKED.
var statusListManager = new StatusListManager(issuerSigningKey, issuerSigningAlgorithm);
var statusBits = new BitArray(100, false); // A list of 100 statuses, all initially valid (false).
statusBits[42] = true; // Set the status at index 42 to true (revoked).
var statusListCredentialJwt = statusListManager.CreateStatusListCredential(issuerName, statusBits);

// Mock the HTTP endpoint that serves the status list.
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
var vcPayload = new VerifiableCredentialPayload
{
    Issuer = issuerName,
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Status = new StatusClaim(statusListUrl, 42), // This VC's status is at index 42, which we know is revoked.
    CredentialSubject = new Dictionary<string, object>
    {
        { "given_name", "Jane" }, { "family_name", "Doe" }, { "email", "janedoe@example.com" }
    }
};
var issuanceOptions = new SdIssuanceOptions { DisclosureStructure = new { vc = new { credential_subject = new { email = true } } } };
var issuerOutput = vcIssuer.Issue(vcPayload, "VerifiedEmployee", issuanceOptions, holderPublicJwk);
Console.WriteLine($"Issuer created a VC of type 'VerifiedEmployee', linked to status list index 42.");


// --- 3. HOLDER: Creates a presentation ---
Console.WriteLine("\n--- Holder's Turn ---");
var holder = new SdJwtHolder(issuerOutput.Issuance, loggerFactory.CreateLogger<SdJwtHolder>());
var presentation = holder.CreatePresentation(
    disclosureSelector: disclosure => disclosure.ClaimValue.ToString()!.Contains("email"),
    kbJwtPayload: new JwtPayload { { "aud", verifierAudience }, { "nonce", "123-abc-456" } },
    kbJwtSigningKey: holderPrivateKey,
    kbJwtSigningAlgorithm: holderSigningAlgorithm
);
Console.WriteLine("Holder created a presentation disclosing only 'email'.");


// --- 4. VERIFIER: Verifies the (revoked) presentation ---
Console.WriteLine("\n--- Verifier's Turn ---");
var issuerKeyProvider = (JwtSecurityToken token) => Task.FromResult<SecurityKey>(issuerSigningKey);
var vcVerifier = new SdJwtVcVerifier(
    issuerKeyProvider,
    new StatusListOptions { HttpClient = httpClient, CacheDuration = TimeSpan.Zero }, // Disable cache for demo
    loggerFactory.CreateLogger<SdJwtVcVerifier>()
);
var issuerValidationParams = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false, ValidateLifetime = false };
var kbValidationParams = new TokenValidationParameters { ValidAudience = verifierAudience, ValidateAudience = true, IssuerSigningKey = holderPublicKey };

Console.WriteLine("\nAttempting to verify a REVOKED credential...");
try
{
    await vcVerifier.VerifyAsync(presentation, issuerValidationParams, kbValidationParams);
}
catch (SecurityTokenException ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\nSUCCESS: Verification correctly failed as expected.");
    Console.WriteLine($"Reason: {ex.Message}");
    ex.Message.Should().Contain("status is marked as invalid/revoked");
    Console.ResetColor();
}


// --- 5. VERIFIER: Verifies a VALID presentation ---
Console.WriteLine("\nAttempting to verify a VALID credential...");
vcPayload.Status!.StatusListIndex = 10; // Change status to a valid index (10)
var validIssuerOutput = vcIssuer.Issue(vcPayload, "VerifiedEmployee", issuanceOptions, holderPublicJwk);
var validHolder = new SdJwtHolder(validIssuerOutput.Issuance);
var validPresentation = validHolder.CreatePresentation(
    disclosureSelector: disclosure => disclosure.ClaimValue.ToString()!.Contains("email"),
    kbJwtPayload: new JwtPayload { { "aud", verifierAudience }, { "nonce", "789-def-012" } },
    kbJwtSigningKey: holderPrivateKey,
    kbJwtSigningAlgorithm: holderSigningAlgorithm
);

try
{
    var result = await vcVerifier.VerifyAsync(validPresentation, issuerValidationParams, kbValidationParams);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\nSUCCESS: Verification passed for the valid credential.");
    Console.WriteLine($"Key Binding Verified: {result.KeyBindingVerified}");
    Console.WriteLine($"Verified Issuer: {result.VerifiableCredential.Issuer}");
    Console.WriteLine("Verified Credential Subject (rehydrated):");
    foreach (var (key, value) in result.VerifiableCredential.CredentialSubject)
    {
        Console.WriteLine($"  - {key}: {System.Text.Json.JsonSerializer.Serialize(value)}");
    }
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nFAILURE: Verification unexpectedly failed: {ex.Message}");
    Console.ResetColor();
}

