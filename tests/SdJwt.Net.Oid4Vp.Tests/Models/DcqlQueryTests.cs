using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Models;

public class DcqlQueryTests {

        // ------------------------------------------------------------
        // DcqlQuery validation
        // ------------------------------------------------------------

        [Fact]
        public void DcqlQuery_Validate_WithValidCredentials_Succeeds() {
                var query = new DcqlQuery {
                        Credentials = [ CreateValidCredentialQuery("pid") ]
                };

                var ex = Record.Exception(() => query.Validate());
                Assert.Null(ex);
        }

        [Fact]
        public void DcqlQuery_Validate_WithEmptyCredentials_Throws() {
                var query = new DcqlQuery { Credentials = Array.Empty<DcqlCredentialQuery>() };

                Assert.Throws<InvalidOperationException>(() => query.Validate());
        }

        [Fact]
        public void DcqlQuery_Validate_WithNullCredentials_Throws() {
                var query = new DcqlQuery { Credentials = null! };

                Assert.Throws<InvalidOperationException>(() => query.Validate());
        }

        [Fact]
        public void DcqlQuery_Validate_WithValidCredentialSets_Succeeds() {
                var query = new DcqlQuery {
                        Credentials = [ CreateValidCredentialQuery("pid"), CreateValidCredentialQuery("dl") ],
                        CredentialSets = [
                                new DcqlCredentialSetQuery {
                                        Options = [ ["pid"], ["dl"] ]
                                }
                        ]
                };

                var ex = Record.Exception(() => query.Validate());
                Assert.Null(ex);
        }

        // ------------------------------------------------------------
        // DcqlCredentialQuery validation
        // ------------------------------------------------------------

        [Fact]
        public void DcqlCredentialQuery_Validate_WithValidValues_Succeeds() {
                var query = CreateValidCredentialQuery("pid");

                var ex = Record.Exception(() => query.Validate());
                Assert.Null(ex);
        }

        [Fact]
        public void DcqlCredentialQuery_Validate_WithEmptyId_Throws() {
                var query = new DcqlCredentialQuery { Id = "", Format = "vc+sd-jwt" };

                Assert.Throws<InvalidOperationException>(() => query.Validate());
        }

        [Fact]
        public void DcqlCredentialQuery_Validate_WithEmptyFormat_Throws() {
                var query = new DcqlCredentialQuery { Id = "pid", Format = "" };

                Assert.Throws<InvalidOperationException>(() => query.Validate());
        }

        // ------------------------------------------------------------
        // DcqlCredentialSetQuery validation
        // ------------------------------------------------------------

        [Fact]
        public void DcqlCredentialSetQuery_Validate_WithEmptyOptions_Throws() {
                var query = new DcqlCredentialSetQuery { Options = Array.Empty<string[]>() };

                Assert.Throws<InvalidOperationException>(() => query.Validate());
        }

        [Fact]
        public void DcqlCredentialSetQuery_RequiredDefaultsToNull() {
                var query = new DcqlCredentialSetQuery { Options = [ ["pid"] ] };

                Assert.Null(query.Required);
        }

        // ------------------------------------------------------------
        // JSON serialization round-trips
        // ------------------------------------------------------------

        [Fact]
        public void DcqlQuery_Serialization_RoundTrip() {
                var query = new DcqlQuery {
                        Credentials = [
                                new DcqlCredentialQuery {
                                        Id = "pid",
                                        Format = "vc+sd-jwt",
                                        Claims = [
                                                new DcqlClaimsQuery {
                                                        Path = [ "family_name" ]
                                                }
                                        ]
                                }
                        ]
                };

                var json = JsonSerializer.Serialize(query);
                var deserialized = JsonSerializer.Deserialize<DcqlQuery>(json);

                Assert.NotNull(deserialized);
                Assert.Single(deserialized.Credentials);
                Assert.Equal("pid", deserialized.Credentials[0].Id);
                Assert.Equal("vc+sd-jwt", deserialized.Credentials[0].Format);
                Assert.Single(deserialized.Credentials[0].Claims!);
        }

        [Fact]
        public void DcqlCredentialSetQuery_Serialization_RoundTrip() {
                var setQuery = new DcqlCredentialSetQuery {
                        Options = [ ["pid"], ["dl"] ],
                        Required = false,
                        Purpose = "Age verification"
                };

                var json = JsonSerializer.Serialize(setQuery);
                var deserialized = JsonSerializer.Deserialize<DcqlCredentialSetQuery>(json);

                Assert.NotNull(deserialized);
                Assert.Equal(2, deserialized.Options.Length);
                Assert.Equal(false, deserialized.Required);
        }

        [Fact]
        public void DcqlClaimsQuery_Serialization_RoundTrip() {
                var claimsQuery = new DcqlClaimsQuery {
                        Path = [ "address", "street_address" ],
                        Values = [ "123 Main St" ]
                };

                var json = JsonSerializer.Serialize(claimsQuery);
                var deserialized = JsonSerializer.Deserialize<DcqlClaimsQuery>(json);

                Assert.NotNull(deserialized);
                Assert.Equal(2, deserialized.Path.Length);
                Assert.Single(deserialized.Values!);
        }

        // ------------------------------------------------------------
        // AuthorizationRequest integration
        // ------------------------------------------------------------

        [Fact]
        public void AuthorizationRequest_CreateCrossDeviceWithDcql_CreatesValidRequest() {
                var dcqlQuery = CreateValidDcqlQuery();

                var request = AuthorizationRequest.CreateCrossDeviceWithDcql(
                    "https://verifier.example.com",
                    "https://verifier.example.com/callback",
                    "test-nonce",
                    dcqlQuery);

                Assert.Equal("https://verifier.example.com", request.ClientId);
                Assert.Equal(Oid4VpConstants.ResponseTypes.VpToken, request.ResponseType);
                Assert.Equal(Oid4VpConstants.ResponseModes.DirectPost, request.ResponseMode);
                Assert.NotNull(request.DcqlQuery);
                Assert.Null(request.PresentationDefinition);
        }

        [Fact]
        public void AuthorizationRequest_Validate_WithDcqlOnly_Succeeds() {
                var request = new AuthorizationRequest {
                        ClientId = "https://verifier.example.com",
                        ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
                        Nonce = "test-nonce",
                        DcqlQuery = CreateValidDcqlQuery()
                };

                var ex = Record.Exception(() => request.Validate());
                Assert.Null(ex);
        }

        [Fact]
        public void AuthorizationRequest_Validate_WithBothDcqlAndPresentationDefinition_Throws() {
                var request = new AuthorizationRequest {
                        ClientId = "https://verifier.example.com",
                        ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
                        Nonce = "test-nonce",
                        DcqlQuery = CreateValidDcqlQuery(),
                        PresentationDefinition = PresentationDefinition.CreateSimple("test", "TestCredential")
                };

                Assert.Throws<InvalidOperationException>(() => request.Validate());
        }

        [Fact]
        public void AuthorizationRequest_Validate_WithNeitherDcqlNorPresentationDef_Throws() {
                var request = new AuthorizationRequest {
                        ClientId = "https://verifier.example.com",
                        ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
                        Nonce = "test-nonce"
                };

                Assert.Throws<InvalidOperationException>(() => request.Validate());
        }

        [Fact]
        public void AuthorizationRequest_NewParameters_DefaultToNull() {
                var request = new AuthorizationRequest();

                Assert.Null(request.DcqlQuery);
                Assert.Null(request.RequestUriMethod);
                Assert.Null(request.TransactionData);
                Assert.Null(request.WalletNonce);
                Assert.Null(request.VerifierInfo);
        }

        [Fact]
        public void AuthorizationRequest_RequestUriMethod_Serializes() {
                var request = AuthorizationRequest.CreateCrossDeviceWithDcql(
                    "https://verifier.example.com",
                    "https://verifier.example.com/callback",
                    "test-nonce",
                    CreateValidDcqlQuery());
                request.RequestUriMethod = Oid4VpConstants.RequestUriMethods.Post;

                var json = JsonSerializer.Serialize(request);

                Assert.Contains("\"request_uri_method\"", json);
                Assert.Contains("\"post\"", json);
        }

        [Fact]
        public void Oid4VpConstants_RequestUriMethods_HaveExpectedValues() {
                Assert.Equal("get", Oid4VpConstants.RequestUriMethods.Get);
                Assert.Equal("post", Oid4VpConstants.RequestUriMethods.Post);
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------

        private static DcqlCredentialQuery CreateValidCredentialQuery(string id) =>
                new DcqlCredentialQuery { Id = id, Format = "vc+sd-jwt" };

        private static DcqlQuery CreateValidDcqlQuery() =>
                new DcqlQuery {
                        Credentials = [ CreateValidCredentialQuery("pid") ]
                };
}
