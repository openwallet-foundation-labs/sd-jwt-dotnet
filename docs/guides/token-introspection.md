# How to Implement Real-Time Credential Status Checking

This guide demonstrates how to implement real-time credential status verification using Token Introspection (RFC 7662) alongside the privacy-preserving Status List approach.

While Status Lists provide excellent scalability and privacy, some use cases require immediate status updates. Token Introspection establishes a direct communication channel with the Issuer's authorization server for real-time status verification.

---

## Key Decisions

| Decision                      | Options                            | Guidance                          |
| ----------------------------- | ---------------------------------- | --------------------------------- |
| Checking strategy?            | Status List, Introspection, Hybrid | Hybrid provides best balance      |
| Introspection authentication? | Client credentials, Bearer token   | Client credentials for M2M        |
| Caching strategy?             | Response-level, None               | Short TTL caching for high-volume |
| Fallback behavior?            | Fail-open, Fail-closed             | Depends on risk tolerance         |
| Use hybrid status checking?   | Yes/No                             | Yes for high-value credentials    |

---

## Prerequisites

```bash
dotnet add package SdJwt.Net.StatusList
```

## Token Introspection Overview

Token Introspection (RFC 7662) allows a Verifier to query an authorization server in real-time to determine the state of a credential. This is useful when:

- Immediate revocation propagation is required
- The credential doesn't include status list references
- High-value transactions require additional assurance

## 1. Configure the Token Introspection Client

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

## 2. Check Credential Status

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

## 3. Advanced: Hybrid Status Checking

The `HybridStatusChecker` combines both Status List and Token Introspection approaches, allowing flexible verification strategies:

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

## Available Strategies

| Strategy             | Description                                       |
| -------------------- | ------------------------------------------------- |
| `StatusListOnly`     | Only use Status List (privacy-preserving, cached) |
| `IntrospectionOnly`  | Only use Introspection (real-time)                |
| `StatusListFirst`    | Try Status List, fall back to Introspection       |
| `IntrospectionFirst` | Try Introspection, fall back to Status List       |
| `BothMustPass`       | Both methods must confirm active status           |

## 4. Handling Authentication

Token Introspection endpoints typically require authentication. The client supports:

### Client Credentials

```csharp
var options = new TokenIntrospectionOptions
{
    IntrospectionEndpoint = "https://issuer.example.com/oauth/introspect",
    ClientId = "verifier-client-id",
    ClientSecret = "verifier-client-secret",
    AuthenticationMethod = ClientAuthenticationMethod.ClientSecretPost
};
```

### Bearer Token

```csharp
var options = new TokenIntrospectionOptions
{
    IntrospectionEndpoint = "https://issuer.example.com/oauth/introspect",
    AccessToken = "bearer-token-from-authorization"
};
```

## 5. Error Handling

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

## Best Practices

1. **Use Hybrid Checking** for high-value credentials that require both real-time verification and privacy-preserving fallback.

2. **Cache Introspection Responses** with short TTLs to reduce load on the authorization server while maintaining freshness.

3. **Implement Circuit Breakers** to handle introspection service unavailability gracefully.

4. **Log Verification Methods** for audit trails showing which method confirmed the credential status.

5. **Consider Rate Limits** - Introspection endpoints may have rate limits; use Status Lists for high-volume scenarios.

## RFC 7662 Compliance

The implementation follows RFC 7662 (OAuth 2.0 Token Introspection) with support for:

- Standard request/response format
- Client authentication methods (client_secret_post, client_secret_basic)
- Token type hints
- Extension parameters

---

## See Also

- [Managing Revocation with Status Lists](managing-revocation.md)
- [RFC 7662 - OAuth 2.0 Token Introspection](https://tools.ietf.org/html/rfc7662)
- [Token Status List Specification](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/)
