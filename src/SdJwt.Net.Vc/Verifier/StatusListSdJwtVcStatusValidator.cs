using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using System.Text.Json;

namespace SdJwt.Net.Vc.Verifier;

/// <summary>
/// SD-JWT VC status validator backed by OAuth Status List checks.
/// </summary>
public sealed class StatusListSdJwtVcStatusValidator : ISdJwtVcStatusValidator, IDisposable {
        private readonly StatusListVerifier _statusListVerifier;
        private readonly Func<string, Task<SecurityKey>> _issuerKeyProvider;
        private readonly StatusListOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusListSdJwtVcStatusValidator"/> class.
        /// </summary>
        /// <param name="issuerKeyProvider">Issuer signing key resolver used for Status List Token validation.</param>
        /// <param name="options">Optional status list options.</param>
        /// <param name="httpClient">Optional HTTP client.</param>
        /// <param name="memoryCache">Optional in-memory cache.</param>
        /// <param name="logger">Optional logger.</param>
        public StatusListSdJwtVcStatusValidator(
            Func<string, Task<SecurityKey>> issuerKeyProvider,
            StatusListOptions? options = null,
            HttpClient? httpClient = null,
            IMemoryCache? memoryCache = null,
            ILogger<StatusListVerifier>? logger = null) {
                _issuerKeyProvider = issuerKeyProvider ?? throw new ArgumentNullException(nameof(issuerKeyProvider));
                _options = options ?? new StatusListOptions();
                // This validator is explicitly used for status checks, so always enable status checking.
                _options.EnableStatusChecking = true;
                _statusListVerifier = new StatusListVerifier(httpClient, memoryCache, logger);
        }

        /// <inheritdoc />
        public async Task<bool> ValidateAsync(object statusClaim, CancellationToken cancellationToken = default) {
                if (statusClaim == null) {
                        return false;
                }

                cancellationToken.ThrowIfCancellationRequested();

                StatusClaim parsedStatusClaim;
                try {
                        parsedStatusClaim = ParseStatusClaim(statusClaim);
                }
                catch {
                        return false;
                }

                var result = await _statusListVerifier
                    .CheckStatusAsync(parsedStatusClaim, _issuerKeyProvider, _options)
                    .ConfigureAwait(false);
                return result.Status == StatusType.Valid;
        }

        /// <summary>
        /// Disposes validator resources.
        /// </summary>
        public void Dispose() {
                _statusListVerifier.Dispose();
        }

        private static StatusClaim ParseStatusClaim(object statusClaim) {
                if (statusClaim is StatusClaim typed) {
                        return typed;
                }

                if (statusClaim is JsonElement jsonElement) {
                        var parsed = jsonElement.Deserialize<StatusClaim>(SdJwtConstants.DefaultJsonSerializerOptions);
                        if (parsed?.StatusList != null) {
                                return parsed;
                        }
                }

                var json = JsonSerializer.Serialize(statusClaim, SdJwtConstants.DefaultJsonSerializerOptions);
                var fallback = JsonSerializer.Deserialize<StatusClaim>(json, SdJwtConstants.DefaultJsonSerializerOptions);
                if (fallback?.StatusList == null) {
                        throw new InvalidOperationException("Status claim could not be parsed as StatusClaim.");
                }

                return fallback;
        }
}
