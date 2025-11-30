using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.PresentationExchange.Services;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Services
{
    public class ConstraintEvaluatorTests
    {
        [Fact]
        public async Task EvaluateAsync_WithSdJwtCredential_ShouldFindVctField()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ConstraintEvaluator>>();
            var mockPathLogger = new Mock<ILogger<JsonPathEvaluator>>();
            var mockFilterLogger = new Mock<ILogger<FieldFilterEvaluator>>();
            
            var pathEvaluator = new JsonPathEvaluator(mockPathLogger.Object);
            var filterEvaluator = new FieldFilterEvaluator(mockFilterLogger.Object);
            var evaluator = new ConstraintEvaluator(mockLogger.Object, pathEvaluator, filterEvaluator);
            
            // Create a mock SD-JWT like in the tests
            var payload = new
            {
                iss = "https://example-issuer.com",
                sub = "did:example:123",
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                vct = "DriverLicense",
                _sd_alg = "sha-256",
                name = "John Doe",
                _sd = new[] { "mock-hash-1", "mock-hash-2" }
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var base64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_'); // Convert to base64url
            
            // Use a proper base64url encoded mock signature
            var mockSignature = Convert.ToBase64String(new byte[32]) // 32-byte signature
                .TrimEnd('=').Replace('+', '-').Replace('/', '_'); // Convert to base64url
            
            var sdJwtCredential = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{base64Payload}.{mockSignature}~WyJzYWx0IiwgImJpcnRoRGF0ZSIsICIxOTkwLTAxLTAxIl0~";
            
            // Create constraints for VCT field
            var constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field
                    {
                        Path = new[] { "$.vct" },
                        Filter = new FieldFilter
                        {
                            Type = "string",
                            Const = "DriverLicense"
                        }
                    }
                }
            };

            // Act
            var result = await evaluator.EvaluateAsync(sdJwtCredential, constraints);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccessful)
            {
                var errorDetails = result.Errors.Select(e => $"{e.Code}: {e.Message}").ToArray();
                result.IsSuccessful.Should().BeTrue($"Expected successful evaluation, but got errors: {string.Join("; ", errorDetails)}");
            }
            result.FieldResults.Should().ContainKey("$.vct");
            result.FieldResults["$.vct"].IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public async Task EvaluateAsync_WithDirectJsonObject_ShouldFindVctField()
        {
            // Arrange  
            var mockLogger = new Mock<ILogger<ConstraintEvaluator>>();
            var mockPathLogger = new Mock<ILogger<JsonPathEvaluator>>();
            var mockFilterLogger = new Mock<ILogger<FieldFilterEvaluator>>();
            
            var pathEvaluator = new JsonPathEvaluator(mockPathLogger.Object);
            var filterEvaluator = new FieldFilterEvaluator(mockFilterLogger.Object);
            var evaluator = new ConstraintEvaluator(mockLogger.Object, pathEvaluator, filterEvaluator);
            
            // Create a direct object (like what the JWT would be parsed to)
            var credential = new
            {
                iss = "https://example-issuer.com",
                sub = "did:example:123",
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                vct = "DriverLicense",
                _sd_alg = "sha-256",
                name = "John Doe"
            };
            
            // Create constraints for VCT field
            var constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field
                    {
                        Path = new[] { "$.vct" },
                        Filter = new FieldFilter
                        {
                            Type = "string",
                            Const = "DriverLicense"
                        }
                    }
                }
            };

            // Act
            var result = await evaluator.EvaluateAsync(credential, constraints);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccessful)
            {
                var errorDetails = result.Errors.Select(e => $"{e.Code}: {e.Message}").ToArray();
                result.IsSuccessful.Should().BeTrue($"Expected successful evaluation, but got errors: {string.Join("; ", errorDetails)}");
            }
            result.FieldResults.Should().ContainKey("$.vct");
            result.FieldResults["$.vct"].IsSuccessful.Should().BeTrue();
        }
    }
}
