# SdJwt.Net.StatusList - Status List Management

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.StatusList.svg)](https://www.nuget.org/packages/SdJwt.Net.StatusList/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of OAuth Status List token creation and verification for revocation and suspension workflows.

## Features

- **Status List Token Issuance**: Create signed `statuslist+jwt` tokens
- **Verifier Support**: Fetch, validate, and evaluate status list entries
- **Multi-bit Status Values**: 1, 2, 4, or 8 bits per credential entry
- **Caching and Retry Options**: HTTP retrieval and cache controls in verifier options
- **Operational Helpers**: APIs for revoke, suspend, reinstate, and aggregation

## Installation

```bash
dotnet add package SdJwt.Net.StatusList
```

## Quick Start

### Create a Status List Token

```csharp
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;

var statusManager = new StatusListManager(statusKey, SecurityAlgorithms.EcdsaSha256);

// 0 = valid, 1 = invalid, 2 = suspended (using 2 bits)
var statusValues = new byte[]
{
    (byte)StatusType.Valid,
    (byte)StatusType.Invalid,
    (byte)StatusType.Suspended
};

var statusListToken = await statusManager.CreateStatusListTokenAsync(
    subject: "https://issuer.example.com/status/1",
    statusValues: statusValues,
    bits: 2);
```

### Check Credential Status

```csharp
using SdJwt.Net.StatusList.Verifier;
using SdJwt.Net.StatusList.Models;

var statusVerifier = new StatusListVerifier(httpClient);

var statusClaim = new StatusClaim
{
    StatusList = new StatusListReference
    {
        Index = 1,
        Uri = "https://issuer.example.com/status/1"
    }
};

var statusResult = await statusVerifier.CheckStatusAsync(
    statusClaim,
    issuer => ResolveStatusIssuerKeyAsync(issuer));

if (statusResult.IsValid)
{
    Console.WriteLine("Credential status is valid.");
}
else
{
    Console.WriteLine($"Credential status is {statusResult.Status} ({statusResult.StatusValue}).");
}
```

## Common Use Cases

- **Revocation**: Mark credentials as invalid when compromised
- **Temporary Suspension**: Pause credentials during investigations
- **High-volume Checking**: Cache and validate status lists in verifier gateways
- **Lifecycle Governance**: Track status transitions as part of audit and compliance

## Documentation

For complete workflows, see:

- [Revocation guide](../../docs/guides/managing-revocation.md)
- [Architecture deep dive](../../docs/concepts/architecture.md)

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
