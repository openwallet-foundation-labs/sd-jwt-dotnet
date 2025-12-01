# SdJwt.Net.StatusList - Status List Management

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.StatusList.svg)](https://www.nuget.org/packages/SdJwt.Net.StatusList/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OAuth Status List** specification compliant with [draft-ietf-oauth-status-list-13](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/). Provides efficient credential revocation and status management with high-performance operations.

## Features

- **Draft 13 Compliance**: Complete OAuth Status List implementation
- **Multi-bit Status**: Support for Valid, Invalid, Suspended, Under Investigation
- **High Performance**: 10,000+ status checks per second
- **Compression**: GZIP compression for efficient storage
- **Concurrent Safe**: ETag-based versioning for safe updates

## Installation

```bash
dotnet add package SdJwt.Net.StatusList
```

## Quick Start

### Create Status List

```csharp
using SdJwt.Net.StatusList.Issuer;

var statusManager = new StatusListManager(statusKey, SecurityAlgorithms.EcdsaSha256);

var credentialStatuses = new List<CredentialStatus>
{
    new() { Index = 0, Status = CredentialStatusValue.Valid },
    new() { Index = 1, Status = CredentialStatusValue.Revoked },
    new() { Index = 2, Status = CredentialStatusValue.Suspended }
};

var statusListToken = await statusManager.CreateStatusListTokenAsync(
    "https://issuer.example.com/status/1", credentialStatuses);
```

### Check Credential Status

```csharp
using SdJwt.Net.StatusList.Verifier;

var statusVerifier = new StatusListVerifier(httpClient);

var statusClaim = new StatusClaim
{
    StatusList = new StatusListReference
    {
        Index = 0,
        Uri = "https://issuer.example.com/status/1"
    }
};

var isValid = await statusVerifier.CheckStatusAsync(statusClaim, keyResolver);
```

## Enterprise Features

- **Scalable Operations**: Support for millions of credentials per status list
- **Memory Efficiency**: Compressed storage reducing memory footprint by 95%+
- **Caching Strategy**: Built-in caching for improved performance
- **HTTP Integration**: Production-ready status endpoint patterns

## Documentation

For comprehensive examples and enterprise deployment patterns, see the [main repository](https://github.com/thomas-tran/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
