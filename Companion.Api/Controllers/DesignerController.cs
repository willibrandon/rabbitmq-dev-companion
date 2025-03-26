using Companion.Core.Models;
using Companion.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Companion.Api.Controllers;

/// <summary>
/// API endpoints for the topology designer
/// </summary>
[ApiController]
[Route("api/designer")]
public class DesignerController : ControllerBase
{
    private readonly ITopologyService _topologyService;

    /// <summary>
    /// Initializes a new instance of the DesignerController class
    /// </summary>
    public DesignerController(ITopologyService topologyService)
    {
        _topologyService = topologyService;
    }

    /// <summary>
    /// Validates a topology
    /// </summary>
    /// <param name="topology">The topology to validate</param>
    /// <returns>A validation result indicating any errors or warnings</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ValidationResult> ValidateTopology([FromBody] Topology topology)
    {
        if (topology == null)
        {
            return BadRequest("Topology cannot be null");
        }

        var result = _topologyService.ValidateTopology(topology);
        return Ok(result);
    }

    /// <summary>
    /// Normalizes a topology by ensuring consistent naming and formatting
    /// </summary>
    /// <param name="topology">The topology to normalize</param>
    /// <returns>The normalized topology</returns>
    [HttpPost("normalize")]
    [ProducesResponseType(typeof(Topology), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Topology> NormalizeTopology([FromBody] Topology topology)
    {
        if (topology == null)
        {
            return BadRequest("Topology cannot be null");
        }

        var normalizedTopology = _topologyService.NormalizeTopology(topology);
        return Ok(normalizedTopology);
    }
} 