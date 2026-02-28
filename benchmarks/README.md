# Benchmarks

Performance benchmarks for the SD-JWT .NET ecosystem using BenchmarkDotNet.

## Run Locally

```pwsh
# Run all benchmarks with standard settings
dotnet run --configuration Release --project benchmarks/SdJwt.Net.Benchmarks/SdJwt.Net.Benchmarks.csproj

# Quick run for development (fewer iterations)
dotnet run --configuration Release --project benchmarks/SdJwt.Net.Benchmarks/SdJwt.Net.Benchmarks.csproj -- --job short --warmupCount 1 --iterationCount 3 --exporters markdown json

# Run specific benchmark class
dotnet run --configuration Release --project benchmarks/SdJwt.Net.Benchmarks/SdJwt.Net.Benchmarks.csproj -- --filter *CoreSdJwt*
```

Results are generated in: `benchmarks/SdJwt.Net.Benchmarks/BenchmarkDotNet.Artifacts/results/`

## Benchmark Suites

### CoreSdJwtBenchmarks

Core SD-JWT operations - the critical path for credential workflows.

| Benchmark | Description | Target |
|-----------|-------------|--------|
| Issue (8 claims, 4 SD) | Standard credential issuance | < 500 us |
| Issue (20 claims, 12 SD) | Large credential stress test | < 1 ms |
| Present (2 disclosures) | Selective presentation | < 100 us |
| Present (8 disclosures) | Large presentation | < 200 us |
| Verify presentation | Signature + disclosure validation | < 300 us |

### StatusListBenchmarks

Revocation and status checking - critical for high-volume verifiers.

| Benchmark | Description | Target |
|-----------|-------------|--------|
| Create token (4K creds) | Standard issuer list | < 10 ms |
| Create token (100K creds) | Large issuer stress test | < 100 ms |
| Check status | Per-credential check | < 50 us |

### HaipBenchmarks

HAIP compliance validation overhead per security level.

| Benchmark | Description | Use Case |
|-----------|-------------|----------|
| Level1 P-256/ES256 | General purpose | Consumer apps |
| Level2 P-256/ES256 | Financial/corporate | Banking, enterprise |
| Level2 P-384/ES384 | Enhanced margin | High-value transactions |
| Level3 P-384/ES384 | Sovereign | Government ID |

## Target Framework

Benchmarks run on **.NET 9.0**. BenchmarkDotNet will add support for .NET 10 in a future release.

## Notes

- Results are environment-dependent (CPU, OS, .NET runtime, power profile)
- CI runs with `--job short` for practical runtime
- For accurate results, close other applications and use release builds
- Memory allocations matter for high-throughput scenarios
