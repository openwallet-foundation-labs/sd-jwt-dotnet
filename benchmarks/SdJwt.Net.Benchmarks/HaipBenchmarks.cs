using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using System.Security.Cryptography;

namespace SdJwt.Net.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class HaipBenchmarks
{
    private ECDsa? _ecdsa;
    private ECDsaSecurityKey? _ecdsaKey;
    private HaipCryptoValidator? _validator;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        _ecdsaKey = new ECDsaSecurityKey(_ecdsa) { KeyId = "bench-haip-key" };
        _validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, NullLogger<HaipCryptoValidator>.Instance);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _ecdsa?.Dispose();
    }

    [Benchmark]
    public HaipComplianceResult ValidateKeyCompliance()
    {
        return _validator!.ValidateKeyCompliance(_ecdsaKey!, SecurityAlgorithms.EcdsaSha384);
    }
}
