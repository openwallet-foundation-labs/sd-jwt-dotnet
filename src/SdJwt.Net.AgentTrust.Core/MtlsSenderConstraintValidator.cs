using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Default <see cref="IMtlsSenderConstraintValidator"/> implementation per RFC 8705.
/// Confirms the presented TLS client certificate's SHA-256 thumbprint matches the capability
/// token's <c>cnf["x5t#S256"]</c> binding.
/// </summary>
public sealed class MtlsSenderConstraintValidator : IMtlsSenderConstraintValidator
{
    private const string ProofType = "mtls";

    /// <inheritdoc/>
    public Task<ProofValidationResult> ValidateAsync(
        X509Certificate2 clientCertificate,
        string expectedThumbprint,
        CancellationToken cancellationToken = default)
    {
        if (clientCertificate == null)
        {
            return Fail("Client certificate is missing.", "missing_client_certificate");
        }

        if (string.IsNullOrWhiteSpace(expectedThumbprint))
        {
            return Fail("Expected certificate thumbprint (cnf.x5t#S256) is missing.", "missing_cnf_x5t");
        }

        var actualThumbprint = ComputeCertificateThumbprint(clientCertificate);
        if (!FixedTimeEquals(actualThumbprint, expectedThumbprint))
        {
            return Fail("Client certificate thumbprint does not match the token's cnf binding.", "mtls_thumbprint_mismatch");
        }

        return Task.FromResult(ProofValidationResult.Success(ProofType));
    }

    /// <summary>
    /// Computes the RFC 8705 <c>x5t#S256</c> value: base64url(SHA-256(DER(certificate))).
    /// </summary>
    public static string ComputeCertificateThumbprint(X509Certificate2 certificate)
    {
        if (certificate == null)
        {
            throw new ArgumentNullException(nameof(certificate));
        }

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(certificate.RawData);
        return Base64UrlEncoder.Encode(hash);
    }

    private static bool FixedTimeEquals(string? left, string? right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
            CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static Task<ProofValidationResult> Fail(string error, string errorCode)
        => Task.FromResult(ProofValidationResult.Failure(error, errorCode, ProofType));
}
