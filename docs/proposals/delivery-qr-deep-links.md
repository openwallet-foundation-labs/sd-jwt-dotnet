# Implementation Plan: OID4VP Delivery via QR Codes and Deep Links

|                   |                                 |
| ----------------- | ------------------------------- |
| **Status**        | Validated implementation plan   |
| **Author**        | SD-JWT .NET Team                |
| **Created**       | 2026-03-04                      |
| **Reviewed**      | 2026-05-09                      |
| **Package**       | `SdJwt.Net.Oid4Vp` (extension)  |
| **Specification** | OpenID4VP 1.0 request URI flows |

---

## Context / Problem statement

OpenID4VP 1.0 defines same-device and cross-device flows, supports `request_uri`, and recommends `direct_post` with `request_uri` when an Authorization Request is passed across devices by QR code. In practice, verifiers still need helper APIs to produce transport-safe payloads:

- **QR codes** for cross-device flows (e.g., kiosk, point-of-sale, print media)
- **Deep links** for same-device flows (e.g., web page redirect, push notification)
- **Universal links** for platform-native invocation (iOS Universal Links, Android App Links)

`SdJwt.Net.Oid4Vp` already has low-level URI helpers for authorization requests and request-by-reference. The remaining gap is a supported transport helper that chooses request-by-value or request-by-reference, enforces payload limits, models expiry/single-use request storage, and produces QR/deep-link payloads without adding an image-rendering dependency.

---

## Goals

1. Generate QR code payloads from OID4VP authorization requests
2. Generate deep link / universal link URIs for same-device flows
3. Support both request-by-value and request-by-reference (`request_uri`)
4. Provide configurable QR rendering (format, size, error correction)
5. Handle request size limits (QR capacity ~4,296 alphanumeric characters)

## Non-Goals

- QR code image rendering (delegate to existing libraries like `QRCoder`)
- Push notification delivery (out of scope)
- Wallet-side QR scanning (wallet application responsibility)

---

## Direction

- Keep the output as URI payloads, not QR bitmap images.
- Use `openid4vp://` for custom-scheme payloads. Do not introduce non-standard schemes.
- Prefer request-by-reference for QR payloads because request objects can exceed QR capacity.
- Support `request_uri_method` values defined by OpenID4VP: `get` and `post`.
- Make request references short lived and optionally single use.

---

## Implementation plan

### Architecture

```mermaid
flowchart LR
    subgraph Verifier["Verifier Service"]
        AuthzReq["Authorization Request<br/>(existing)"]
        Transport["Transport Builder<br/>(new)"]
    end

    Transport --> QR["QR Payload Generator"]
    Transport --> DL["Deep Link Generator"]
    Transport --> UL["Universal Link Generator"]
    Transport --> ReqRef["Request Object Store"]

    QR --> QRLib["QR Rendering Library<br/>(external)"]

    AuthzReq --> Transport
```

### Component design

#### `Oid4VpTransportBuilder`

Fluent API for generating transport-ready payloads on top of existing request URI helpers:

```csharp
public sealed class Oid4VpTransportBuilder
{
    public Oid4VpTransportBuilder WithAuthorizationRequest(AuthorizationRequest request);
    public Oid4VpTransportBuilder WithRequestUri(string requestUri);
    public Oid4VpTransportBuilder WithRequestUriMethod(string requestUriMethod);
    public QrPayload BuildQrPayload(QrPayloadOptions options);
    public DeepLinkPayload BuildDeepLink(DeepLinkOptions options);
    public UniversalLinkPayload BuildUniversalLink(UniversalLinkOptions options);
}
```

#### `QrPayloadOptions`

```csharp
public class QrPayloadOptions
{
    public QrContentMode ContentMode { get; set; } = QrContentMode.RequestByReference;
    public int MaxPayloadSize { get; set; } = 4096;
    public TimeSpan RequestUriExpiry { get; set; } = TimeSpan.FromMinutes(5);
    public bool SingleUseRequestUri { get; set; } = true;
}

public enum QrContentMode
{
    RequestByValue,     // Full request in QR (small requests only)
    RequestByReference  // request_uri in QR, full request at URI
}
```

### Sequence: cross-device QR flow

```mermaid
sequenceDiagram
    participant Verifier
    participant ReqStore as Request Store
    participant QR as QR Display
    participant Wallet

    Verifier->>ReqStore: Store authorization request
    ReqStore-->>Verifier: request_uri
    Verifier->>QR: Render QR with openid4vp://...?request_uri=...
    Wallet->>QR: Scan QR code
    Wallet->>ReqStore: GET request_uri
    ReqStore-->>Wallet: Full authorization request
    Wallet->>Verifier: VP Token response
```

### Sequence: same-device deep link flow

```mermaid
sequenceDiagram
    participant Browser
    participant Verifier
    participant Wallet

    Browser->>Verifier: Start verification
    Verifier-->>Browser: Deep link redirect
    Browser->>Wallet: Open via deep link (openid4vp://...)
    Wallet->>Wallet: Process request, user consent
    Wallet->>Verifier: VP Token response (redirect back)
```

---

## API surface

```csharp
// Generate QR payload for cross-device
var transport = new Oid4VpTransportBuilder()
    .WithAuthorizationRequest(authzRequest)
    .WithRequestUri("https://verifier.example.com/requests/" + requestId);

var qrPayload = transport.BuildQrPayload(new QrPayloadOptions
{
    ContentMode = QrContentMode.RequestByReference
});

// qrPayload.Content = "openid4vp://?request_uri=https%3A%2F%2F..."
// qrPayload.ContentLength = 156
// Pass qrPayload.Content to QR rendering library

// Generate deep link for same-device
var deepLink = transport.BuildDeepLink(new DeepLinkOptions
{
    Scheme = "openid4vp",
    FallbackUrl = "https://wallet.example.com/download"
});

// deepLink.Uri = "openid4vp://?request=eyJ..."
```

---

## Security considerations

| Concern                   | Mitigation                                                   |
| ------------------------- | ------------------------------------------------------------ |
| QR code screenshot replay | Request URI with short TTL (5 min default) + single-use flag |
| Request tampering         | JAR (JWT Authorization Request) with signed payload          |
| Phishing via malicious QR | Wallet validates issuer identity before displaying consent   |
| Deep link hijacking       | Universal links with domain verification                     |

---

## Estimated effort

| Component                                             | Effort     |
| ----------------------------------------------------- | ---------- |
| Component                                             | Effort     |
| ---------------------------------------------------   | ---------- |
| `Oid4VpTransportBuilder`                              | 2 days     |
| `QrPayload`, `DeepLinkPayload`, universal link models | 1 day      |
| `IRequestObjectStore` with expiry and single-use API  | 2 days     |
| Payload size policy and request-by-reference fallback | 1 day      |
| Tests + documentation                                 | 2 days     |
| **Total**                                             | **8 days** |

---

## Alternatives considered

| Alternative                          | Rejected Because                                                                                       |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------ |
| Bundle QR rendering into the package | Adds image processing dependency; better to output payloads and let consumers choose rendering library |
| Custom URI scheme per implementation | Non-standard; use `openid4vp://` unless a profile explicitly defines another scheme                    |
| WebSocket-based delivery             | Over-engineered for most use cases; QR + deep links cover 95% of scenarios                             |

---

## Related documentation

- [OpenID4VP](../concepts/openid4vp.md) - Underlying verification protocol
- [DC API](../concepts/dc-api.md) - Browser-based alternative transport
- [Capability Matrix](../reference/capabilities.md) - Ecosystem feature coverage
