using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace SdJwt.Net.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class StatusListBenchmarks : IDisposable
{
    private const string StatusListUri = "https://status.example.com/list/primary";

    private ECDsa? _issuerEcdsa;
    private ECDsaSecurityKey? _issuerKey;
    private StatusListManager? _statusListManager;
    private byte[]? _statusValues;
    private string? _prebuiltStatusListToken;
    private StatusListVerifier? _statusListVerifier;
    private StatusClaim? _statusClaim;
    private StatusListOptions? _statusOptions;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _issuerKey = new ECDsaSecurityKey(_issuerEcdsa) { KeyId = "bench-status-key" };
        _statusListManager = new StatusListManager(_issuerKey, SecurityAlgorithms.EcdsaSha256);

        _statusValues = new byte[4096];
        _statusValues[42] = 1;
        _statusValues[1024] = 1;

        _prebuiltStatusListToken = _statusListManager
            .CreateStatusListTokenAsync(StatusListUri, _statusValues, bits: 1)
            .GetAwaiter()
            .GetResult();

        var httpClient = new HttpClient(new StaticTokenHttpMessageHandler(StatusListUri, _prebuiltStatusListToken));
        _statusListVerifier = new StatusListVerifier(httpClient);

        _statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = StatusListUri,
                Index = 42
            }
        };

        _statusOptions = new StatusListOptions
        {
            CacheStatusLists = false
        };
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _statusListVerifier?.Dispose();
        _issuerEcdsa?.Dispose();
    }

    [Benchmark]
    public Task<string> CreateStatusListToken()
    {
        return _statusListManager!.CreateStatusListTokenAsync(StatusListUri, _statusValues!, bits: 1);
    }

    [Benchmark]
    public Task<StatusCheckResult> CheckStatus()
    {
        return _statusListVerifier!.CheckStatusAsync(
            _statusClaim!,
            _ => Task.FromResult<SecurityKey>(_issuerKey!),
            _statusOptions);
    }

    public void Dispose()
    {
        GlobalCleanup();
    }

    private sealed class StaticTokenHttpMessageHandler(string statusListUri, string token) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.ToString() != statusListUri)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(token, Encoding.UTF8, "application/statuslist+jwt")
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/statuslist+jwt");
            return Task.FromResult(response);
        }
    }
}
