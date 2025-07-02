using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;

namespace SdJwt.Net.Tests;

public class EndToEndTests : TestBase
{
    //[Fact]
    //public async Task FullFlow_WithRecursionAndKeyBinding_VerifiesSuccessfully()
    //{
    //    // 1. ISSUER
    //    var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
    //    var claims = new JwtPayload
    //    {
    //        { "iss", TrustedIssuer },
    //        { "sub", "user123" },
    //        { "verified_claims", new
    //            {
    //                verification = new {
    //                    trust_framework = "eidas",
    //                    evidence = new {
    //                        type = "id_document",
    //                        document = new {
    //                            number = "123456789",
    //                            country = "DE"
    //                        }
    //                    }
    //                },
    //                claims = new {
    //                    given_name = "Jane",
    //                    family_name = "Doe",
    //                    birthdate = "1990-01-01"
    //                }
    //            }
    //        }
    //    };
    //    var options = new SdIssuanceOptions
    //    {
    //        DisclosureStructure = new
    //        {
    //            verified_claims = new
    //            {
    //                claims = new
    //                {
    //                    given_name = true,
    //                    birthdate = true
    //                }
    //            }
    //        }
    //    };
    //    var issuerOutput = issuer.Issue(claims, options, HolderPublicJwk);

    //    // 2. HOLDER
    //    var holder = new SdJwtHolder(issuerOutput.Issuance);
    //    var presentation = holder.CreatePresentation(
    //        d => d.ClaimValue.ToString()!.Contains("Jane"), // Only disclose the given_name
    //        new JwtPayload { { "aud", "verifier" }, { "nonce", "xyz" } },
    //        HolderPrivateKey,
    //        HolderSigningAlgorithm);

    //    // 3. VERIFIER
    //    var verifier = new SdVerifier(_ => Task.FromResult(IssuerSigningKey));
    //    var validationParams = new TokenValidationParameters
    //    {
    //        ValidIssuer = TrustedIssuer,
    //        ValidateAudience = false,
    //        ValidateIssuer = false,
    //    };
    //    var kbValidationParams = new TokenValidationParameters
    //    {
    //        ValidAudience = "verifier",
    //        IssuerSigningKey = HolderPublicKey,
    //        ValidateIssuer = false
    //    };

    //    // Act
    //    var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.True(result.KeyBindingVerified);

    //    var verifiedClaims = result.ClaimsPrincipal.Claims.ToList();

    //    // Check always-visible claims
    //    Assert.Contains(verifiedClaims, c => c.Type == "iss" && c.Value == TrustedIssuer);
    //    Assert.Contains(verifiedClaims, c => c.Type == "sub" && c.Value == "user123");

    //    // Check rehydrated structure
    //    var verifiedClaimsJson = verifiedClaims.First(c => c.Type == "verified_claims").Value;
    //    var vcNode = JsonNode.Parse(verifiedClaimsJson)!.AsObject();

    //    Assert.Equal("DE", vcNode["verification"]!["evidence"]!["document"]!["country"]!.GetValue<string>());

    //    var innerClaims = vcNode["claims"]!.AsObject();

    //    // Check disclosed claim is present
    //    Assert.True(innerClaims.ContainsKey("given_name"));
    //    Assert.Equal("Jane", innerClaims["given_name"]!.GetValue<string>());

    //    // Check non-disclosed claims are absent
    //    Assert.False(innerClaims.ContainsKey("family_name"));
    //    Assert.False(innerClaims.ContainsKey("birthdate"));
    //}
}