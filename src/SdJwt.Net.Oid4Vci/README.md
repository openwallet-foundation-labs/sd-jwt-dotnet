# SdJwt.Net.Oid4Vci - OpenID for Verifiable Credential Issuance

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Oid4Vci.svg)](https://www.nuget.org/packages/SdJwt.Net.Oid4Vci/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OpenID4VCI 1.0** models and helpers for standardized credential issuance flows.

## Features

-   **OID4VCI 1.0 Models**: Offer, token, request, and response payloads
-   **Grant Flow Support**: Authorization Code and Pre-Authorized Code patterns
-   **Proof Support**: Single-proof and multi-proof request models
-   **Format Metadata**: Typed configuration models for `dc+sd-jwt`, `mso_mdoc`, `jwt_vc_json`, `jwt_vc_json-ld`, and `ldp_vc`
-   **Request/Response Encryption Metadata**: Issuer metadata for credential request and response encryption capabilities
-   **Transport-Agnostic Design**: Works with ASP.NET Core, worker services, and custom gateways
-   **Deferred Issuance Support**: Acceptance-token response model support

## Installation

```bash
dotnet add package SdJwt.Net.Oid4Vci
```

## Quick Start

### Build a Credential Offer

```csharp
using SdJwt.Net.Oid4Vci.Models;
using System.Text.Json;

var credentialOffer = new CredentialOffer
{
    CredentialIssuer = "https://issuer.example.com",
    CredentialConfigurationIds = new[] { "university_degree" }
};

credentialOffer.AddPreAuthorizedCodeGrant(
    preAuthorizedCode: "pre-auth-code-123");

var qrUrl =
    $"openid-credential-offer://?credential_offer={Uri.EscapeDataString(JsonSerializer.Serialize(credentialOffer))}";
```

### Build a Credential Request

```csharp
using SdJwt.Net.Oid4Vci.Models;

var credentialRequest = CredentialRequest.Create(
    vct: "https://issuer.example.com/credentials/university-degree",
    proofJwt: holderProofJwt);

credentialRequest.Validate();
```

### Advertise Supported Formats

```csharp
using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.Oid4Vci.Models.Formats;

var metadata = new CredentialIssuerMetadata
{
    CredentialIssuer = "https://issuer.example.com",
    CredentialConfigurationsSupported = new Dictionary<string, CredentialConfiguration>
    {
        ["employee_sd_jwt"] = new SdJwtVcCredentialConfiguration
        {
            Vct = "https://issuer.example.com/credentials/employee"
        },
        ["employee_jwt_vc_json_ld"] = new JwtVcJsonLdCredentialConfiguration
        {
            CredentialDefinition = new JwtVcJsonLdCredentialDefinition
            {
                Context = new[] { "https://www.w3.org/ns/credentials/v2" },
                Type = new[] { "VerifiableCredential", "EmployeeCredential" }
            }
        }
    }
};
```

### Build a Credential Response

```csharp
using SdJwt.Net.Oid4Vci.Models;

var credentialResponse = CredentialResponse.Success(
    credential: issuedCredential,
    format: Oid4VciConstants.SdJwtVcFormat);
```

## Common Use Cases

-   **University issuance** with pre-authorized codes
-   **Government onboarding** with OAuth authorization code flow
-   **Enterprise onboarding** with batch and deferred issuance
-   **Wallet interoperability** through standard OpenID4VCI payloads for SD-JWT VC, W3C VCDM, and ISO mdoc formats

## Test Notes

The test projects target `net10.0`. On Windows worktrees with restrictive inherited ACLs, use a serialized test run to avoid MSBuild worker buildup:

```pwsh
$env:MSBUILDDISABLENODEREUSE = "1"
dotnet test tests/SdJwt.Net.Oid4Vci.Tests/SdJwt.Net.Oid4Vci.Tests.csproj -f net10.0 -m:1 -nr:false -p:UseSharedCompilation=false
```

## Documentation

For full end-to-end implementation, see:

-   [Issuance guide](../../docs/guides/issuing-credentials.md)
-   [Sample app](../../samples/SdJwt.Net.Samples/README.md)

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
