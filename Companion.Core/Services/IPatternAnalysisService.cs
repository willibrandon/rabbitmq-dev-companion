using Companion.Core.Models;

namespace Companion.Core.Services;

/// <summary>
/// Service for analyzing RabbitMQ topology patterns and providing recommendations
/// </summary>
public interface IPatternAnalysisService
{
    /// <summary>
    /// Analyzes a topology for potential issues, anti-patterns, and areas of improvement
    /// </summary>
    /// <param name="topology">The topology to analyze</param>
    /// <returns>Analysis results containing findings and recommendations</returns>
    AnalysisResult AnalyzeTopology(Topology topology);
} 