using System.Text.Json.Serialization;

namespace Companion.Learning.Models;

/// <summary>
/// Represents a learning module with steps and validation criteria
/// </summary>
public class LearningModule
{
    /// <summary>
    /// Gets or sets the unique identifier for the module
    /// </summary>
    public string ModuleId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the module
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the module
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the difficulty level of the module
    /// </summary>
    public DifficultyLevel Difficulty { get; set; }

    /// <summary>
    /// Gets or sets the ordered list of steps in the module
    /// </summary>
    public List<LearningStep> Steps { get; set; } = new();

    /// <summary>
    /// Gets or sets the prerequisites for this module (other module IDs)
    /// </summary>
    public List<string> Prerequisites { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimated time to complete the module (in minutes)
    /// </summary>
    public int EstimatedTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with this module
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Represents a single step in a learning module
/// </summary>
public class LearningStep
{
    /// <summary>
    /// Gets or sets the title of the step
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the step
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation criteria for completing this step
    /// </summary>
    public string ValidationCriteria { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any code snippets associated with this step
    /// </summary>
    public string? CodeSnippet { get; set; }

    /// <summary>
    /// Gets or sets any hints for completing this step
    /// </summary>
    public List<string> Hints { get; set; } = new();
}

/// <summary>
/// Represents the difficulty level of a learning module
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DifficultyLevel
{
    /// <summary>
    /// Beginner level, suitable for those new to RabbitMQ
    /// </summary>
    Beginner,

    /// <summary>
    /// Intermediate level, requires basic RabbitMQ knowledge
    /// </summary>
    Intermediate,

    /// <summary>
    /// Advanced level, covers complex patterns and scenarios
    /// </summary>
    Advanced
} 