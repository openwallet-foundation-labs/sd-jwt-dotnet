# SdJwt.Net.Wallet

Generic, extensible identity wallet implementation for .NET.

## Overview

This package provides a **generic wallet foundation** for building identity wallet applications, supporting multiple credential formats and protocols. The architecture is informed by production EUDI Android/iOS implementations and the OWF .NET ecosystem.

## Features

### Core Capabilities

-   **Credential Management**: Store, retrieve, filter, and present credentials
-   **Key Management**: Generate, sign, and manage cryptographic keys with HAIP compliance
-   **Format Plugins**: Extensible credential format handling (SD-JWT VC, mdoc)
-   **Protocol Adapters**: OpenID4VCI and OpenID4VP support

### Advanced Features

-   Batch credential issuance and policies
-   DPoP (Demonstrating Proof of Possession)
-   Wallet attestation (WIA/WUA)
-   Transaction logging for audit
-   Multi-issuer configuration

## Installation

```bash
dotnet add package SdJwt.Net.Wallet
```

## Quick Start

```csharp
using SdJwt.Net.Wallet;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Storage;

// Create wallet with file-based storage
var storage = new FileCredentialStore("./wallet-data");
var keyManager = new SoftwareKeyManager();
var credentialManager = new DefaultCredentialManager(storage);

var wallet = new GenericWallet(
    new WalletOptions { WalletId = "my-wallet" },
    credentialManager,
    keyManager);

// Process credential offer (OID4VCI)
var offer = await wallet.ProcessCredentialOfferAsync("openid-credential-offer://...");
var credential = await wallet.AcceptCredentialOfferAsync(offer);

// Process presentation request (OID4VP)
var request = await wallet.ProcessPresentationRequestAsync("openid4vp://...");
var result = await wallet.CreateAndSubmitPresentationAsync(request);
```

## Architecture

```
SdJwt.Net.Wallet/
  Core/                    # Core interfaces and implementations
    ICredentialManager.cs
    IKeyManager.cs
    IWallet.cs
  Formats/                 # Credential format plugins
    ICredentialFormatPlugin.cs
    SdJwtVcFormatPlugin.cs
  Protocols/               # Protocol adapters
    IOid4VciAdapter.cs
    IOid4VpAdapter.cs
  Storage/                 # Storage abstractions
    ICredentialStore.cs
  Attestation/             # Wallet attestation
    IWalletAttestationsProvider.cs
  Audit/                   # Transaction logging
    ITransactionLogger.cs
```

## Dependencies

This package builds on top of the SD-JWT .NET ecosystem:

| Package                        | Purpose                  |
| ------------------------------ | ------------------------ |
| SdJwt.Net                      | Core SD-JWT operations   |
| SdJwt.Net.Vc                   | Verifiable Credentials   |
| SdJwt.Net.Oid4Vci              | Credential issuance      |
| SdJwt.Net.Oid4Vp               | Presentation protocol    |
| SdJwt.Net.PresentationExchange | DIF PEX matching         |
| SdJwt.Net.HAIP                 | Cryptographic compliance |
| SdJwt.Net.StatusList           | Credential status        |

## Related Documentation

-   [Wallet Architecture Design](../../docs/proposals/wallet-architecture.md)
-   [OpenID4VCI Deep Dive](../../docs/concepts/openid4vci-deep-dive.md)
-   [OpenID4VP Deep Dive](../../docs/concepts/openid4vp-deep-dive.md)
-   [Enterprise Roadmap](../../docs/ENTERPRISE_ROADMAP.md)

## License

Apache 2.0
