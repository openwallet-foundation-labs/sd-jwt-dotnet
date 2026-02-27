using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.PresentationExchange.Services;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Services;

public class JsonPathAndFilterAdvancedTests {
        [Fact]
        public async Task JsonPathEvaluator_ShouldSupportRecursiveDescentAndUnion() {
                // Arrange
                var logger = new Mock<ILogger<JsonPathEvaluator>>();
                var evaluator = new JsonPathEvaluator(logger.Object);
                using var document = JsonDocument.Parse("""
                    {
                      "vc": {
                        "credentialSubject": {
                          "name": "Alice",
                          "children": [
                            { "name": "Bob" },
                            { "name": "Charlie" }
                          ]
                        }
                      }
                    }
                    """);

                // Act
                var recursiveResult = await evaluator.EvaluateAsync(document, "$.vc..name");
                var unionResult = await evaluator.EvaluateAsync(document, "$.vc.credentialSubject.children[0,1].name");

                // Assert
                recursiveResult.IsSuccessful.Should().BeTrue();
                recursiveResult.GetStringValues().Should().Contain(new[] { "Alice", "Bob", "Charlie" });
                unionResult.IsSuccessful.Should().BeTrue();
                unionResult.GetStringValues().Should().Contain(new[] { "Bob", "Charlie" });
        }

        [Fact]
        public async Task JsonPathEvaluator_ShouldSupportArraySlices() {
                // Arrange
                var logger = new Mock<ILogger<JsonPathEvaluator>>();
                var evaluator = new JsonPathEvaluator(logger.Object);
                using var document = JsonDocument.Parse("""
                    {
                      "values": [10, 20, 30, 40, 50]
                    }
                    """);

                // Act
                var result = await evaluator.EvaluateAsync(document, "$.values[1:4]");

                // Assert
                result.IsSuccessful.Should().BeTrue();
                result.GetNumericValues().Should().Equal(20, 30, 40);
        }

        [Fact]
        public async Task FieldFilterEvaluator_ShouldValidateObjectPropertiesAndAdditionalProperties() {
                // Arrange
                var logger = new Mock<ILogger<FieldFilterEvaluator>>();
                var evaluator = new FieldFilterEvaluator(logger.Object);
                using var json = JsonDocument.Parse("""
                    {
                      "name": "Alice",
                      "age": 34
                    }
                    """);
                var filter = new FieldFilter {
                        Type = "object",
                        Required = new[] { "name", "age" },
                        AdditionalProperties = false,
                        Properties = new Dictionary<string, object> {
                                ["name"] = new Dictionary<string, object> {
                                        ["type"] = "string",
                                        ["minLength"] = 1
                                },
                                ["age"] = new Dictionary<string, object> {
                                        ["type"] = "integer",
                                        ["minimum"] = 18
                                }
                        }
                };

                // Act
                var successResult = await evaluator.EvaluateAsync(json.RootElement, filter);

                // Assert
                successResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task FieldFilterEvaluator_ShouldValidatePredicateFilters() {
                // Arrange
                var logger = new Mock<ILogger<FieldFilterEvaluator>>();
                var evaluator = new FieldFilterEvaluator(logger.Object);
                using var json = JsonDocument.Parse("25");
                var filter = PredicateFilter.CreateAgeOver(21);

                // Act
                var result = await evaluator.EvaluateAsync(json.RootElement, filter);

                // Assert
                result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task FieldFilterEvaluator_ShouldEvaluateItemsSchema() {
                // Arrange
                var logger = new Mock<ILogger<FieldFilterEvaluator>>();
                var evaluator = new FieldFilterEvaluator(logger.Object);
                using var json = JsonDocument.Parse("[21, 30, 42]");
                var filter = new FieldFilter {
                        Type = "array",
                        Items = new Dictionary<string, object> {
                                ["type"] = "integer",
                                ["minimum"] = 18
                        },
                        MinItems = 1
                };

                // Act
                var result = await evaluator.EvaluateAsync(json.RootElement, filter);

                // Assert
                result.IsSuccessful.Should().BeTrue();
        }
}
