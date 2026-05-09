# SdJwt.Net.Wallet

Reference holder-side wallet infrastructure for .NET.

## Overview

This package provides reference wallet infrastructure for credential storage, key abstraction, format plugins, and issuance/presentation flow orchestration. It is intended for samples, prototypes, interoperability testing, and wallet-framework builders.

`SdJwt.Net.Wallet` is not a standalone consumer wallet application, not a certified wallet product, and not a replacement for production key management, secure storage, platform attestation, or ecosystem onboarding.

## Features

### Core Capabilities

-   **Credential Management**: Store, retrieve, filter, and present credentials
-   **Key Management**: Generate, sign, and manage cryptographic keys with high-assurance policy hooks
-   **Format Plugins**: Extensible credential format handling (SD-JWT VC, mdoc)
-   **Protocol Adapters**: OpenID4VCI and OpenID4VP support

### Advanced Features

-   Batch credential policy support (`OneTimeUse`, `RotateUse`)
-   Deferred credential polling for OID4VCI
-   DPoP proof hooks for issuance token exchange (`IDPoPProofProvider`)
-   Wallet/key attestation hooks (WIA/WUA) via `IWalletAttestationsProvider`
-   Transaction logging hooks via `ITransactionLogger`
-   Live document status resolution hook (`IDocumentStatusResolver`)
-   Multi-issuer configuration registry for OID4VCI
-   Resumable issuance sessions
-   Presentation session abstractions for remote and proximity flows

### Planned Extensions

-   Full proximity transport handling (BLE/NFC/QR handover)
-   Per-issuer DPoP policy profile
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
using SdJwt.Net.Wallet.Issuance;
using SdJwt.Net.Wallet.Status;

ICredentialStore store = new InMemoryCredentialStore();
IKeyManager keyManager = /* your IKeyManager implementation */;
IOid4VciAdapter oid4VciAdapter = /* your OID4VCI adapter */;
IOid4VpAdapter oid4VpAdapter = /* your OID4VP adapter */;
IWalletAttestationsProvider attestationProvider = /* optional */;
ITransactionLogger transactionLogger = /* optional */;
IDPoPProofProvider dpopProofProvider = /* optional */;
IDocumentStatusResolver statusResolver = /* optional */;

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
        TransactionLogger = transactionLogger,
        DPoPProofProvider = dpopProofProvider,
        DocumentStatusResolver = statusResolver
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

// Optional status resolver path
var status = await wallet.CheckStatusAsync("credential-id");

// Optional multi-issuer registration and resumable issuance
await wallet.RegisterIssuerConfigurationAsync(new WalletIssuerConfiguration
{
    IssuerName = "issuer-a",
    CredentialIssuer = "https://issuer-a.example.com"
});
var pending = await wallet.StartIssuanceSessionAsync(offer);
var resumed = await wallet.ResumeIssuanceSessionAsync(pending.SessionId);
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
    IDPoPProofProvider.cs
    DPoPProofRequest.cs
  Storage/                 # Storage abstractions
    ICredentialStore.cs
  Issuance/                # Issuer registry and pending issuance sessions
    WalletIssuerConfiguration.cs
    PendingIssuanceSession.cs
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
  Status/                  # Live status resolution abstractions
    IDocumentStatusResolver.cs
    DocumentStatus.cs
    DocumentStatusResult.cs
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

-   [OpenID4VCI Deep Dive](../../docs/concepts/openid4vci-deep-dive.md)
-   [OpenID4VP Deep Dive](../../docs/concepts/openid4vp-deep-dive.md)
-   [Enterprise Roadmap](../../docs/ENTERPRISE_ROADMAP.md)

## License

Apache 2.0
