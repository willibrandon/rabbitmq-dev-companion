namespace Companion.Core.Models;

/// <summary>
/// Represents the result of a topology validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the topology is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets optional warnings that don't invalidate the topology but should be considered
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with the specified errors
    /// </summary>
    public static ValidationResult Failed(params string[] errors) => 
        new() { IsValid = false, Errors = errors.ToList() };

    /// <summary>
    /// Creates a successful validation result with warnings
    /// </summary>
    public static ValidationResult SuccessWithWarnings(params string[] warnings) => 
        new() { IsValid = true, Warnings = warnings.ToList() };
} 