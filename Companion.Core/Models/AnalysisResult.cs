using System.Text.Json.Serialization;

namespace Companion.Core.Models;

/// <summary>
/// Represents the result of a topology analysis
/// </summary>
public class AnalysisResult
{
    /// <summary>
    /// List of findings from the analysis
    /// </summary>
    public List<AnalysisFinding> Findings { get; set; } = new();
}

/// <summary>
/// Represents a single finding from the topology analysis
/// </summary>
public class AnalysisFinding
{
    /// <summary>
    /// The type of finding (warning, info, error)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AnalysisFindingType Type { get; set; }

    /// <summary>
    /// The message describing the finding
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The severity level of the finding
    /// </summary>
    public int SeverityLevel { get; set; }

    /// <summary>
    /// Optional recommendations for addressing the finding
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Types of findings in a topology analysis
/// </summary>
public enum AnalysisFindingType
{
    /// <summary>
    /// Informational finding
    /// </summary>
    Info,

    /// <summary>
    /// Warning finding
    /// </summary>
    Warning,

    /// <summary>
    /// Error finding
    /// </summary>
    Error
} 