using System.Net;
using System.Text;
using SdJwt.Net.Vc.Metadata;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Shared helper methods for VC metadata and verifier tests.
/// </summary>
public static class VcMetadataTestHelpers
{
    /// <summary>
    /// Creates an HttpClient with stubbed responses for testing.
    /// </summary>
    /// <param name="responses">Dictionary mapping URLs to HttpResponseMessage instances.</param>
    /// <returns>An HttpClient configured with the stubbed responses.</returns>
    public static HttpClient CreateHttpClient(Dictionary<string, HttpResponseMessage> responses)
    {
        return new HttpClient(new StubHttpHandler(responses));
    }

    /// <summary>
    /// Creates an HTTP 200 OK response with JSON content.
    /// </summary>
    /// <param name="json">The JSON content string.</param>
    /// <returns>An HttpResponseMessage with JSON content type.</returns>
    public static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// A stub HTTP handler that returns predefined responses based on request URLs.
    /// </summary>
    public sealed class StubHttpHandler(Dictionary<string, HttpResponseMessage> responses) : HttpMessageHandler
    {
        private readonly Dictionary<string, HttpResponseMessage> _responses = responses;

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = request.RequestUri!.AbsoluteUri;
            if (_responses.TryGetValue(key, out var response))
            {
                return Task.FromResult(CloneResponse(response));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("not found", Encoding.UTF8, "text/plain")
            });
        }

        private static HttpResponseMessage CloneResponse(HttpResponseMessage original)
        {
            var clone = new HttpResponseMessage(original.StatusCode);
            if (original.Content != null)
            {
                var payload = original.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var mediaType = original.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                clone.Content = new StringContent(payload, Encoding.UTF8, mediaType);
            }
            return clone;
        }
    }

    /// <summary>
    /// A mock implementation of IJwtVcIssuerMetadataResolver for testing.
    /// </summary>
    public sealed class MockJwtVcIssuerMetadataResolver : IJwtVcIssuerMetadataResolver
    {
        /// <summary>
        /// Gets or sets the result to return from ResolveAsync.
        /// </summary>
        public JwtVcIssuerMetadataResolutionResult? Result
        {
            get; set;
        }

        /// <inheritdoc />
        public Task<JwtVcIssuerMetadataResolutionResult> ResolveAsync(string issuer, CancellationToken cancellationToken = default)
        {
            if (Result == null)
            {
                throw new InvalidOperationException("MockJwtVcIssuerMetadataResolver.Result not configured.");
            }
            return Task.FromResult(Result);
        }
    }
}
