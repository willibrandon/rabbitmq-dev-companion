namespace Companion.Core.Common;

/// <summary>
/// Represents the result of an operation, including success status, data, and error information
/// </summary>
/// <typeparam name="T">The type of data returned by the operation</typeparam>
public class OperationResult<T>
{
    /// <summary>
    /// Gets or sets whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the data returned by the operation
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets additional error details if available
    /// </summary>
    public object? ErrorDetails { get; set; }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static OperationResult<T> CreateSuccess(T data) =>
        new() { Success = true, Data = data };

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static OperationResult<T> CreateError(string message, object? details = null) =>
        new() { Success = false, ErrorMessage = message, ErrorDetails = details };
} 