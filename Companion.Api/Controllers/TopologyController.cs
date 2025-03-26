using Companion.Core.Models;
using Companion.Infrastructure.RabbitMq;
using Microsoft.AspNetCore.Mvc;

namespace Companion.Api.Controllers;

/// <summary>
/// API endpoints for managing RabbitMQ topologies
/// </summary>
[ApiController]
[Route("api/topologies")]
public class TopologyController : ControllerBase
{
    private readonly IRabbitMqManagementClient _rabbitMqClient;

    /// <summary>
    /// Initializes a new instance of the TopologyController class
    /// </summary>
    public TopologyController(IRabbitMqManagementClient rabbitMqClient)
    {
        _rabbitMqClient = rabbitMqClient;
    }

    /// <summary>
    /// Gets the current topology from the RabbitMQ broker
    /// </summary>
    /// <returns>The current topology</returns>
    [HttpGet("from-broker")]
    [ProducesResponseType(typeof(Topology), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<Topology>> GetTopologyFromBroker(CancellationToken cancellationToken)
    {
        var isHealthy = await _rabbitMqClient.GetHealthStatusAsync(cancellationToken);
        if (!isHealthy)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "RabbitMQ broker is not available");
        }

        var topology = await _rabbitMqClient.GetTopologyFromBrokerAsync(cancellationToken);
        return Ok(topology);
    }

    /// <summary>
    /// Gets the health status of the RabbitMQ broker
    /// </summary>
    /// <returns>200 OK if healthy, 503 Service Unavailable if not</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealthStatus(CancellationToken cancellationToken)
    {
        var isHealthy = await _rabbitMqClient.GetHealthStatusAsync(cancellationToken);
        return isHealthy ? Ok() : StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
} 