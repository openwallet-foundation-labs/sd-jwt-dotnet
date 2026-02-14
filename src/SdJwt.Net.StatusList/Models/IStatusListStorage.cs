namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Storage abstraction interface for Status List persistence with optimistic concurrency control.
/// Implements ETag-based versioning to prevent race conditions in multi-user environments.
/// </summary>
public interface IStatusListStorage
{
    /// <summary>
    /// Retrieves a Status List Token along with its version (ETag) for optimistic concurrency control.
    /// </summary>
    /// <param name="listId">The unique identifier of the status list.</param>
    /// <returns>A tuple containing the status list token and its ETag version.</returns>
    Task<(string? token, string etag)> GetStatusListWithETagAsync(string listId);

    /// <summary>
    /// Attempts to save a Status List Token with optimistic concurrency control.
    /// </summary>
    /// <param name="listId">The unique identifier of the status list.</param>
    /// <param name="token">The Status List Token to save.</param>
    /// <param name="expectedETag">The expected ETag version for optimistic concurrency.</param>
    /// <returns>A tuple indicating success and the new ETag if successful.</returns>
    Task<(bool success, string newETag)> TrySaveStatusListAsync(string listId, string token, string expectedETag);

    /// <summary>
    /// Retrieves a Status List Token without version information (legacy support).
    /// </summary>
    /// <param name="listId">The unique identifier of the status list.</param>
    /// <returns>The Status List Token if found, null otherwise.</returns>
    Task<string?> GetStatusListAsync(string listId);

    /// <summary>
    /// Saves a Status List Token without concurrency control (legacy support).
    /// Use TrySaveStatusListAsync for production environments.
    /// </summary>
    /// <param name="listId">The unique identifier of the status list.</param>
    /// <param name="token">The Status List Token to save.</param>
    Task SaveStatusListAsync(string listId, string token);
}

/// <summary>
/// Exception thrown when optimistic concurrency conflicts occur during status list updates.
/// </summary>
public class ConcurrencyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class.
    /// </summary>
    /// <param name="message">The error message explaining the concurrency conflict.</param>
    public ConcurrencyException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class.
    /// </summary>
    /// <param name="message">The error message explaining the concurrency conflict.</param>
    /// <param name="innerException">The inner exception that caused the conflict.</param>
    public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}