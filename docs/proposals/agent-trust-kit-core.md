# Agent Trust Kit — Core Library Proposal

## Document Information

| Field      | Value                                                                        |
| ---------- | ---------------------------------------------------------------------------- |
| Version    | 1.0.0                                                                        |
| Package    | `SdJwt.Net.AgentTrust.Core`                                                  |
| Status     | Draft Proposal                                                               |
| Created    | 2026-03-01                                                                   |
| Depends On | `SdJwt.Net` (core SD-JWT engine)                                             |
| Related    | [Overview](agent-trust-kit-overview.md), [Policy](agent-trust-kit-policy.md) |

---

## Purpose

`SdJwt.Net.AgentTrust.Core` provides the foundational capability token issuance and verification engine for machine-to-machine agent interactions. It wraps the existing `SdIssuer`/`SdVerifier` with a capability-specific claim profile, key custody abstraction, nonce/replay prevention, and audit receipt generation.

---

## Design Justification

### Why a Separate Package (Not Extensions on `SdJwt.Net`)?

| Reason                      | Detail                                                                                |
| --------------------------- | ------------------------------------------------------------------------------------- |
| **Single Responsibility**   | Core SD-JWT implements RFC 9901; agent capability is a domain-specific profile on top |
| **No breaking changes**     | Existing 11 packages remain untouched; new package depends on core                    |
| **Independent versioning**  | AgentTrust can iterate faster than the standards-track core                           |
| **Clear adoption boundary** | Teams adopt AgentTrust only if they need M2M agent capabilities                       |

### What We Reuse From Core

| Capability Needed          | Reused From `SdJwt.Net`      | Evidence                                        |
| -------------------------- | ---------------------------- | ----------------------------------------------- |
| SD-JWT issuance + signing  | `SdIssuer.Issue()`           | Full RFC 9901 compliance, algorithm enforcement |
| SD-JWT verification        | `SdVerifier.VerifyAsync()`   | Signature, expiry, nonce validation             |
| Selective disclosure       | Disclosure structure options | Tool receives only required claims              |
| Key binding (`cnf`)        | Holder key binding support   | Proof-of-possession for sender constraint       |
| Hash algorithm enforcement | SHA-256/384/512 only         | MD5/SHA-1 blocked at core level                 |

---

## Component Design

### 1. Capability Claim Profile

Defines the SD-JWT claim structure for capability tokens:

```csharp
namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Represents the capability scope of an agent trust token.
/// This is the custom "cap" claim in the SD-JWT payload.
/// </summary>
public record CapabilityClaim
{
    /// <summary>
    /// Target tool identifier (e.g., "MemberLookup", "mcp://weather-service").
    /// </summary>
    public required string Tool { get; init; }

    /// <summary>
    /// Specific action within the tool (e.g., "GetFees", "Query").
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Optional resource scope (e.g., "member:12345", "region:APAC").
    /// </summary>
    public string? Resource { get; init; }

    /// <summary>
    /// Optional operation limits enforced by the receiver.
    /// </summary>
    public CapabilityLimits? Limits { get; init; }

    /// <summary>
    /// Optional description of the purpose for audit/transparency.
    /// </summary>
    public string? Purpose { get; init; }
}

/// <summary>
/// Machine-enforceable limits on a capability grant.
/// </summary>
public record CapabilityLimits
{
    /// <summary>
    /// Maximum number of results/records the tool should return.
    /// </summary>
    public int? MaxResults { get; init; }

    /// <summary>
    /// Maximum number of invocations allowed with this token.
    /// </summary>
    public int? MaxInvocations { get; init; }

    /// <summary>
    /// Maximum payload size in bytes.
    /// </summary>
    public int? MaxPayloadBytes { get; init; }
}
```

### 2. Capability Context

Provides correlation and traceability metadata:

```csharp
namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Execution context for capability token correlation and tracing.
/// This is the custom "ctx" claim in the SD-JWT payload.
/// </summary>
public record CapabilityContext
{
    /// <summary>
    /// Unique correlation identifier for the entire workflow execution.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Optional workflow identifier for multi-step orchestrations.
    /// </summary>
    public string? WorkflowId { get; init; }

    /// <summary>
    /// Optional step identifier within a workflow.
    /// </summary>
    public string? StepId { get; init; }

    /// <summary>
    /// Optional tenant identifier for multi-tenant environments.
    /// </summary>
    public string? TenantId { get; init; }
}
```

### 3. Capability Token Issuer

Wraps `SdIssuer` with capability-specific logic:

```csharp
namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Options for minting a capability token.
/// </summary>
public record CapabilityTokenOptions
{
    /// <summary>
    /// Issuer identity (agent service principal or identity).
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Target audience (tool or agent endpoint identifier).
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    /// Capability scope for this token.
    /// </summary>
    public required CapabilityClaim Capability { get; init; }

    /// <summary>
    /// Execution context for correlation.
    /// </summary>
    public required CapabilityContext Context { get; init; }

    /// <summary>
    /// Token lifetime. Defaults to 60 seconds.
    /// </summary>
    public TimeSpan Lifetime { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Claims to selectively disclose (all others are hidden).
    /// If null, all claims are disclosed by default.
    /// </summary>
    public IReadOnlyList<string>? DisclosableClaims { get; init; }
}

/// <summary>
/// Result of minting a capability token.
/// </summary>
public record CapabilityTokenResult
{
    /// <summary>
    /// The minted SD-JWT capability token string.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Unique token identifier (jti) for audit and replay tracking.
    /// </summary>
    public required string TokenId { get; init; }

    /// <summary>
    /// Token expiration time.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>
/// Mints SD-JWT capability tokens for agent-to-tool and agent-to-agent calls.
/// Delegates to <see cref="SdIssuer"/> for SD-JWT creation and signing.
/// </summary>
public class CapabilityTokenIssuer
{
    private readonly SdIssuer _sdIssuer;
    private readonly INonceStore _nonceStore;
    private readonly ILogger<CapabilityTokenIssuer> _logger;

    /// <summary>
    /// Initializes a new capability token issuer.
    /// </summary>
    /// <param name="signingKey">The security key to sign capability tokens.</param>
    /// <param name="signingAlgorithm">The signing algorithm (e.g., "ES256").</param>
    /// <param name="nonceStore">Store for tracking issued token IDs.</param>
    /// <param name="logger">Optional logger.</param>
    public CapabilityTokenIssuer(
        SecurityKey signingKey,
        string signingAlgorithm,
        INonceStore nonceStore,
        ILogger<CapabilityTokenIssuer>? logger = null);

    /// <summary>
    /// Mints a new capability token with the specified options.
    /// </summary>
    /// <param name="options">Token issuance options.</param>
    /// <returns>The minted capability token.</returns>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public CapabilityTokenResult Mint(CapabilityTokenOptions options);
}
```

### 4. Capability Token Verifier

Wraps `SdVerifier` with capability-specific validation:

```csharp
namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Options for verifying a capability token.
/// </summary>
public record CapabilityVerificationOptions
{
    /// <summary>
    /// Expected audience for this tool/agent endpoint.
    /// </summary>
    public required string ExpectedAudience { get; init; }

    /// <summary>
    /// Trusted issuer identities and their public keys.
    /// </summary>
    public required IReadOnlyDictionary<string, SecurityKey> TrustedIssuers { get; init; }

    /// <summary>
    /// Whether to enforce replay prevention via nonce store.
    /// Defaults to true.
    /// </summary>
    public bool EnforceReplayPrevention { get; init; } = true;

    /// <summary>
    /// Clock skew tolerance for expiry validation.
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; init; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Result of capability token verification.
/// </summary>
public record CapabilityVerificationResult
{
    public bool IsValid { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The verified capability claim, if valid.
    /// </summary>
    public CapabilityClaim? Capability { get; init; }

    /// <summary>
    /// The verified context, if valid.
    /// </summary>
    public CapabilityContext? Context { get; init; }

    /// <summary>
    /// The token ID (jti) for audit purposes.
    /// </summary>
    public string? TokenId { get; init; }

    /// <summary>
    /// The issuer identity.
    /// </summary>
    public string? Issuer { get; init; }

    public static CapabilityVerificationResult Success(
        CapabilityClaim capability,
        CapabilityContext context,
        string tokenId,
        string issuer);

    public static CapabilityVerificationResult Failure(string error, string errorCode);
}

/// <summary>
/// Verifies SD-JWT capability tokens and enforces constraints.
/// Delegates to <see cref="SdVerifier"/> for cryptographic verification.
/// </summary>
public class CapabilityTokenVerifier
{
    private readonly INonceStore _nonceStore;
    private readonly ILogger<CapabilityTokenVerifier> _logger;

    public CapabilityTokenVerifier(
        INonceStore nonceStore,
        ILogger<CapabilityTokenVerifier>? logger = null);

    /// <summary>
    /// Verifies a capability token and returns the validated claims.
    /// </summary>
    public Task<CapabilityVerificationResult> VerifyAsync(
        string token,
        CapabilityVerificationOptions options,
        CancellationToken cancellationToken = default);
}
```

### 5. Key Custody Provider

Abstraction for signing key management:

```csharp
namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Provides signing keys for capability token issuance.
/// Implementations may use local keys, HSM, Azure Key Vault, AWS KMS, etc.
/// </summary>
public interface IKeyCustodyProvider
{
    /// <summary>
    /// Retrieves the signing key for the specified agent identity.
    /// </summary>
    Task<SecurityKey> GetSigningKeyAsync(
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the signing algorithm for the specified agent identity.
    /// </summary>
    Task<string> GetSigningAlgorithmAsync(
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates the signing key for the specified agent identity.
    /// </summary>
    Task RotateKeyAsync(
        string agentId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default in-memory key custody provider for development and testing.
/// </summary>
public class InMemoryKeyCustodyProvider : IKeyCustodyProvider
{
    // Stores keys in-memory. NOT suitable for production.
}
```

### 6. Nonce/Replay Store

```csharp
namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Store for tracking token IDs to prevent replay attacks.
/// </summary>
public interface INonceStore
{
    /// <summary>
    /// Records a token ID as used. Returns false if already seen.
    /// </summary>
    Task<bool> TryMarkAsUsedAsync(
        string tokenId,
        DateTimeOffset expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a token ID has been previously used.
    /// </summary>
    Task<bool> IsUsedAsync(
        string tokenId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory nonce store using IMemoryCache.
/// Expired entries are automatically evicted.
/// </summary>
public class MemoryNonceStore : INonceStore
{
    public MemoryNonceStore(IMemoryCache cache);
}
```

### 7. Audit Receipt

```csharp
namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// An auditable receipt emitted after a capability token is evaluated.
/// </summary>
public record AuditReceipt
{
    /// <summary>
    /// The token ID (jti) of the capability token evaluated.
    /// </summary>
    public required string TokenId { get; init; }

    /// <summary>
    /// Timestamp of the evaluation.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// The decision: Allow or Deny.
    /// </summary>
    public required ReceiptDecision Decision { get; init; }

    /// <summary>
    /// The tool and action that were evaluated.
    /// </summary>
    public required string Tool { get; init; }
    public required string Action { get; init; }

    /// <summary>
    /// Correlation ID for workflow tracing.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Optional denial reason code when Decision is Deny.
    /// </summary>
    public string? DenyReason { get; init; }

    /// <summary>
    /// Evaluation duration in milliseconds.
    /// </summary>
    public long? DurationMs { get; init; }
}

public enum ReceiptDecision
{
    Allow,
    Deny
}

/// <summary>
/// Writes audit receipts to a configured sink.
/// </summary>
public interface IReceiptWriter
{
    /// <summary>
    /// Writes an audit receipt.
    /// </summary>
    Task WriteAsync(
        AuditReceipt receipt,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default receipt writer that uses ILogger to emit structured logs.
/// Production deployments should implement a durable sink (DB, event stream).
/// </summary>
public class LoggingReceiptWriter : IReceiptWriter
{
    public LoggingReceiptWriter(ILogger<LoggingReceiptWriter> logger);
}
```

---

## Workflow 0 — Bootstrap (Agent Identity + Key Setup)

### Steps

1. Provision **Agent Identity** (service principal, workload identity, managed identity).
2. Provision signing keys via `IKeyCustodyProvider` (local for dev, KMS/HSM for production).
3. Configure `CapabilityTokenIssuer` with:
   - Issuer ID (`iss`)
   - Allowed audiences (`aud`) mapped to tool/A2A endpoints
   - Default token lifetime
4. Configure `INonceStore` and `IReceiptWriter` sinks.
5. Register trusted issuers on receiver side via `CapabilityVerificationOptions.TrustedIssuers`.

### Outputs

- Agent can mint capability tokens for authorized tools only.
- Receiver knows which issuers to trust (static allowlist for PoC; later federation/trust registry).

---

## Workflow 5 — Containment (Revocation/Expiry/Rotation)

### Tier 1 — Expiry-Based Containment (PoC Default)

Capability tokens have short lifetimes (default 60 seconds). No infrastructure needed.

### Tier 2 — Deny List

`INonceStore` extended with a deny list for specific `jti` values. Immediate revocation of specific tokens.

### Tier 3 — Issuer Key Rollover

`IKeyCustodyProvider.RotateKeyAsync()` removes compromised keys from trust anchors across receivers.

### Tier 4 — Status List Integration (Future)

Leverage existing `SdJwt.Net.StatusList` for full credential lifecycle management when needed.

---

## NuGet Package Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0;net10.0;netstandard2.1</TargetFrameworks>
    <PackageId>SdJwt.Net.AgentTrust.Core</PackageId>
    <Description>Core capability token issuance and verification for machine-to-machine
        agent trust, built on SD-JWT (RFC 9901).</Description>
    <PackageTags>sd-jwt;agent-trust;capability;m2m;maf;mcp;rfc9901</PackageTags>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\SdJwt.Net\SdJwt.Net.csproj" />
    <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="9.0.6" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.6" />
  </ItemGroup>
</Project>
```

---

## Test Strategy

| Category           | Coverage Target | Examples                                               |
| ------------------ | --------------- | ------------------------------------------------------ |
| Token Minting      | 100%            | Valid mint, missing fields, expiry enforcement         |
| Token Verification | 100%            | Valid token, expired, wrong audience, replay detection |
| Capability Claims  | 100%            | All claim combinations, limits enforcement             |
| Nonce Store        | 100%            | Mark used, duplicate detection, expiry eviction        |
| Key Custody        | 90%             | In-memory provider, rotation, missing key              |
| Receipt Writer     | 100%            | Allow/deny receipts, structured log output             |

**Estimated test count:** 120-150 unit tests

---

## Estimated Effort

| Phase                | Duration    |
| -------------------- | ----------- |
| Core models + claims | 1 week      |
| Issuer + Verifier    | 1.5 weeks   |
| Key custody + nonce  | 1 week      |
| Receipt writer       | 0.5 weeks   |
| Testing + polish     | 1 week      |
| **Total**            | **5 weeks** |
