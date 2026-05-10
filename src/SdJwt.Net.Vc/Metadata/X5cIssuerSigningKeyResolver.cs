using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.Vc.Metadata;

/// <summary>
/// Options for x5c-based issuer signing key resolution.
/// </summary>
public class X5cIssuerSigningKeyResolverOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to validate the certificate chain.
    /// </summary>
    public bool ValidateCertificateChain { get; set; } = true;

    /// <summary>
    /// Gets or sets the trusted root certificates for chain validation.
    /// When empty, the system certificate store is used.
    /// </summary>
    public X509Certificate2Collection TrustedRoots { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to check certificate revocation.
    /// </summary>
    public bool CheckRevocation { get; set; } = true;

    /// <summary>
    /// Gets or sets the revocation mode for certificate validation.
    /// </summary>
    public X509RevocationMode RevocationMode { get; set; } = X509RevocationMode.Online;

    /// <summary>
    /// Gets or sets the allowed clock skew for certificate validity checks.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the current time for validation. When null, uses the system clock.
    /// </summary>
    public DateTimeOffset? ValidationTime
    {
        get; set;
    }
}

/// <summary>
/// Resolves issuer signing keys from the x5c JWT header parameter for SD-JWT VC verification.
/// Implements HAIP 1.0 Final Section 6.1 requirement for x5c-based issuer key resolution.
/// </summary>
public class X5cIssuerSigningKeyResolver : IJwtVcIssuerSigningKeyResolver
{
    private readonly X5cIssuerSigningKeyResolverOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="X5cIssuerSigningKeyResolver"/> class.
    /// </summary>
    /// <param name="options">Optional resolver options.</param>
    public X5cIssuerSigningKeyResolver(X5cIssuerSigningKeyResolverOptions? options = null)
    {
        _options = options ?? new X5cIssuerSigningKeyResolverOptions();
    }

    /// <inheritdoc />
    public Task<SecurityKey> ResolveSigningKeyAsync(JwtSecurityToken token, CancellationToken cancellationToken = default)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        var x5cValues = ExtractX5cHeader(token);
        if (x5cValues == null || x5cValues.Count == 0)
        {
            throw new SecurityTokenException("JWT header does not contain an 'x5c' parameter.");
        }

        var certificates = ParseCertificateChain(x5cValues);
        if (certificates.Count == 0)
        {
            throw new SecurityTokenException("No valid certificates found in the 'x5c' header.");
        }

        var leafCertificate = certificates[0];

        if (_options.ValidateCertificateChain)
        {
            ValidateChain(certificates);
        }

        var key = new X509SecurityKey(leafCertificate);
        return Task.FromResult<SecurityKey>(key);
    }

    private static List<string>? ExtractX5cHeader(JwtSecurityToken token)
    {
        if (!token.Header.TryGetValue("x5c", out var x5cRaw))
        {
            return null;
        }

        if (x5cRaw is List<object> objectList)
        {
            return objectList.Select(o => o?.ToString() ?? string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        if (x5cRaw is System.Text.Json.JsonElement jsonElement &&
            jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            var result = new List<string>();
            foreach (var item in jsonElement.EnumerateArray())
            {
                var value = item.GetString();
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }
            return result;
        }

        // Handle IEnumerable<object> from various JSON libraries
        if (x5cRaw is System.Collections.IEnumerable enumerable and not string)
        {
            var result = new List<string>();
            foreach (var item in enumerable)
            {
                var value = item?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }
            return result.Count > 0 ? result : null;
        }

        return null;
    }

    private static List<X509Certificate2> ParseCertificateChain(List<string> x5cValues)
    {
        var certificates = new List<X509Certificate2>();
        foreach (var base64Cert in x5cValues)
        {
            byte[] certBytes;
            try
            {
                certBytes = Convert.FromBase64String(base64Cert);
            }
            catch (FormatException ex)
            {
                throw new SecurityTokenException("Invalid Base64 encoding in 'x5c' certificate.", ex);
            }

            try
            {
#if NET9_0_OR_GREATER
                certificates.Add(X509CertificateLoader.LoadCertificate(certBytes));
#else
                certificates.Add(new X509Certificate2(certBytes));
#endif
            }
            catch (CryptographicException ex)
            {
                throw new SecurityTokenException("Invalid X.509 certificate in 'x5c' header.", ex);
            }
        }

        return certificates;
    }

    private void ValidateChain(List<X509Certificate2> certificates)
    {
        var leafCertificate = certificates[0];
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = _options.CheckRevocation
            ? _options.RevocationMode
            : X509RevocationMode.NoCheck;

        if (_options.ValidationTime.HasValue)
        {
            chain.ChainPolicy.VerificationTime = _options.ValidationTime.Value.UtcDateTime;
        }

        // Add intermediate certificates from the x5c chain
        for (int i = 1; i < certificates.Count; i++)
        {
            chain.ChainPolicy.ExtraStore.Add(certificates[i]);
        }

        // Add trusted roots
#if !NETSTANDARD2_1
        if (_options.TrustedRoots.Count > 0)
        {
            chain.ChainPolicy.CustomTrustStore.AddRange(_options.TrustedRoots);
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        }
#else
        // CustomTrustStore is not available on netstandard2.1;
        // trusted roots are added to ExtraStore instead.
        if (_options.TrustedRoots.Count > 0)
        {
            chain.ChainPolicy.ExtraStore.AddRange(_options.TrustedRoots);
        }
#endif

        if (!chain.Build(leafCertificate))
        {
            var errors = chain.ChainStatus
                .Select(s => s.StatusInformation)
                .Where(s => !string.IsNullOrEmpty(s));
            throw new SecurityTokenException(
                $"Certificate chain validation failed: {string.Join("; ", errors)}");
        }
    }
}
