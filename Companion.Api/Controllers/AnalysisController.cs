using Companion.Core.Models;
using Companion.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Companion.Api.Controllers;

/// <summary>
/// API endpoints for analyzing RabbitMQ topologies
/// </summary>
[ApiController]
[Route("api/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly IPatternAnalysisService _patternAnalysisService;

    /// <summary>
    /// Initializes a new instance of the AnalysisController class
    /// </summary>
    public AnalysisController(IPatternAnalysisService patternAnalysisService)
    {
        _patternAnalysisService = patternAnalysisService;
    }

    /// <summary>
    /// Analyzes a topology for potential issues and recommendations
    /// </summary>
    /// <param name="topology">The topology to analyze</param>
    /// <returns>Analysis results containing findings and recommendations</returns>
    [HttpPost("run")]
    [ProducesResponseType(typeof(AnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<AnalysisResult> AnalyzeTopology([FromBody] Topology topology)
    {
        if (topology == null)
        {
            return BadRequest("Topology cannot be null");
        }

        var result = _patternAnalysisService.AnalyzeTopology(topology);
        return Ok(result);
    }
} 