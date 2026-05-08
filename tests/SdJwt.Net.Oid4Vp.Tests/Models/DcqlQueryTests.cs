using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.Oid4Vp.Models.Dcql.Formats;
using SdJwt.Net.Oid4Vp.Verifier;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Models;

public class DcqlQueryTests
{

    // ------------------------------------------------------------
    // DcqlQuery validation
    // ------------------------------------------------------------

    [Fact]
    public void DcqlQuery_Validate_WithValidCredentials_Succeeds()
    {
        var query = new DcqlQuery
        {
            Credentials = [CreateValidCredentialQuery("pid")]
        };

        var ex = Record.Exception(() => query.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void DcqlQuery_Validate_WithEmptyCredentials_Throws()
    {
        var query = new DcqlQuery { Credentials = Array.Empty<DcqlCredentialQuery>() };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlQuery_Validate_WithNullCredentials_Throws()
    {
        var query = new DcqlQuery { Credentials = null! };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlQuery_Validate_WithValidCredentialSets_Succeeds()
    {
        var query = new DcqlQuery
        {
            Credentials = [CreateValidCredentialQuery("pid"), CreateValidCredentialQuery("dl")],
            CredentialSets = [
                        new DcqlCredentialSetQuery {
                                        Options = [ ["pid"], ["dl"] ]
                                }
                ]
        };

        var ex = Record.Exception(() => query.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void DcqlQuery_Validate_WithDuplicateCredentialIds_Throws()
    {
        var query = new DcqlQuery
        {
            Credentials = [CreateValidCredentialQuery("pid"), CreateValidCredentialQuery("pid")]
        };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlQuery_Validate_WithCredentialSetUnknownReference_Throws()
    {
        var query = new DcqlQuery
        {
            Credentials = [CreateValidCredentialQuery("pid")],
            CredentialSets =
            [
                new DcqlCredentialSetQuery
                {
                    Options = [["pid", "unknown"]]
                }
            ]
        };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    // ------------------------------------------------------------
    // DcqlCredentialQuery validation
    // ------------------------------------------------------------

    [Fact]
    public void DcqlCredentialQuery_Validate_WithValidValues_Succeeds()
    {
        var query = CreateValidCredentialQuery("pid");

        var ex = Record.Exception(() => query.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithEmptyId_Throws()
    {
        var query = new DcqlCredentialQuery { Id = "", Format = "vc+sd-jwt" };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithEmptyFormat_Throws()
    {
        var query = new DcqlCredentialQuery { Id = "pid", Format = "" };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithDcSdJwtMissingMeta_Throws()
    {
        var query = new DcqlCredentialQuery
        {
            Id = "pid",
            Format = Oid4VpConstants.SdJwtVcFormat
        };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithDcSdJwtMissingVctValues_Throws()
    {
        var query = new DcqlCredentialQuery
        {
            Id = "pid",
            Format = Oid4VpConstants.SdJwtVcFormat,
            Meta = new SdJwtVcMeta()
        };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithMsoMdocMissingDoctype_Throws()
    {
        var query = new DcqlCredentialQuery
        {
            Id = "mdl",
            Format = Oid4VpConstants.MsoMdocFormat,
            Meta = new MsoMdocMeta()
        };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithW3cVcMissingTypeValues_Throws()
    {
        var query = new DcqlCredentialQuery
        {
            Id = "degree",
            Format = Oid4VpConstants.JwtVcJsonFormat,
            Meta = new W3cVcMeta()
        };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithInvalidClaimPath_Throws()
    {
        var query = CreateValidCredentialQuery("pid");
        query.Claims =
        [
            new DcqlClaimsQuery { Path = Array.Empty<object>() }
        ];

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialQuery_Validate_WithDuplicateClaimIds_Throws()
    {
        var query = CreateValidCredentialQuery("pid");
        query.Claims =
        [
            new DcqlClaimsQuery { Id = "given", Path = ["given_name"] },
            new DcqlClaimsQuery { Id = "given", Path = ["family_name"] }
        ];

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    // ------------------------------------------------------------
    // DcqlCredentialSetQuery validation
    // ------------------------------------------------------------

    [Fact]
    public void DcqlCredentialSetQuery_Validate_WithEmptyOptions_Throws()
    {
        var query = new DcqlCredentialSetQuery { Options = Array.Empty<string[]>() };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    [Fact]
    public void DcqlCredentialSetQuery_RequiredDefaultsToNull()
    {
        var query = new DcqlCredentialSetQuery { Options = [["pid"]] };

        Assert.Null(query.Required);
    }

    [Fact]
    public void DcqlCredentialSetQuery_Validate_WithEmptyInnerOption_Throws()
    {
        var query = new DcqlCredentialSetQuery { Options = [Array.Empty<string>()] };

        Assert.Throws<InvalidOperationException>(() => query.Validate());
    }

    // ------------------------------------------------------------
    // JSON serialization round-trips
    // ------------------------------------------------------------

    [Fact]
    public void DcqlQuery_Serialization_RoundTrip()
    {
        var query = new DcqlQuery
        {
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
    public void DcqlCredentialSetQuery_Serialization_RoundTrip()
    {
        var setQuery = new DcqlCredentialSetQuery
        {
            Options = [["pid"], ["dl"]],
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
    public void DcqlClaimsQuery_Serialization_RoundTrip()
    {
        var claimsQuery = new DcqlClaimsQuery
        {
            Path = ["address", "street_address"],
            Values = ["123 Main St"]
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
    public void AuthorizationRequest_CreateCrossDeviceWithDcql_CreatesValidRequest()
    {
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
    public void AuthorizationRequest_Validate_WithDcqlOnly_Succeeds()
    {
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "test-nonce",
            DcqlQuery = CreateValidDcqlQuery()
        };

        var ex = Record.Exception(() => request.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void AuthorizationRequest_Validate_WithBothDcqlAndPresentationDefinition_Throws()
    {
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "test-nonce",
            DcqlQuery = CreateValidDcqlQuery(),
            PresentationDefinition = PresentationDefinition.CreateSimple("test", "TestCredential")
        };

        Assert.Throws<InvalidOperationException>(() => request.Validate());
    }

    [Fact]
    public void AuthorizationRequest_Validate_WithNeitherDcqlNorPresentationDef_Throws()
    {
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "test-nonce"
        };

        Assert.Throws<InvalidOperationException>(() => request.Validate());
    }

    [Fact]
    public void AuthorizationRequest_NewParameters_DefaultToNull()
    {
        var request = new AuthorizationRequest();

        Assert.Null(request.DcqlQuery);
        Assert.Null(request.RequestUriMethod);
        Assert.Null(request.TransactionData);
        Assert.Null(request.WalletNonce);
        Assert.Null(request.VerifierInfo);
    }

    [Fact]
    public void AuthorizationRequest_TransactionData_SerializesAsArray()
    {
        var request = AuthorizationRequest.CreateCrossDeviceWithDcql(
            "https://verifier.example.com",
            "https://verifier.example.com/callback",
            "test-nonce",
            CreateValidDcqlQuery());
        request.TransactionData = ["eyJ0eXBlIjoicGF5bWVudCJ9", "eyJ0eXBlIjoiY29uc2VudCJ9"];

        var json = JsonSerializer.Serialize(request);

        Assert.Contains("\"transaction_data\"", json);
        Assert.Contains("[", json);
        Assert.Contains("eyJ0eXBlIjoicGF5bWVudCJ9", json);
    }

    [Fact]
    public void AuthorizationRequest_VerifierInfo_SerializesAsArray()
    {
        var request = AuthorizationRequest.CreateCrossDeviceWithDcql(
            "https://verifier.example.com",
            "https://verifier.example.com/callback",
            "test-nonce",
            CreateValidDcqlQuery());
        request.VerifierInfo =
        [
            new VerifierInfo
            {
                Format = "jwt",
                Data = "eyJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJ2ZXJpZmllciJ9.signature",
                CredentialIds = ["pid"]
            }
        ];

        var json = JsonSerializer.Serialize(request);

        Assert.Contains("\"verifier_info\"", json);
        Assert.Contains("\"format\":\"jwt\"", json);
        Assert.Contains("\"credential_ids\"", json);
    }

    [Fact]
    public void AuthorizationResponse_GetDcqlVpTokens_WithDictionary_ReturnsEntries()
    {
        var response = AuthorizationResponse.SuccessWithDcql(new Dictionary<string, string[]>
        {
            ["pid"] = ["token-1", "token-2"]
        });

        var tokens = response.GetDcqlVpTokens();

        Assert.True(response.HasVpTokens);
        Assert.Single(tokens);
        Assert.Equal(["token-1", "token-2"], tokens["pid"]);
    }

    [Fact]
    public void VpTokenValidator_DcqlResponse_WithSatisfiedAlternativeCredentialSet_Succeeds()
    {
        var query = new DcqlQuery
        {
            Credentials = [CreateValidCredentialQuery("mdl"), CreateValidCredentialQuery("pid")],
            CredentialSets =
            [
                new DcqlCredentialSetQuery
                {
                    Options = [["mdl"], ["pid"]]
                }
            ]
        };
        var response = new Dictionary<string, string[]> { ["pid"] = ["token"] };

        var result = InvokeValidateDcqlResponse(response, query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void VpTokenValidator_DcqlResponse_WithUnsatisfiedAndCredentialSet_Fails()
    {
        var query = new DcqlQuery
        {
            Credentials = [CreateValidCredentialQuery("mdl"), CreateValidCredentialQuery("pid")],
            CredentialSets =
            [
                new DcqlCredentialSetQuery
                {
                    Options = [["mdl", "pid"]]
                }
            ]
        };
        var response = new Dictionary<string, string[]> { ["pid"] = ["token"] };

        var result = InvokeValidateDcqlResponse(response, query);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void VpTokenValidator_DcqlCredentialClaims_WithMatchingVctAndClaim_Succeeds()
    {
        var query = CreateValidCredentialQuery("pid");
        query.Claims =
        [
            new DcqlClaimsQuery { Path = ["given_name"], Values = ["Alice"] }
        ];
        var claims = new Dictionary<string, object>
        {
            ["vct"] = "https://credentials.example.com/identity_credential",
            ["given_name"] = "Alice"
        };

        var result = InvokeValidateDcqlCredentialClaims(query, claims);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void VpTokenValidator_DcqlCredentialClaims_WithMissingClaim_Fails()
    {
        var query = CreateValidCredentialQuery("pid");
        query.Claims =
        [
            new DcqlClaimsQuery { Path = ["given_name"] }
        ];
        var claims = new Dictionary<string, object>
        {
            ["vct"] = "https://credentials.example.com/identity_credential"
        };

        var result = InvokeValidateDcqlCredentialClaims(query, claims);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AuthorizationRequest_RequestUriMethod_Serializes()
    {
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
    public void Oid4VpConstants_RequestUriMethods_HaveExpectedValues()
    {
        Assert.Equal("get", Oid4VpConstants.RequestUriMethods.Get);
        Assert.Equal("post", Oid4VpConstants.RequestUriMethods.Post);
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    private static DcqlCredentialQuery CreateValidCredentialQuery(string id) =>
            new DcqlCredentialQuery
            {
                Id = id,
                Format = Oid4VpConstants.SdJwtVcFormat,
                Meta = new SdJwtVcMeta
                {
                    VctValues = ["https://credentials.example.com/identity_credential"]
                }
            };

    private static DcqlQuery CreateValidDcqlQuery() =>
            new DcqlQuery
            {
                Credentials = [CreateValidCredentialQuery("pid")]
            };

    private static CustomValidationResult InvokeValidateDcqlResponse(
        Dictionary<string, string[]> dcqlVpTokens,
        DcqlQuery expectedQuery)
    {
        var method = typeof(VpTokenValidator).GetMethod(
            "ValidateDcqlResponse",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return (CustomValidationResult)method.Invoke(null, [dcqlVpTokens, expectedQuery])!;
    }

    private static CustomValidationResult InvokeValidateDcqlCredentialClaims(
        DcqlCredentialQuery query,
        Dictionary<string, object> claims)
    {
        var method = typeof(VpTokenValidator).GetMethod(
            "ValidateDcqlCredentialClaims",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return (CustomValidationResult)method.Invoke(null, [query, claims])!;
    }
}
