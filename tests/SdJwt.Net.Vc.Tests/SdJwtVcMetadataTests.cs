using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Metadata;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

public class SdJwtVcMetadataTests : TestBase {
        [Fact]
        public void IntegrityMetadataValidator_ValidatesSha256Digest() {
                const string content = "{\"vct\":\"https://example.com/types/pid\"}";
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
                var integrity = $"sha-256-{Convert.ToBase64String(hash)}";

                Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
                Assert.False(IntegrityMetadataValidator.Validate(content, "sha-256-AAAAAAAAAAAAAAAAAAAAAA=="));
        }

        [Fact]
        public async Task JwtVcIssuerMetadataResolver_RejectsWhenBothJwksAndJwksUriArePresent() {
                var issuer = "https://issuer.example.com";
                var metadataJson = """
                    {
                      "issuer": "https://issuer.example.com",
                      "jwks_uri": "https://issuer.example.com/jwks.json",
                      "jwks": { "keys": [ { "kty": "EC", "crv": "P-256", "x": "x", "y": "y", "kid": "1" } ] }
                    }
                    """;

                using var httpClient = CreateHttpClient(new Dictionary<string, HttpResponseMessage> {
                    ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = CreateJsonResponse(metadataJson)
                });

                var resolver = new JwtVcIssuerMetadataResolver(httpClient);
                await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(issuer));
        }

        [Fact]
        public async Task TypeMetadataResolver_DetectsCircularExtendsDependency() {
                var a = "https://types.example.com/a";
                var b = "https://types.example.com/b";

                var aJson = $$"""
                    {
                      "vct": "{{a}}",
                      "extends": "{{b}}"
                    }
                    """;
                var bJson = $$"""
                    {
                      "vct": "{{b}}",
                      "extends": "{{a}}"
                    }
                    """;

                using var httpClient = CreateHttpClient(new Dictionary<string, HttpResponseMessage> {
                    [a] = CreateJsonResponse(aJson),
                    [b] = CreateJsonResponse(bJson)
                });

                var resolver = new TypeMetadataResolver(httpClient);
                await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(a));
        }

        [Fact]
        public async Task VerifyAsync_WithTypeMetadataPolicy_ValidatesVctIntegrity() {
                var vct = "https://types.example.com/pid";
                var typeMetadataJson = $$"""
                    {
                      "vct": "{{vct}}",
                      "display": [
                        { "locale": "en-US", "name": "PID" }
                      ]
                    }
                    """;
                var integrity = $"sha-256-{Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(typeMetadataJson)))}";

                var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
                var payload = new SdJwtVcPayload {
                        Issuer = TrustedIssuer,
                        Subject = "did:example:123",
                        VctIntegrity = integrity,
                        AdditionalData = new Dictionary<string, object> { ["given_name"] = "Alice" }
                };
                var output = vcIssuer.Issue(vct, payload, new SdIssuanceOptions());
                var holder = new SdJwtHolder(output.Issuance);
                var presentation = holder.CreatePresentation(_ => true);

                var options = new TypeMetadataResolverOptions();
                options.LocalTypeMetadataByVct[vct] = typeMetadataJson;
                using var httpClient = new HttpClient(new StubHttpHandler(new Dictionary<string, HttpResponseMessage>()));
                var typeResolver = new TypeMetadataResolver(httpClient, options);

                var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
                var validationParams = new TokenValidationParameters {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false
                };
                var policy = new SdJwtVcVerificationPolicy {
                        RequireTypeMetadata = true,
                        TypeMetadataResolver = typeResolver
                };

                var result = await verifier.VerifyAsync(
                    presentation,
                    validationParams,
                    verificationPolicy: policy);

                Assert.Equal(vct, result.VerifiableCredentialType);
        }

        [Fact]
        public async Task VerifyAsync_WithTypeMetadataPolicy_ThrowsOnInvalidVctIntegrity() {
                var vct = "https://types.example.com/pid";
                var typeMetadataJson = $$"""
                    {
                      "vct": "{{vct}}"
                    }
                    """;

                var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
                var payload = new SdJwtVcPayload {
                        Issuer = TrustedIssuer,
                        Subject = "did:example:123",
                        VctIntegrity = "sha-256-AAAAAAAAAAAAAAAAAAAAAA==",
                        AdditionalData = new Dictionary<string, object> { ["given_name"] = "Alice" }
                };
                var output = vcIssuer.Issue(vct, payload, new SdIssuanceOptions());
                var holder = new SdJwtHolder(output.Issuance);
                var presentation = holder.CreatePresentation(_ => true);

                var options = new TypeMetadataResolverOptions();
                options.LocalTypeMetadataByVct[vct] = typeMetadataJson;
                using var httpClient = new HttpClient(new StubHttpHandler(new Dictionary<string, HttpResponseMessage>()));
                var typeResolver = new TypeMetadataResolver(httpClient, options);

                var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
                var validationParams = new TokenValidationParameters {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false
                };
                var policy = new SdJwtVcVerificationPolicy {
                        RequireTypeMetadata = true,
                        TypeMetadataResolver = typeResolver
                };

                await Assert.ThrowsAsync<SecurityTokenException>(() =>
                    verifier.VerifyAsync(presentation, validationParams, verificationPolicy: policy));
        }

        [Fact]
        public async Task VerifyAsync_WithMetadataBasedIssuerKeyResolver_ValidatesIssuerSignature() {
                var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
                var payload = new SdJwtVcPayload {
                        Issuer = TrustedIssuer,
                        Subject = "did:example:123",
                        AdditionalData = new Dictionary<string, object> { ["given_name"] = "Alice" }
                };
                var output = vcIssuer.Issue("https://types.example.com/pid", payload, new SdIssuanceOptions());
                var holder = new SdJwtHolder(output.Issuance);
                var presentation = holder.CreatePresentation(_ => true);

                var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
                var metadataJson = JsonSerializer.Serialize(new {
                        issuer = TrustedIssuer,
                        jwks = new {
                                keys = new[] { issuerJwk }
                        }
                });

                using var httpClient = CreateHttpClient(new Dictionary<string, HttpResponseMessage> {
                    [$"{TrustedIssuer}/.well-known/jwt-vc-issuer"] = CreateJsonResponse(metadataJson)
                });

                var metadataResolver = new JwtVcIssuerMetadataResolver(httpClient);
                var keyResolver = new JwtVcIssuerSigningKeyResolver(metadataResolver, httpClient);
                var verifier = new SdJwtVcVerifier(keyResolver);
                var validationParams = new TokenValidationParameters {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false
                };

                var result = await verifier.VerifyAsync(presentation, validationParams);
                Assert.Equal("https://types.example.com/pid", result.VerifiableCredentialType);
        }

        [Fact]
        public async Task TypeMetadataResolver_RejectsUnsafeInlineSvgTemplate() {
                var vct = "https://types.example.com/render-unsafe";
                var unsafeSvg = "<svg xmlns='http://www.w3.org/2000/svg'><script>alert(1)</script></svg>";
                var unsafeSvgDataUri = $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(unsafeSvg))}";
                var metadataJson = $$"""
                    {
                      "vct": "{{vct}}",
                      "display": [
                        {
                          "locale": "en-US",
                          "name": "Unsafe VC",
                          "rendering": {
                            "svg_templates": [
                              { "uri": "{{unsafeSvgDataUri}}" }
                            ]
                          }
                        }
                      ]
                    }
                    """;

                var options = new TypeMetadataResolverOptions();
                options.LocalTypeMetadataByVct[vct] = metadataJson;
                using var httpClient = new HttpClient(new StubHttpHandler(new Dictionary<string, HttpResponseMessage>()));
                var resolver = new TypeMetadataResolver(httpClient, options);

                await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
        }

        [Fact]
        public async Task TypeMetadataResolver_RejectsInvalidDataUriIntegrity() {
                var vct = "https://types.example.com/render-integrity-bad";
                var svg = "<svg xmlns='http://www.w3.org/2000/svg'><text>Hello</text></svg>";
                var svgBytes = Encoding.UTF8.GetBytes(svg);
                var dataUri = $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";
                var metadataJson = $$"""
                    {
                      "vct": "{{vct}}",
                      "display": [
                        {
                          "locale": "en-US",
                          "name": "Integrity VC",
                          "rendering": {
                            "svg_templates": [
                              { "uri": "{{dataUri}}", "uri#integrity": "sha-256-AAAAAAAAAAAAAAAAAAAAAA==" }
                            ]
                          }
                        }
                      ]
                    }
                    """;

                var options = new TypeMetadataResolverOptions();
                options.LocalTypeMetadataByVct[vct] = metadataJson;
                using var httpClient = new HttpClient(new StubHttpHandler(new Dictionary<string, HttpResponseMessage>()));
                var resolver = new TypeMetadataResolver(httpClient, options);

                await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
        }

        [Fact]
        public async Task TypeMetadataResolver_ValidatesRemoteSvgResourceIntegrity() {
                var vct = "https://types.example.com/render-remote-integrity";
                var svgBytes = Encoding.UTF8.GetBytes("<svg xmlns='http://www.w3.org/2000/svg'><text>hello</text></svg>");
                var integrity = $"sha-256-{Convert.ToBase64String(SHA256.HashData(svgBytes))}";
                var remoteSvgUri = "https://assets.example.com/card.svg";
                var metadataJson = $$"""
                    {
                      "vct": "{{vct}}",
                      "display": [
                        {
                          "locale": "en-US",
                          "name": "Remote SVG VC",
                          "rendering": {
                            "svg_templates": [
                              { "uri": "{{remoteSvgUri}}", "uri#integrity": "{{integrity}}" }
                            ]
                          }
                        }
                      ]
                    }
                    """;

                var options = new TypeMetadataResolverOptions();
                options.LocalTypeMetadataByVct[vct] = metadataJson;
                using var httpClient = CreateHttpClient(new Dictionary<string, HttpResponseMessage> {
                    [remoteSvgUri] = new HttpResponseMessage(HttpStatusCode.OK) {
                            Content = new StringContent(Encoding.UTF8.GetString(svgBytes), Encoding.UTF8, "image/svg+xml")
                    }
                });
                var resolver = new TypeMetadataResolver(httpClient, options);

                var result = await resolver.ResolveAsync(vct);
                Assert.Equal(vct, result.Metadata.Vct);
        }

        [Fact]
        public async Task TypeMetadataResolver_RejectsExtensionSelectiveDisclosureConflict() {
                var child = "https://types.example.com/child";
                var parent = "https://types.example.com/parent";
                var childJson = $$"""
                    {
                      "vct": "{{child}}",
                      "extends": "{{parent}}",
                      "claims": [
                        { "path": ["age"], "sd": "never" }
                      ]
                    }
                    """;
                var parentJson = $$"""
                    {
                      "vct": "{{parent}}",
                      "claims": [
                        { "path": ["age"], "sd": "always" }
                      ]
                    }
                    """;

                using var httpClient = CreateHttpClient(new Dictionary<string, HttpResponseMessage> {
                    [child] = CreateJsonResponse(childJson),
                    [parent] = CreateJsonResponse(parentJson)
                });
                var resolver = new TypeMetadataResolver(httpClient);

                await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(child));
        }

        private static HttpClient CreateHttpClient(Dictionary<string, HttpResponseMessage> responses) {
                return new HttpClient(new StubHttpHandler(responses));
        }

        private static HttpResponseMessage CreateJsonResponse(string json) {
                return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
        }

        private sealed class StubHttpHandler(Dictionary<string, HttpResponseMessage> responses) : HttpMessageHandler {
                private readonly Dictionary<string, HttpResponseMessage> _responses = responses;

                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
                        var key = request.RequestUri!.AbsoluteUri;
                        if (_responses.TryGetValue(key, out var response)) {
                                return Task.FromResult(CloneResponse(response));
                        }

                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) {
                                Content = new StringContent("not found", Encoding.UTF8, "text/plain")
                        });
                }

                private static HttpResponseMessage CloneResponse(HttpResponseMessage original) {
                        var clone = new HttpResponseMessage(original.StatusCode);
                        if (original.Content != null) {
                                var payload = original.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                var mediaType = original.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                                clone.Content = new StringContent(payload, Encoding.UTF8, mediaType);
                                if (original.Content.Headers.ContentType?.CharSet != null) {
                                        clone.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType) {
                                                CharSet = original.Content.Headers.ContentType.CharSet
                                        };
                                }
                        }
                        return clone;
                }
        }
}
