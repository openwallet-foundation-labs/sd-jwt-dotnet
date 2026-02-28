using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using System.Security.Cryptography;

namespace SdJwt.Net.Benchmarks;

/// <summary>
/// HAIP compliance validation benchmarks measuring algorithm and key validation overhead.
/// These validations run on every credential operation in compliant deployments.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class HaipBenchmarks
{
    private ECDsa? _ecdsaP256;
    private ECDsa? _ecdsaP384;
    private ECDsaSecurityKey? _p256Key;
    private ECDsaSecurityKey? _p384Key;
    private HaipCryptoValidator? _level1Validator;
    private HaipCryptoValidator? _level2Validator;
    private HaipCryptoValidator? _level3Validator;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _ecdsaP256 = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _ecdsaP384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        _p256Key = new ECDsaSecurityKey(_ecdsaP256) { KeyId = "bench-p256-key" };
        _p384Key = new ECDsaSecurityKey(_ecdsaP384) { KeyId = "bench-p384-key" };

        _level1Validator = new HaipCryptoValidator(HaipLevel.Level1_High, NullLogger<HaipCryptoValidator>.Instance);
        _level2Validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, NullLogger<HaipCryptoValidator>.Instance);
        _level3Validator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, NullLogger<HaipCryptoValidator>.Instance);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _ecdsaP256?.Dispose();
        _ecdsaP384?.Dispose();
    }

    /// <summary>
    /// Level 1 validation with P-256 key and ES256 algorithm.
    /// Baseline for general-purpose deployments.
    /// </summary>
    [Benchmark(Description = "Level1 P-256/ES256")]
    public HaipComplianceResult ValidateLevel1_P256()
    {
        return _level1Validator!.ValidateKeyCompliance(_p256Key!, SecurityAlgorithms.EcdsaSha256);
    }

    /// <summary>
    /// Level 2 validation with P-256 key.
    /// Financial and corporate deployments.
    /// </summary>
    [Benchmark(Description = "Level2 P-256/ES256")]
    public HaipComplianceResult ValidateLevel2_P256()
    {
        return _level2Validator!.ValidateKeyCompliance(_p256Key!, SecurityAlgorithms.EcdsaSha256);
    }

    /// <summary>
    /// Level 2 validation with P-384 key for enhanced security margin.
    /// </summary>
    [Benchmark(Description = "Level2 P-384/ES384")]
    public HaipComplianceResult ValidateLevel2_P384()
    {
        return _level2Validator!.ValidateKeyCompliance(_p384Key!, SecurityAlgorithms.EcdsaSha384);
    }

    /// <summary>
    /// Level 3 (Sovereign) validation with P-384 key.
    /// Government and critical infrastructure deployments.
    /// </summary>
    [Benchmark(Description = "Level3 P-384/ES384")]
    public HaipComplianceResult ValidateLevel3_P384()
    {
        return _level3Validator!.ValidateKeyCompliance(_p384Key!, SecurityAlgorithms.EcdsaSha384);
    }
}
