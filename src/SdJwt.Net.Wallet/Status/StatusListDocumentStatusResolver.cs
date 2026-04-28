using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Status;

/// <summary>
/// Bridges the StatusList package (<see cref="StatusListVerifier"/>) to the wallet's
/// <see cref="IDocumentStatusResolver"/> interface for live revocation checks.
/// </summary>
public class StatusListDocumentStatusResolver : IDocumentStatusResolver, IDisposable
{
    private readonly StatusListVerifier _verifier;
    private readonly Func<string, Task<SecurityKey>> _issuerKeyResolver;
    private readonly ILogger _logger;
    private readonly bool _ownsVerifier;

    /// <summary>
    /// Initializes a new instance using an externally managed <see cref="StatusListVerifier"/>.
    /// </summary>
    /// <param name="verifier">The status list verifier instance.</param>
    /// <param name="issuerKeyResolver">
    /// A callback that resolves an issuer's public key by issuer identifier.
    /// </param>
    /// <param name="logger">Optional logger.</param>
    public StatusListDocumentStatusResolver(
        StatusListVerifier verifier,
        Func<string, Task<SecurityKey>> issuerKeyResolver,
        ILogger<StatusListDocumentStatusResolver>? logger = null)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
        _issuerKeyResolver = issuerKeyResolver ?? throw new ArgumentNullException(nameof(issuerKeyResolver));
        _logger = logger ?? NullLogger<StatusListDocumentStatusResolver>.Instance;
        _ownsVerifier = false;
    }

    /// <summary>
    /// Initializes a new instance that creates and owns its own <see cref="StatusListVerifier"/>.
    /// </summary>
    /// <param name="issuerKeyResolver">
    /// A callback that resolves an issuer's public key by issuer identifier.
    /// </param>
    /// <param name="httpClient">Optional HTTP client for status list retrieval.</param>
    /// <param name="logger">Optional logger.</param>
    public StatusListDocumentStatusResolver(
        Func<string, Task<SecurityKey>> issuerKeyResolver,
        HttpClient? httpClient = null,
        ILogger<StatusListDocumentStatusResolver>? logger = null)
    {
        _issuerKeyResolver = issuerKeyResolver ?? throw new ArgumentNullException(nameof(issuerKeyResolver));
        _logger = logger ?? NullLogger<StatusListDocumentStatusResolver>.Instance;
        _verifier = new StatusListVerifier(httpClient);
        _ownsVerifier = true;
    }

    /// <inheritdoc/>
    public async Task<DocumentStatusResult> ResolveStatusAsync(
        StoredCredential credential,
        CancellationToken cancellationToken = default)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }

        var statusClaim = ExtractStatusClaim(credential);
        if (statusClaim?.StatusList == null)
        {
            return new DocumentStatusResult
            {
                Status = DocumentStatus.Valid,
                Reason = "No status claim present in credential."
            };
        }

        try
        {
            var result = await _verifier.CheckStatusAsync(statusClaim, _issuerKeyResolver).ConfigureAwait(false);

            return new DocumentStatusResult
            {
                Status = MapStatus(result),
                Reason = result.ErrorMessage,
                Metadata = new Dictionary<string, object>
                {
                    ["statusListUri"] = result.StatusListUri ?? string.Empty,
                    ["statusValue"] = result.StatusValue,
                    ["retrievedAt"] = result.RetrievedAt,
                    ["fromCache"] = result.FromCache
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Status check failed for credential {CredentialId}", credential.Id);
            return new DocumentStatusResult
            {
                Status = DocumentStatus.Reserved,
                Reason = $"Status check failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Extracts a <see cref="StatusClaim"/> from the credential's raw JWT payload.
    /// </summary>
    private static StatusClaim? ExtractStatusClaim(StoredCredential credential)
    {
        if (string.IsNullOrEmpty(credential.RawCredential))
        {
            return null;
        }

        try
        {
            // SD-JWT format: header.payload.signature~disclosure1~disclosure2~...
            var compactToken = credential.RawCredential.Split('~')[0];
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(compactToken))
            {
                return null;
            }

            var jwt = handler.ReadJwtToken(compactToken);
            var statusPayload = jwt.Payload.ContainsKey("status")
                ? jwt.Payload["status"]
                : null;

            if (statusPayload == null)
            {
                return null;
            }

            var json = JsonSerializer.Serialize(statusPayload);
            return JsonSerializer.Deserialize<StatusClaim>(json);
        }
        catch
        {
            return null;
        }
    }

    private static DocumentStatus MapStatus(StatusCheckResult result)
    {
        if (result.IsValid)
        {
            return DocumentStatus.Valid;
        }

        if (result.IsInvalid)
        {
            return DocumentStatus.Invalid;
        }

        if (result.IsSuspended)
        {
            return DocumentStatus.Suspended;
        }

        return DocumentStatus.Reserved;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_ownsVerifier)
        {
            _verifier.Dispose();
        }
    }
}
