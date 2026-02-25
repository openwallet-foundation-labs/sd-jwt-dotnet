using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.StatusList.Models;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;
using System.Text.Json;
using StatusListModel = SdJwt.Net.StatusList.Models.StatusList;

namespace SdJwt.Net.StatusList.Issuer;

/// <summary>
/// Manages the creation and signing of Status List Tokens according to draft-ietf-oauth-status-list-13.
/// Provides scalable, privacy-preserving credential status management with compression and caching.
/// </summary>
public class StatusListManager {
        private readonly SecurityKey _signingKey;
        private readonly string _signingAlgorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusListManager"/> class.
        /// </summary>
        /// <param name="signingKey">The security key used to sign the Status List Token.</param>
        /// <param name="signingAlgorithm">The JWT signing algorithm.</param>
        public StatusListManager(SecurityKey signingKey, string signingAlgorithm) {
                if (string.IsNullOrWhiteSpace(signingAlgorithm))
                        throw new ArgumentException("Value cannot be null or whitespace.", nameof(signingAlgorithm));

                _signingKey = signingKey ?? throw new ArgumentNullException(nameof(signingKey));
                _signingAlgorithm = signingAlgorithm;
        }

        /// <summary>
        /// Creates a signed Status List Token according to draft-ietf-oauth-status-list-13.
        /// </summary>
        /// <param name="subject">The subject (URI) of the Status List Token. Must equal the uri in referenced tokens.</param>
        /// <param name="statusValues">Array of status values for Referenced Tokens.</param>
        /// <param name="bits">Number of bits per status value (1, 2, 4, or 8).</param>
        /// <param name="validUntil">Optional expiration time.</param>
        /// <param name="timeToLive">Optional TTL in seconds for caching.</param>
        /// <param name="aggregationUri">Optional URI for Status List Aggregation.</param>
        /// <returns>A signed Status List Token in JWT format.</returns>
        public async Task<string> CreateStatusListTokenAsync(
            string subject,
            byte[] statusValues,
            int bits = 1,
            DateTime? validUntil = null,
            int? timeToLive = null,
            string? aggregationUri = null) {
                if (string.IsNullOrWhiteSpace(subject))
                        throw new ArgumentException("Subject cannot be null or empty", nameof(subject));
                if (statusValues == null)
                        throw new ArgumentNullException(nameof(statusValues));
                if (bits != 1 && bits != 2 && bits != 4 && bits != 8)
                        throw new ArgumentException("Bits must be 1, 2, 4, or 8", nameof(bits));

                // Create compressed byte array according to specification
                var compressedBits = await CompressStatusListAsync(statusValues, bits);
                var encodedBits = Base64UrlEncoder.Encode(compressedBits);

                // Create Status List structure
                var statusList = new StatusListModel {
                        Bits = bits,
                        List = encodedBits,
                        AggregationUri = aggregationUri
                };

                // Create the payload
                var payload = new StatusListTokenPayload {
                        Subject = subject,
                        IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        ExpiresAt = validUntil.HasValue ? ((DateTimeOffset)validUntil.Value).ToUnixTimeSeconds() : null,
                        TimeToLive = timeToLive,
                        StatusList = statusList
                };

                return CreateJwtToken(payload);
        }

        /// <summary>
        /// Creates a Status List Token from a BitArray of status values.
        /// </summary>
        /// <param name="subject">The subject (URI) of the Status List Token.</param>
        /// <param name="statusBits">BitArray representing status values.</param>
        /// <param name="bits">Number of bits per status value.</param>
        /// <param name="validUntil">Optional expiration time.</param>
        /// <param name="timeToLive">Optional TTL in seconds.</param>
        /// <param name="aggregationUri">Optional aggregation URI.</param>
        /// <returns>A signed Status List Token.</returns>
        public async Task<string> CreateStatusListTokenFromBitArrayAsync(
            string subject,
            BitArray statusBits,
            int bits = 1,
            DateTime? validUntil = null,
            int? timeToLive = null,
            string? aggregationUri = null) {
                if (statusBits == null)
                        throw new ArgumentNullException(nameof(statusBits));

                // Convert BitArray to byte array based on bit size
                var statusValues = ConvertBitArrayToStatusValues(statusBits, bits);

                return await CreateStatusListTokenAsync(
                    subject, statusValues, bits, validUntil, timeToLive, aggregationUri);
        }

        /// <summary>
        /// Updates the status of specific Referenced Tokens in an existing Status List.
        /// </summary>
        /// <param name="existingToken">The current Status List Token.</param>
        /// <param name="updates">Dictionary mapping indices to new status values.</param>
        /// <returns>A new Status List Token with updated statuses.</returns>
        public async Task<string> UpdateStatusAsync(string existingToken, Dictionary<int, StatusType> updates) {
                if (string.IsNullOrEmpty(existingToken))
                        throw new ArgumentException("Existing token cannot be null or empty", nameof(existingToken));
                if (updates == null || updates.Count == 0)
                        throw new ArgumentException("Updates cannot be null or empty", nameof(updates));

                // Parse existing token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(existingToken);

                // Extract status list from payload
                var statusListJson = jwt.Claims.FirstOrDefault(c => c.Type == "status_list")?.Value
                    ?? throw new InvalidOperationException("Status list claim not found in token");

                var statusList = JsonSerializer.Deserialize<StatusListModel>(statusListJson, SdJwtConstants.DefaultJsonSerializerOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize status list");

                // Decompress and update status values
                var currentValues = await DecompressStatusListAsync(statusList.List, statusList.Bits);

                foreach (var update in updates) {
                        var index = update.Key;
                        var newStatus = (byte)update.Value;

                        if (index < 0 || index >= currentValues.Length)
                                throw new ArgumentOutOfRangeException(nameof(updates), $"Index {index} is out of range");

                        currentValues[index] = newStatus;
                }

                // Create new token with updated values
                var subject = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                    ?? throw new InvalidOperationException("Subject claim not found in token");

                var exp = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
                DateTime? validUntil = null;
                if (!string.IsNullOrEmpty(exp) && long.TryParse(exp, out var expValue)) {
                        validUntil = DateTimeOffset.FromUnixTimeSeconds(expValue).DateTime;
                }

                var ttl = jwt.Claims.FirstOrDefault(c => c.Type == "ttl")?.Value;
                int? timeToLive = null;
                if (!string.IsNullOrEmpty(ttl) && int.TryParse(ttl, out var ttlValue)) {
                        timeToLive = ttlValue;
                }

                return await CreateStatusListTokenAsync(
                    subject, currentValues, statusList.Bits, validUntil, timeToLive, statusList.AggregationUri);
        }

        /// <summary>
        /// Revokes Referenced Tokens by setting their status to INVALID.
        /// </summary>
        /// <param name="existingToken">The current Status List Token.</param>
        /// <param name="indices">Indices of tokens to revoke.</param>
        /// <returns>Updated Status List Token.</returns>
        public async Task<string> RevokeTokensAsync(string existingToken, int[] indices) {
                var updates = indices.ToDictionary(i => i, _ => StatusType.Invalid);
                return await UpdateStatusAsync(existingToken, updates);
        }

        /// <summary>
        /// Suspends Referenced Tokens by setting their status to SUSPENDED.
        /// </summary>
        /// <param name="existingToken">The current Status List Token.</param>
        /// <param name="indices">Indices of tokens to suspend.</param>
        /// <returns>Updated Status List Token.</returns>
        public async Task<string> SuspendTokensAsync(string existingToken, int[] indices) {
                var updates = indices.ToDictionary(i => i, _ => StatusType.Suspended);
                return await UpdateStatusAsync(existingToken, updates);
        }

        /// <summary>
        /// Reinstates Referenced Tokens by setting their status to VALID.
        /// </summary>
        /// <param name="existingToken">The current Status List Token.</param>
        /// <param name="indices">Indices of tokens to reinstate.</param>
        /// <returns>Updated Status List Token.</returns>
        public async Task<string> ReinstateTokensAsync(string existingToken, int[] indices) {
                var updates = indices.ToDictionary(i => i, _ => StatusType.Valid);
                return await UpdateStatusAsync(existingToken, updates);
        }

        /// <summary>
        /// Creates a Status List Aggregation document.
        /// </summary>
        /// <param name="statusListUris">Array of Status List Token URIs.</param>
        /// <returns>JSON representation of Status List Aggregation.</returns>
        public string CreateStatusListAggregation(string[] statusListUris) {
                if (statusListUris == null || statusListUris.Length == 0)
                        throw new ArgumentException("Status list URIs cannot be null or empty", nameof(statusListUris));

                var aggregation = new StatusListAggregation {
                        StatusLists = statusListUris
                };

                return JsonSerializer.Serialize(aggregation, SdJwtConstants.DefaultJsonSerializerOptions);
        }

        /// <summary>
        /// Creates a JWT token from the Status List Token payload.
        /// </summary>
        private string CreateJwtToken(StatusListTokenPayload payload) {
                var tokenHandler = new JwtSecurityTokenHandler();
                var header = new JwtHeader(new SigningCredentials(_signingKey, _signingAlgorithm)) {
                        [JwtHeaderParameterNames.Typ] = "statuslist+jwt"
                };

                var claims = new List<Claim>
                {
            new(JwtRegisteredClaimNames.Sub, payload.Subject),
            new(JwtRegisteredClaimNames.Iat, payload.IssuedAt.ToString()),
            new("status_list", JsonSerializer.Serialize(payload.StatusList, SdJwtConstants.DefaultJsonSerializerOptions), JsonClaimValueTypes.Json)
        };

                if (payload.ExpiresAt.HasValue)
                        claims.Add(new(JwtRegisteredClaimNames.Exp, payload.ExpiresAt.Value.ToString()));

                if (payload.TimeToLive.HasValue)
                        claims.Add(new("ttl", payload.TimeToLive.Value.ToString()));

                var jwtPayload = new JwtPayload(claims);
                var token = new JwtSecurityToken(header, jwtPayload);

                return tokenHandler.WriteToken(token);
        }

        #region Safe API Methods for Production Use

        /// <summary>
        /// Safely extracts status bits from a Status List Token using proper parsing.
        /// Provides safe alternative to manual bit manipulation.
        /// </summary>
        /// <param name="statusListToken">The Status List Token to parse.</param>
        /// <returns>BitArray containing the status bits.</returns>
        public static BitArray GetBitsFromToken(string statusListToken) {
                if (string.IsNullOrEmpty(statusListToken))
                        throw new ArgumentException("Status list token cannot be null or empty", nameof(statusListToken));

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(statusListToken);

                var statusListClaim = jwt.Claims.FirstOrDefault(c => c.Type == "status_list")?.Value
                    ?? throw new InvalidOperationException("Status list claim not found in token");

                var statusList = JsonSerializer.Deserialize<StatusListModel>(statusListClaim, SdJwtConstants.DefaultJsonSerializerOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize status list");

                // Decompress and convert to BitArray
                var statusValues = DecompressStatusListAsync(statusList.List, statusList.Bits).GetAwaiter().GetResult();

                // Convert status values to bit array
                var totalBits = statusValues.Length * statusList.Bits;
                var bitArray = new BitArray(totalBits);

                for (int i = 0; i < statusValues.Length; i++) {
                        var statusValue = statusValues[i];
                        var bitIndex = i * statusList.Bits;

                        for (int bit = 0; bit < statusList.Bits; bit++) {
                                var globalBitIndex = bitIndex + bit;
                                bitArray[globalBitIndex] = (statusValue & (1 << bit)) != 0;
                        }
                }

                return bitArray;
        }

        /// <summary>
        /// Creates a properly initialized BitArray for status list with specified number of credentials and bits per credential.
        /// Provides safe initialization avoiding manual bit calculation errors.
        /// </summary>
        /// <param name="credentialCount">Number of credentials the status list will track.</param>
        /// <param name="bitsPerCredential">Number of bits per credential (1, 2, 4, or 8).</param>
        /// <returns>Initialized BitArray ready for use.</returns>
        public BitArray CreateStatusBits(int credentialCount, int bitsPerCredential = 1) {
                if (credentialCount <= 0)
                        throw new ArgumentException("Credential count must be positive", nameof(credentialCount));

                if (bitsPerCredential != 1 && bitsPerCredential != 2 && bitsPerCredential != 4 && bitsPerCredential != 8)
                        throw new ArgumentException("Bits per credential must be 1, 2, 4, or 8", nameof(bitsPerCredential));

                var totalBits = credentialCount * bitsPerCredential;
                return new BitArray(totalBits); // All bits default to false (Valid status)
        }

        /// <summary>
        /// Safely sets the status of a credential in a status bits array.
        /// Prevents manual bit manipulation errors and ensures correct encoding.
        /// </summary>
        /// <param name="statusBits">The BitArray containing status bits.</param>
        /// <param name="credentialIndex">The index of the credential to update.</param>
        /// <param name="status">The new status to set.</param>
        /// <param name="bitsPerCredential">Number of bits per credential (defaults to 1).</param>
        public void SetCredentialStatus(BitArray statusBits, int credentialIndex, StatusType status, int bitsPerCredential = 1) {
                if (statusBits == null)
                        throw new ArgumentNullException(nameof(statusBits));

                if (credentialIndex < 0)
                        throw new ArgumentException("Credential index must be non-negative", nameof(credentialIndex));

                if (bitsPerCredential != 1 && bitsPerCredential != 2 && bitsPerCredential != 4 && bitsPerCredential != 8)
                        throw new ArgumentException("Bits per credential must be 1, 2, 4, or 8", nameof(bitsPerCredential));

                var startBitIndex = credentialIndex * bitsPerCredential;

                if (startBitIndex + bitsPerCredential > statusBits.Length)
                        throw new ArgumentOutOfRangeException(nameof(credentialIndex),
                            $"Credential index {credentialIndex} with {bitsPerCredential} bits would exceed BitArray length {statusBits.Length}");

                var statusValue = (int)status;

                // Set bits for this credential (little-endian format)
                for (int bit = 0; bit < bitsPerCredential; bit++) {
                        var bitIndex = startBitIndex + bit;
                        statusBits[bitIndex] = (statusValue & (1 << bit)) != 0;
                }
        }

        /// <summary>
        /// Safely retrieves the status of a credential from a status bits array.
        /// Prevents manual bit manipulation errors and ensures correct decoding.
        /// </summary>
        /// <param name="statusBits">The BitArray containing status bits.</param>
        /// <param name="credentialIndex">The index of the credential to query.</param>
        /// <param name="bitsPerCredential">Number of bits per credential (defaults to 1).</param>
        /// <returns>The status of the specified credential.</returns>
        public StatusType GetCredentialStatus(BitArray statusBits, int credentialIndex, int bitsPerCredential = 1) {
                if (statusBits == null)
                        throw new ArgumentNullException(nameof(statusBits));

                if (credentialIndex < 0)
                        throw new ArgumentException("Credential index must be non-negative", nameof(credentialIndex));

                if (bitsPerCredential != 1 && bitsPerCredential != 2 && bitsPerCredential != 4 && bitsPerCredential != 8)
                        throw new ArgumentException("Bits per credential must be 1, 2, 4, or 8", nameof(bitsPerCredential));

                var startBitIndex = credentialIndex * bitsPerCredential;

                if (startBitIndex + bitsPerCredential > statusBits.Length)
                        throw new ArgumentOutOfRangeException(nameof(credentialIndex),
                            $"Credential index {credentialIndex} with {bitsPerCredential} bits would exceed BitArray length {statusBits.Length}");

                var statusValue = 0;

                // Extract bits for this credential (little-endian format)
                for (int bit = 0; bit < bitsPerCredential; bit++) {
                        var bitIndex = startBitIndex + bit;
                        if (statusBits[bitIndex]) {
                                statusValue |= (1 << bit);
                        }
                }

                return StatusTypeExtensions.FromValue((byte)statusValue);
        }

        #endregion

        /// <summary>
        /// Compresses status values using DEFLATE with ZLIB as specified in draft-13.
        /// </summary>
        private static async Task<byte[]> CompressStatusListAsync(byte[] statusValues, int bits) {
                // Convert status values to bit array based on bits per status
                var totalBits = statusValues.Length * bits;
                var byteArray = new byte[(totalBits + 7) / 8];

                for (int i = 0; i < statusValues.Length; i++) {
                        var statusValue = statusValues[i];
                        var bitIndex = i * bits;

                        // Set bits for this status value
                        for (int bit = 0; bit < bits; bit++) {
                                var globalBitIndex = bitIndex + bit;
                                var byteIndex = globalBitIndex / 8;
                                var bitInByte = globalBitIndex % 8;

                                if ((statusValue & (1 << bit)) != 0) {
                                        byteArray[byteIndex] |= (byte)(1 << bitInByte);
                                }
                        }
                }

                // Compress using DEFLATE with ZLIB format
                using var output = new MemoryStream();
                using (var deflate = new DeflateStream(output, CompressionLevel.Optimal)) {
                        await deflate.WriteAsync(byteArray, 0, byteArray.Length);
                }

                return output.ToArray();
        }

        /// <summary>
        /// Decompresses status list and extracts status values.
        /// </summary>
        private static async Task<byte[]> DecompressStatusListAsync(string encodedList, int bits) {
                var compressedBytes = Base64UrlEncoder.DecodeBytes(encodedList);

                using var input = new MemoryStream(compressedBytes);
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();

                await deflate.CopyToAsync(output);
                var decompressedBytes = output.ToArray();

                // Convert back to status values
                var totalBits = decompressedBytes.Length * 8;
                var statusCount = totalBits / bits;
                var statusValues = new byte[statusCount];

                for (int i = 0; i < statusCount; i++) {
                        var bitIndex = i * bits;
                        byte statusValue = 0;

                        for (int bit = 0; bit < bits; bit++) {
                                var globalBitIndex = bitIndex + bit;
                                var byteIndex = globalBitIndex / 8;
                                var bitInByte = globalBitIndex % 8;

                                if (byteIndex < decompressedBytes.Length &&
                                    (decompressedBytes[byteIndex] & (1 << bitInByte)) != 0) {
                                        statusValue |= (byte)(1 << bit);
                                }
                        }

                        statusValues[i] = statusValue;
                }

                return statusValues;
        }

        /// <summary>
        /// Converts BitArray to status values based on bit size.
        /// </summary>
        private static byte[] ConvertBitArrayToStatusValues(BitArray bitArray, int bits) {
                var statusCount = bitArray.Length / bits;
                var statusValues = new byte[statusCount];

                for (int i = 0; i < statusCount; i++) {
                        byte statusValue = 0;
                        for (int bit = 0; bit < bits; bit++) {
                                var bitIndex = i * bits + bit;
                                if (bitIndex < bitArray.Length && bitArray[bitIndex]) {
                                        statusValue |= (byte)(1 << bit);
                                }
                        }
                        statusValues[i] = statusValue;
                }

                return statusValues;
        }
}