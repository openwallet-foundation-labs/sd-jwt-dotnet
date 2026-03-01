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

-   Batch credential policy support (`OneTimeUse`, `RotateUse`)
-   Deferred credential polling for OID4VCI
-   Wallet/key attestation hooks (WIA/WUA) via `IWalletAttestationsProvider`
-   Transaction logging hooks via `ITransactionLogger`
-   Presentation session abstractions for remote and proximity flows

### Planned Extensions

-   DPoP proof generation and per-issuer DPoP configuration
-   Full proximity transport handling (BLE/NFC/QR handover)
-   Multi-issuer configuration registry
-   DCQL support for OpenID4VP matching

## Installation

```bash
dotnet add package SdJwt.Net.Wallet
```

## Quick Start

```csharp
using SdJwt.Net.Wallet;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Storage;
using SdJwt.Net.Wallet.Protocols;
using SdJwt.Net.Wallet.Attestation;
using SdJwt.Net.Wallet.Audit;

ICredentialStore store = new InMemoryCredentialStore();
IKeyManager keyManager = /* your IKeyManager implementation */;
IOid4VciAdapter oid4VciAdapter = /* your OID4VCI adapter */;
IOid4VpAdapter oid4VpAdapter = /* your OID4VP adapter */;
IWalletAttestationsProvider attestationProvider = /* optional */;
ITransactionLogger transactionLogger = /* optional */;

var wallet = new GenericWallet(
    store,
    keyManager,
    options: new WalletOptions
    {
        WalletId = "my-wallet",
        DisplayName = "My Wallet",
        Oid4VciAdapter = oid4VciAdapter,
        Oid4VpAdapter = oid4VpAdapter,
        WalletAttestationsProvider = attestationProvider,
        TransactionLogger = transactionLogger
    });

// Process credential offer (OID4VCI)
var offer = await wallet.ProcessCredentialOfferAsync("openid-credential-offer://...");
var issuance = await wallet.AcceptCredentialOfferAsync(offer);

// Process presentation request (OID4VP)
var request = await wallet.ProcessPresentationRequestAsync("openid4vp://...");
var result = await wallet.CreateAndSubmitPresentationAsync(request);

// Optional attestation helpers
var wia = await wallet.GenerateWalletAttestationAsync("key-id");
var wua = await wallet.GenerateKeyAttestationAsync(new[] { "key-id" }, nonce: "issuer-nonce");

// Optional session abstraction
var remoteSession = wallet.CreateRemotePresentationSession();
```

## Architecture

```
SdJwt.Net.Wallet/
  Core/                    # Core models and abstractions
    CredentialModels.cs
    ICredentialManager.cs
    IKeyManager.cs
  WalletOptions.cs
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
    TransactionType.cs
    TransactionStatus.cs
    TransactionLog.cs
    ITransactionLogger.cs
  Sessions/                # Presentation session abstractions
    IPresentationSession.cs
    PresentationFlowType.cs
    RemotePresentationSession.cs
    ProximityPresentationSession.cs
  GenericWallet.cs
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
