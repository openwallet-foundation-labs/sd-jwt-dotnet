# Benchmarks

This repository uses a real benchmark harness based on BenchmarkDotNet.

## Run locally

```pwsh
dotnet run --configuration Release --project benchmarks/SdJwt.Net.Benchmarks/SdJwt.Net.Benchmarks.csproj -- --job short --warmupCount 1 --iterationCount 3 --exporters markdown json
```

Artifacts are generated in:

`benchmarks/SdJwt.Net.Benchmarks/BenchmarkDotNet.Artifacts/results/`

## Benchmarks included

- `CoreSdJwtBenchmarks`
- `StatusListBenchmarks`
- `HaipBenchmarks`

## Notes

- Benchmarks are environment-dependent (CPU, OS, .NET runtime, power profile).
- CI runs with a short job to keep runtime practical and publishes artifacts.
