# How to combine Status Lists and Introspection

| Field                | Value                                                           |
| -------------------- | --------------------------------------------------------------- |
| **Package maturity** | Spec-tracking (Token Status List draft-20)                      |
| **Code status**      | Runnable package APIs with illustrative hybrid-checker wiring   |
| **Related concept**  | [Verifiable Credentials](../concepts/verifiable-credentials.md) |

> **Privacy trade-off:** Token Introspection (RFC 7662) contacts the issuer in real time. This is not privacy-preserving: the issuer can observe which credentials are being verified and when. Status Lists avoid this by publishing a cached bitstring. Use introspection only when immediate revocation propagation outweighs privacy considerations, and prefer Status Lists as the primary mechanism.

|                      |                                                                                                                                                                                                                                                    |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers implementing real-time credential verification alongside privacy-preserving status lists.                                                                                                                                               |
| **Purpose**          | Walk through Token Introspection (RFC 7662) - configuring the client, checking credential status, hybrid strategies, and authentication - using `SdJwt.Net.StatusList`.                                                                            |
| **Scope**            | Introspection client setup, status checking, hybrid checker strategies (StatusListFirst, BothMustPass, etc.), authentication methods, and error handling. Out of scope: status list internals (see [Managing Revocation](managing-revocation.md)). |
| **Success criteria** | Reader can configure a token introspection client, implement hybrid status checking with fallback strategies, and handle introspection errors with proper circuit-breaker patterns.                                                                |

## What your application still owns

This guide does not provide: introspection endpoint implementation (issuer-side), production authentication secrets, circuit breaker and retry tuning, privacy impact assessments, or rate limit planning for high-volume scenarios.

---

## Key decisions

| Decision                      | Options                            | Guidance                          |
| ----------------------------- | ---------------------------------- | --------------------------------- |
| Checking strategy?            | Status List, Introspection, Hybrid | Hybrid for balanced coverage      |
| Introspection authentication? | Client credentials, Bearer token   | Client credentials for M2M        |
| Caching strategy?             | Response-level, None               | Short TTL caching for high-volume |
| Fallback behavior?            | Fail-open, Fail-closed             | Depends on risk tolerance         |
| Use hybrid status checking?   | Yes/No                             | Yes for high-value credentials    |

---

## Prerequisites

```bash
dotnet add package SdJwt.Net.StatusList
```

## Token introspection overview

Token Introspection (RFC 7662) lets a verifier query an authorization server in real time to determine the state of a credential. Use it when:

- Immediate revocation propagation is required
- The credential doesn't include status list references
- High-value transactions require additional assurance

## 1. Configure the token introspection client

The `TokenIntrospectionClient` handles communication with OAuth 2.0 introspection endpoints:

```csharp
using SdJwt.Net.StatusList.Introspection;

var options = new TokenIntrospectionOptions
{
    IntrospectionEndpoint = "https://issuer.example.com/oauth/introspect",
    ClientId = "verifier-client-id",
    ClientSecret = "verifier-client-secret"
};

var httpClient = new HttpClient();
var introspectionClient = new TokenIntrospectionClient(httpClient, options);
```

## 2. Check credential status

```csharp
// Check status of a credential
var response = await introspectionClient.IntrospectAsync(credentialToken);

if (response.Active)
{
    Console.WriteLine($"Credential is active");
    Console.WriteLine($"Subject: {response.Subject}");
    Console.WriteLine($"Expires: {response.ExpirationTime}");
}
else
{
    Console.WriteLine("Credential is inactive or revoked");
}
```

## 3. Advanced: hybrid status checking

The `HybridStatusChecker` combines Status List and Token Introspection, allowing flexible verification strategies:

```csharp
using SdJwt.Net.StatusList.Services;

// Create the hybrid checker with both clients
var hybridChecker = new HybridStatusChecker(
    statusListClient,
    introspectionClient,
    preferredStrategy: HybridCheckStrategy.StatusListFirst
);

// Check status using the configured strategy
var result = await hybridChecker.CheckStatusAsync(credential);

if (result.IsActive)
{
    Console.WriteLine($"Credential verified via: {result.VerificationMethod}");
}
```

## Available strategies

| Strategy             | Description                                       |
| -------------------- | ------------------------------------------------- |
| `StatusListOnly`     | Only use Status List (privacy-preserving, cached) |
| `IntrospectionOnly`  | Only use Introspection (real-time)                |
| `StatusListFirst`    | Try Status List, fall back to Introspection       |
| `IntrospectionFirst` | Try Introspection, fall back to Status List       |
| `BothMustPass`       | Both methods must confirm active status           |

## 4. Handling authentication

Token Introspection endpoints typically require authentication. The client supports two methods.

### Client credentials

```csharp
var options = new TokenIntrospectionOptions
{
    IntrospectionEndpoint = "https://issuer.example.com/oauth/introspect",
    ClientId = "verifier-client-id",
    ClientSecret = "verifier-client-secret",
    AuthenticationMethod = ClientAuthenticationMethod.ClientSecretPost
};
```

### Bearer token

```csharp
var options = new TokenIntrospectionOptions
{
    IntrospectionEndpoint = "https://issuer.example.com/oauth/introspect",
    AccessToken = "bearer-token-from-authorization"
};
```

## 5. Error handling

```csharp
try
{
    var response = await introspectionClient.IntrospectAsync(token);

    if (response.Active)
    {
        // Credential is valid
    }
}
catch (TokenIntrospectionException ex)
{
    // Handle introspection errors
    Console.WriteLine($"Introspection failed: {ex.Message}");

    // Consider fallback strategy
    if (useFallback)
    {
        var fallbackResult = await statusListClient.CheckStatusAsync(credential);
    }
}
```

## Best practices

- Use hybrid checking for high-value credentials that require both real-time verification and a privacy-preserving fallback.
- Cache introspection responses with short TTLs to reduce load on the authorization server while maintaining freshness.
- Implement circuit breakers to handle introspection service unavailability gracefully.
- Log which verification method confirmed the credential status, for audit trails.
- Introspection endpoints may have rate limits; use Status Lists for high-volume scenarios.

## RFC 7662 compliance

The implementation follows RFC 7662 (OAuth 2.0 Token Introspection) with support for:

- Standard request/response format
- Client authentication methods (client_secret_post, client_secret_basic)
- Token type hints
- Extension parameters

---

## See also

- [Managing Revocation with Status Lists](managing-revocation.md)
- [RFC 7662 - OAuth 2.0 Token Introspection](https://tools.ietf.org/html/rfc7662)
- [Token Status List Specification](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/)
