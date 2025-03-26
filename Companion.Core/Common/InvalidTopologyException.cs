namespace Companion.Core.Common;

/// <summary>
/// Exception thrown when a topology validation fails
/// </summary>
public class InvalidTopologyException : Exception
{
    /// <summary>
    /// Gets the validation errors that caused this exception
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the InvalidTopologyException class
    /// </summary>
    public InvalidTopologyException(string message) : base(message)
    {
        ValidationErrors = new[] { message };
    }

    /// <summary>
    /// Initializes a new instance of the InvalidTopologyException class with multiple validation errors
    /// </summary>
    public InvalidTopologyException(IEnumerable<string> validationErrors)
        : base($"Topology validation failed with {validationErrors.Count()} errors")
    {
        ValidationErrors = validationErrors.ToList().AsReadOnly();
    }
} 