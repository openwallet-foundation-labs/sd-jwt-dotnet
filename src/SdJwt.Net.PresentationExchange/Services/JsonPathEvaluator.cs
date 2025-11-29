using System.Text.Json;
using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;
using System.Text.RegularExpressions;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Evaluates JSON path expressions against credential data.
/// Provides simplified JSON path evaluation for presentation exchange scenarios.
/// </summary>
public class JsonPathEvaluator
{
    private readonly ILogger<JsonPathEvaluator> _logger;
    private static readonly Regex JsonPathRegex = new Regex(@"^\$(\.[a-zA-Z_][a-zA-Z0-9_]*|\[\d+\]|\['\w+'\]|\[""[^""]+\""\])*$", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the JsonPathEvaluator class.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public JsonPathEvaluator(ILogger<JsonPathEvaluator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates a JSON path expression against a JSON document.
    /// </summary>
    /// <param name="jsonDocument">The JSON document to query</param>
    /// <param name="jsonPath">The JSON path expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the path evaluation result</returns>
    public async Task<JsonPathEvaluationResult> EvaluateAsync(
        JsonDocument jsonDocument,
        string jsonPath,
        CancellationToken cancellationToken = default)
    {
        var result = new JsonPathEvaluationResult
        {
            JsonPath = jsonPath
        };

        try
        {
            _logger.LogDebug("Evaluating JSON path: {JsonPath}", jsonPath);

            // Validate JSON path format
            if (!IsValidJsonPath(jsonPath))
            {
                result.AddError("INVALID_JSON_PATH", $"Invalid JSON path format: {jsonPath}");
                return result;
            }

            // Start evaluation from root
            var values = new List<JsonElement>();
            await EvaluatePathRecursiveAsync(jsonDocument.RootElement, jsonPath, values, cancellationToken);

            result.Values = values.ToArray();
            result.IsSuccessful = values.Any();

            if (result.IsSuccessful)
            {
                _logger.LogDebug("JSON path evaluation successful. Path: {JsonPath}, Values found: {Count}", 
                    jsonPath, values.Count);
            }
            else
            {
                _logger.LogDebug("JSON path evaluation found no values. Path: {JsonPath}", jsonPath);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("JSON path evaluation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating JSON path: {JsonPath}", jsonPath);
            result.AddError("PATH_EVALUATION_ERROR", $"JSON path evaluation failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Recursively evaluates a JSON path against a JSON element.
    /// </summary>
    /// <param name="currentElement">The current JSON element</param>
    /// <param name="remainingPath">The remaining path to evaluate</param>
    /// <param name="values">The list to collect matching values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the recursive evaluation</returns>
    private async Task EvaluatePathRecursiveAsync(
        JsonElement currentElement,
        string remainingPath,
        List<JsonElement> values,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(remainingPath) || remainingPath == "$")
        {
            values.Add(currentElement);
            return;
        }

        // Remove leading $ if present
        if (remainingPath.StartsWith("$"))
            remainingPath = remainingPath.Substring(1);

        if (string.IsNullOrEmpty(remainingPath))
        {
            values.Add(currentElement);
            return;
        }

        // Parse the next path segment
        var (nextSegment, nextRemainingPath) = ParseNextPathSegment(remainingPath);

        switch (nextSegment.Type)
        {
            case PathSegmentType.Property:
                await EvaluatePropertyAccessAsync(currentElement, nextSegment.Value!, nextRemainingPath, values, cancellationToken);
                break;

            case PathSegmentType.ArrayIndex:
                await EvaluateArrayIndexAsync(currentElement, nextSegment.Index!.Value, nextRemainingPath, values, cancellationToken);
                break;

            case PathSegmentType.ArrayAll:
                await EvaluateArrayAllAsync(currentElement, nextRemainingPath, values, cancellationToken);
                break;

            case PathSegmentType.Wildcard:
                await EvaluateWildcardAsync(currentElement, nextRemainingPath, values, cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Evaluates property access on a JSON object.
    /// </summary>
    private async Task EvaluatePropertyAccessAsync(
        JsonElement currentElement,
        string propertyName,
        string remainingPath,
        List<JsonElement> values,
        CancellationToken cancellationToken)
    {
        if (currentElement.ValueKind == JsonValueKind.Object && currentElement.TryGetProperty(propertyName, out var property))
        {
            await EvaluatePathRecursiveAsync(property, remainingPath, values, cancellationToken);
        }
    }

    /// <summary>
    /// Evaluates array index access.
    /// </summary>
    private async Task EvaluateArrayIndexAsync(
        JsonElement currentElement,
        int index,
        string remainingPath,
        List<JsonElement> values,
        CancellationToken cancellationToken)
    {
        if (currentElement.ValueKind == JsonValueKind.Array)
        {
            var arrayLength = currentElement.GetArrayLength();
            
            // Handle negative indices (from end)
            if (index < 0)
                index = arrayLength + index;

            if (index >= 0 && index < arrayLength)
            {
                var element = currentElement[index];
                await EvaluatePathRecursiveAsync(element, remainingPath, values, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Evaluates access to all array elements.
    /// </summary>
    private async Task EvaluateArrayAllAsync(
        JsonElement currentElement,
        string remainingPath,
        List<JsonElement> values,
        CancellationToken cancellationToken)
    {
        if (currentElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in currentElement.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await EvaluatePathRecursiveAsync(element, remainingPath, values, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Evaluates wildcard access (all properties or array elements).
    /// </summary>
    private async Task EvaluateWildcardAsync(
        JsonElement currentElement,
        string remainingPath,
        List<JsonElement> values,
        CancellationToken cancellationToken)
    {
        switch (currentElement.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in currentElement.EnumerateObject())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await EvaluatePathRecursiveAsync(property.Value, remainingPath, values, cancellationToken);
                }
                break;

            case JsonValueKind.Array:
                await EvaluateArrayAllAsync(currentElement, remainingPath, values, cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Parses the next segment of a JSON path.
    /// </summary>
    /// <param name="path">The path to parse</param>
    /// <returns>The next segment and remaining path</returns>
    private (PathSegment segment, string remainingPath) ParseNextPathSegment(string path)
    {
        if (string.IsNullOrEmpty(path))
            return (new PathSegment { Type = PathSegmentType.Property, Value = "" }, "");

        // Property access: .property
        if (path.StartsWith("."))
        {
            var dotIndex = path.IndexOf('.', 1);
            var bracketIndex = path.IndexOf('[', 1);
            
            var endIndex = -1;
            if (dotIndex == -1 && bracketIndex == -1)
                endIndex = path.Length;
            else if (dotIndex == -1)
                endIndex = bracketIndex;
            else if (bracketIndex == -1)
                endIndex = dotIndex;
            else
                endIndex = Math.Min(dotIndex, bracketIndex);

            var propertyName = path.Substring(1, endIndex - 1);
            var remainingPath = endIndex == path.Length ? "" : path.Substring(endIndex);

            return (new PathSegment { Type = PathSegmentType.Property, Value = propertyName }, remainingPath);
        }

        // Array access: [index] or ['property'] or ["property"]
        if (path.StartsWith("["))
        {
            var closingBracketIndex = path.IndexOf(']');
            if (closingBracketIndex == -1)
                throw new ArgumentException($"Invalid JSON path: unclosed bracket in '{path}'");

            var bracketContent = path.Substring(1, closingBracketIndex - 1);
            var remainingPath = closingBracketIndex + 1 == path.Length ? "" : path.Substring(closingBracketIndex + 1);

            // Wildcard: [*]
            if (bracketContent == "*")
            {
                return (new PathSegment { Type = PathSegmentType.Wildcard }, remainingPath);
            }

            // Quoted property name: ['property'] or ["property"]
            if ((bracketContent.StartsWith("'") && bracketContent.EndsWith("'")) ||
                (bracketContent.StartsWith("\"") && bracketContent.EndsWith("\"")))
            {
                var propertyName = bracketContent.Substring(1, bracketContent.Length - 2);
                return (new PathSegment { Type = PathSegmentType.Property, Value = propertyName }, remainingPath);
            }

            // Array index: [0], [1], etc.
            if (int.TryParse(bracketContent, out var index))
            {
                return (new PathSegment { Type = PathSegmentType.ArrayIndex, Index = index }, remainingPath);
            }

            throw new ArgumentException($"Invalid JSON path: invalid bracket content '{bracketContent}'");
        }

        // If we get here, treat as a property name without leading dot
        var nextDotIndex = path.IndexOf('.');
        var nextBracketIndex = path.IndexOf('[');
        
        var nextEndIndex = -1;
        if (nextDotIndex == -1 && nextBracketIndex == -1)
            nextEndIndex = path.Length;
        else if (nextDotIndex == -1)
            nextEndIndex = nextBracketIndex;
        else if (nextBracketIndex == -1)
            nextEndIndex = nextDotIndex;
        else
            nextEndIndex = Math.Min(nextDotIndex, nextBracketIndex);

        var propName = path.Substring(0, nextEndIndex);
        var nextRemainingPath = nextEndIndex == path.Length ? "" : path.Substring(nextEndIndex);

        return (new PathSegment { Type = PathSegmentType.Property, Value = propName }, nextRemainingPath);
    }

    /// <summary>
    /// Validates if a string is a valid JSON path.
    /// </summary>
    /// <param name="jsonPath">The path to validate</param>
    /// <returns>True if the path is valid</returns>
    private bool IsValidJsonPath(string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
            return false;

        // Must start with $
        if (!jsonPath.StartsWith("$"))
            return false;

        // Simple validation - more comprehensive validation would be done during parsing
        try
        {
            // Try to parse the path to see if it's well-formed
            var testPath = jsonPath;
            if (testPath == "$")
                return true;

            // Remove $ and validate segments
            testPath = testPath.Substring(1);
            
            while (!string.IsNullOrEmpty(testPath))
            {
                var (segment, remaining) = ParseNextPathSegment(testPath);
                testPath = remaining;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Represents a segment of a JSON path.
/// </summary>
internal class PathSegment
{
    public PathSegmentType Type { get; set; }
    public string? Value { get; set; }
    public int? Index { get; set; }
}

/// <summary>
/// Types of JSON path segments.
/// </summary>
internal enum PathSegmentType
{
    Property,
    ArrayIndex,
    ArrayAll,
    Wildcard
}

/// <summary>
/// Represents the result of JSON path evaluation.
/// </summary>
public class JsonPathEvaluationResult
{
    /// <summary>
    /// Gets or sets the JSON path that was evaluated.
    /// </summary>
    public string JsonPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the evaluation was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Gets or sets the values found by the JSON path evaluation.
    /// </summary>
    public JsonElement[] Values { get; set; } = Array.Empty<JsonElement>();

    /// <summary>
    /// Gets or sets any errors that occurred during evaluation.
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Adds an error to the result.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public void AddError(string errorCode, string message)
    {
        Errors.Add($"{errorCode}: {message}");
        IsSuccessful = false;
    }

    /// <summary>
    /// Gets the first value if available.
    /// </summary>
    /// <returns>The first value or null if none available</returns>
    public JsonElement? GetFirstValue()
    {
        return Values.FirstOrDefault();
    }

    /// <summary>
    /// Gets all values as strings.
    /// </summary>
    /// <returns>Array of string values</returns>
    public string[] GetStringValues()
    {
        return Values.Where(v => v.ValueKind == JsonValueKind.String)
                    .Select(v => v.GetString() ?? "")
                    .ToArray();
    }

    /// <summary>
    /// Gets all values as numbers.
    /// </summary>
    /// <returns>Array of numeric values</returns>
    public double[] GetNumericValues()
    {
        return Values.Where(v => v.ValueKind == JsonValueKind.Number)
                    .Select(v => v.GetDouble())
                    .ToArray();
    }
}
