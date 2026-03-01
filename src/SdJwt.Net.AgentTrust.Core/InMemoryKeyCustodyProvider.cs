using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// In-memory key custody for dev/test.
/// </summary>
public class InMemoryKeyCustodyProvider : IKeyCustodyProvider
{
    private readonly Dictionary<string, (SecurityKey Key, string Algorithm)> _keys = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes provider.
    /// </summary>
    public InMemoryKeyCustodyProvider()
    {
    }

    /// <summary>
    /// Initializes provider with preconfigured keys.
    /// </summary>
    public InMemoryKeyCustodyProvider(IReadOnlyDictionary<string, (SecurityKey Key, string Algorithm)> keys)
    {
        foreach (var item in keys)
        {
            _keys[item.Key] = item.Value;
        }
    }

    /// <inheritdoc/>
    public Task<SecurityKey> GetSigningKeyAsync(string agentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            throw new ArgumentException("Agent id is required.", nameof(agentId));
        }

        if (!_keys.TryGetValue(agentId, out var material))
        {
            var random = new byte[32];
            RandomNumberGenerator.Fill(random);
            material = (new SymmetricSecurityKey(random), SecurityAlgorithms.HmacSha256);
            _keys[agentId] = material;
        }

        return Task.FromResult(material.Key);
    }

    /// <inheritdoc/>
    public Task<string> GetSigningAlgorithmAsync(string agentId, CancellationToken cancellationToken = default)
    {
        if (!_keys.TryGetValue(agentId, out var material))
        {
            return Task.FromResult(SecurityAlgorithms.HmacSha256);
        }

        return Task.FromResult(material.Algorithm);
    }

    /// <inheritdoc/>
    public Task RotateKeyAsync(string agentId, CancellationToken cancellationToken = default)
    {
        var random = new byte[32];
        RandomNumberGenerator.Fill(random);
        _keys[agentId] = (new SymmetricSecurityKey(random), SecurityAlgorithms.HmacSha256);
        return Task.CompletedTask;
    }
}

