using Companion.Core.Models;
using Companion.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Companion.Api.Controllers;

/// <summary>
/// API endpoints for managing RabbitMQ topologies
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class TopologyController : ControllerBase
{
    private readonly ITopologyService _topologyService;

    /// <summary>
    /// Initializes a new instance of the TopologyController class
    /// </summary>
    public TopologyController(ITopologyService topologyService)
    {
        _topologyService = topologyService;
    }

    /// <summary>
    /// Gets the current topology from the RabbitMQ broker
    /// </summary>
    /// <returns>The current topology</returns>
    [HttpGet("from-broker")]
    [ProducesResponseType(typeof(Topology), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Topology>> GetFromBroker()
    {
        var topology = await _topologyService.GetFromBrokerAsync();
        return Ok(topology);
    }

    /// <summary>
    /// Creates a new topology
    /// </summary>
    /// <param name="topology">The topology to create</param>
    /// <returns>The created topology</returns>
    [HttpPost]
    [Authorize(Policy = "RequireEditorRole")] // Only editors can create/update topologies
    [ProducesResponseType(typeof(Topology), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Topology>> CreateTopology([FromBody] Topology topology)
    {
        var result = await _topologyService.SaveTopologyAsync(topology);
        return CreatedAtAction(nameof(GetFromBroker), result);
    }

    /// <summary>
    /// Gets the health status of the RabbitMQ broker
    /// </summary>
    /// <returns>200 OK if healthy, 503 Service Unavailable if not</returns>
    [HttpGet("health")]
    [AllowAnonymous] // Allow health check without authentication
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckHealth()
    {
        var isHealthy = await _topologyService.CheckBrokerHealthAsync();
        return isHealthy ? Ok() : StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
} 