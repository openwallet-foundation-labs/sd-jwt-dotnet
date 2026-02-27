using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Oid4Vci.Client;
using SdJwt.Net.Oid4Vci.Issuer;
using SdJwt.Net.Oid4Vci.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests;

public class ProofTests
{
    [Fact]
    public void ProofBuilder_CreateProof_ProducesValidJwt()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var nonce = "test-nonce-123";
        var clientId = "wallet-client-123";

        // Act
        var proof = ProofBuilder.CreateProof(privateKey, issuerUrl, nonce, clientId);

        // Assert
        Assert.NotNull(proof);
        Assert.NotEmpty(proof);

        // Verify the JWT structure
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(proof);

        Assert.Equal(Oid4VciConstants.ProofJwtType, token.Header.Typ);
        Assert.Equal(SecurityAlgorithms.EcdsaSha256, token.Header.Alg);
        Assert.True(token.Header.ContainsKey("jwk"));

        var claims = token.Claims.ToDictionary(c => c.Type, c => c.Value);
        Assert.Equal(clientId, claims["iss"]);
        Assert.Equal(issuerUrl, claims["aud"]);
        Assert.Equal(nonce, claims["nonce"]);
        Assert.True(claims.ContainsKey("iat"));
    }

    [Fact]
    public void ProofBuilder_CreateProofWithoutClientId_ProducesValidJwt()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var nonce = "test-nonce-123";

        // Act
        var proof = ProofBuilder.CreateProof(privateKey, issuerUrl, nonce);

        // Assert
        Assert.NotNull(proof);
        Assert.NotEmpty(proof);

        // Verify the JWT structure
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(proof);

        Assert.Equal(Oid4VciConstants.ProofJwtType, token.Header.Typ);

        var claims = token.Claims.ToDictionary(c => c.Type, c => c.Value);
        Assert.False(claims.ContainsKey("iss")); // No client ID provided
        Assert.Equal(issuerUrl, claims["aud"]);
        Assert.Equal(nonce, claims["nonce"]);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_ValidProof_Success()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();
        var clientId = "wallet-client-123";

        var proof = ProofBuilder.CreateProof(privateKey, issuerUrl, nonce, clientId);

        // Act
        var result = CNonceValidator.ValidateProof(proof, nonce, issuerUrl);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PublicKey);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(issuerUrl, result.Audience);
        Assert.Equal(nonce, result.Nonce);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WrongNonce_ThrowsException()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var correctNonce = CNonceValidator.GenerateNonce();
        var wrongNonce = CNonceValidator.GenerateNonce();

        var proof = ProofBuilder.CreateProof(privateKey, issuerUrl, correctNonce);

        // Act & Assert
        var exception = Assert.Throws<ProofValidationException>(() =>
            CNonceValidator.ValidateProof(proof, wrongNonce, issuerUrl));

        Assert.Contains("Invalid nonce", exception.Message);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WrongAudience_ThrowsException()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var correctIssuerUrl = "https://issuer.example.com";
        var wrongIssuerUrl = "https://wrong-issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();

        var proof = ProofBuilder.CreateProof(privateKey, correctIssuerUrl, nonce);

        // Act & Assert
        var exception = Assert.Throws<ProofValidationException>(() =>
            CNonceValidator.ValidateProof(proof, nonce, wrongIssuerUrl));

        Assert.Contains("Invalid audience", exception.Message);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WrongJwtType_ThrowsException()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();

        // Create a JWT with wrong type
        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(privateKey, SecurityAlgorithms.EcdsaSha256))
        {
            ["typ"] = "wrong-type" // Wrong type
        };

        var payload = new JwtPayload
        {
            ["aud"] = issuerUrl,
            ["nonce"] = nonce,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var token = new JwtSecurityToken(header, payload);
        var wrongTypeProof = handler.WriteToken(token);

        // Act & Assert
        var exception = Assert.Throws<ProofValidationException>(() =>
            CNonceValidator.ValidateProof(wrongTypeProof, nonce, issuerUrl));

        Assert.Contains("Invalid JWT type", exception.Message);
    }

    [Fact]
    public void CNonceValidator_GenerateNonce_ProducesValidNonce()
    {
        // Act
        var nonce1 = CNonceValidator.GenerateNonce();
        var nonce2 = CNonceValidator.GenerateNonce(16);

        // Assert
        Assert.NotNull(nonce1);
        Assert.Equal(32, nonce1.Length); // Default length
        Assert.NotNull(nonce2);
        Assert.Equal(16, nonce2.Length);
        Assert.NotEqual(nonce1, nonce2); // Should be different
    }

    [Fact]
    public void ProofBuilder_WithExplicitAlgorithm_ProducesCorrectJwt()
    {
        // Arrange
        using var key = RSA.Create(2048);
        var privateKey = new RsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var nonce = "test-nonce-123";
        var algorithm = SecurityAlgorithms.RsaSha256;

        // Act
        var proof = ProofBuilder.CreateProof(privateKey, algorithm, issuerUrl, nonce);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(proof);

        Assert.Equal(Oid4VciConstants.ProofJwtType, token.Header.Typ);
        Assert.Equal(algorithm, token.Header.Alg);
        Assert.True(token.Header.ContainsKey("jwk"));
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WithKidResolver_Success()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key) { KeyId = "kid-1" };
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(privateKey, SecurityAlgorithms.EcdsaSha256))
        {
            ["typ"] = Oid4VciConstants.ProofJwtType
        };
        header["kid"] = "kid-1";

        var payload = new JwtPayload
        {
            ["aud"] = issuerUrl,
            ["nonce"] = nonce,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var token = new JwtSecurityToken(header, payload);
        var jwt = handler.WriteToken(token);

        // Act
        var result = CNonceValidator.ValidateProof(
            jwt,
            nonce,
            issuerUrl,
            kid => kid == "kid-1" ? privateKey : null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nonce, result.Nonce);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WithX5cHeader_Success()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=oid4vci-proof-test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        var signingKey = new X509SecurityKey(cert);
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256))
        {
            ["typ"] = Oid4VciConstants.ProofJwtType
        };
        header["x5c"] = new[] { Convert.ToBase64String(cert.RawData) };

        var payload = new JwtPayload
        {
            ["aud"] = issuerUrl,
            ["nonce"] = nonce,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var token = new JwtSecurityToken(header, payload);
        var jwt = handler.WriteToken(token);

        // Act
        var result = CNonceValidator.ValidateProof(jwt, nonce, issuerUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(issuerUrl, result.Audience);
        Assert.Equal(nonce, result.Nonce);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WithX5cTrustValidation_SucceedsForTrustedRoot()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=oid4vci-proof-root",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        var signingKey = new X509SecurityKey(cert);
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256))
        {
            ["typ"] = Oid4VciConstants.ProofJwtType
        };
        header["x5c"] = new[] { Convert.ToBase64String(cert.RawData) };

        var payload = new JwtPayload
        {
            ["aud"] = issuerUrl,
            ["nonce"] = nonce,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var token = new JwtSecurityToken(header, payload);
        var jwt = handler.WriteToken(token);

        var options = new ProofValidationOptions
        {
            ValidateX5cChain = true,
            TrustedRootCertificates = new[] { cert }
        };

        // Act
        var result = CNonceValidator.ValidateProof(jwt, nonce, issuerUrl, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nonce, result.Nonce);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WithX5cTrustValidation_ThrowsWhenRootNotTrusted()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=oid4vci-proof-root",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        using var otherRootKey = RSA.Create(2048);
        var otherReq = new CertificateRequest(
            "CN=oid4vci-untrusted-root",
            otherRootKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var untrustedRoot = otherReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        var signingKey = new X509SecurityKey(cert);
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256))
        {
            ["typ"] = Oid4VciConstants.ProofJwtType
        };
        header["x5c"] = new[] { Convert.ToBase64String(cert.RawData) };

        var payload = new JwtPayload
        {
            ["aud"] = issuerUrl,
            ["nonce"] = nonce,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var token = new JwtSecurityToken(header, payload);
        var jwt = handler.WriteToken(token);

        var options = new ProofValidationOptions
        {
            ValidateX5cChain = true,
            TrustedRootCertificates = new[] { untrustedRoot }
        };

        // Act & Assert
        var ex = Assert.Throws<ProofValidationException>(() =>
            CNonceValidator.ValidateProof(jwt, nonce, issuerUrl, options));
        Assert.Contains("trusted root", ex.Message);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WithRequiredAttestation_ThrowsWhenMissing()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();
        var proof = ProofBuilder.CreateProof(privateKey, issuerUrl, nonce);
        var options = new ProofValidationOptions
        {
            RequireKeyAttestation = true
        };

        // Act & Assert
        var ex = Assert.Throws<ProofValidationException>(() =>
            CNonceValidator.ValidateProof(proof, nonce, issuerUrl, options));
        Assert.Contains("attestation", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CNonceValidator_ValidateProof_WithAttestationValidator_Succeeds()
    {
        // Arrange
        using var key = ECDsa.Create();
        var privateKey = new ECDsaSecurityKey(key);
        var issuerUrl = "https://issuer.example.com";
        var nonce = CNonceValidator.GenerateNonce();

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(privateKey, SecurityAlgorithms.EcdsaSha256))
        {
            ["typ"] = Oid4VciConstants.ProofJwtType
        };
        var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(privateKey);
        jwk.D = null;
        jwk.DP = null;
        jwk.DQ = null;
        jwk.P = null;
        jwk.Q = null;
        jwk.QI = null;
        header["jwk"] = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Serialize(jwk))!;
        header["key_attestation"] = "attestation-token";

        var payload = new JwtPayload
        {
            ["aud"] = issuerUrl,
            ["nonce"] = nonce,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var token = new JwtSecurityToken(header, payload);
        var jwt = handler.WriteToken(token);

        var options = new ProofValidationOptions
        {
            RequireKeyAttestation = true,
            KeyAttestationValidator = (attestation, _, _) => attestation == "attestation-token"
        };

        // Act
        var result = CNonceValidator.ValidateProof(jwt, nonce, issuerUrl, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nonce, result.Nonce);
    }
}
