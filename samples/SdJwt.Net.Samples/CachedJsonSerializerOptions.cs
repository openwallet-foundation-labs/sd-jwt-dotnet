using System.Text.Json;

namespace SdJwt.Net.Samples;

internal static class DefaultOptions
{
    // Cache the JsonSerializerOptions instance to avoid creating a new one for every serialization operation.
    public static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
