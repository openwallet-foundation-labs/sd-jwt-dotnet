# Platform Support

This page summarizes target frameworks, runtime platforms, and performance measurement for SD-JWT .NET.

## Supported Target Frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0
- .NET Standard 2.1 for compatible packages

Support is validated through CI where package dependencies and platform APIs allow. Individual packages may support a subset depending on their external dependencies.

## Supported Platforms

- Windows x64, x86, ARM64
- Linux x64, ARM64
- macOS x64 and Apple Silicon
- Containers running supported .NET runtimes
- Cloud deployments on Azure, AWS, GCP, or equivalent platforms

## Performance Benchmarks

Performance is measured with a BenchmarkDotNet harness in [`benchmarks/SdJwt.Net.Benchmarks`](../benchmarks/SdJwt.Net.Benchmarks).

Run benchmarks locally:

```pwsh
dotnet run --configuration Release --project benchmarks/SdJwt.Net.Benchmarks/SdJwt.Net.Benchmarks.csproj -- --job short --warmupCount 1 --iterationCount 3 --exporters markdown json
```

Benchmark results are generated in:

- `benchmarks/SdJwt.Net.Benchmarks/BenchmarkDotNet.Artifacts/results/`

The CI `performance-benchmarks` job executes the same harness and uploads result artifacts for each run.

## Related Documentation

- [Security Model](security.md)
- [Documentation Portal](README.md)
- [Package Maturity](../MATURITY.md)
