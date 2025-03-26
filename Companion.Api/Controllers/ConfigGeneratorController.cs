using Companion.Core.Models;
using Companion.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Companion.Api.Controllers;

/// <summary>
/// API endpoints for generating configuration files
/// </summary>
[ApiController]
[Route("api/config-generator")]
public class ConfigGeneratorController : ControllerBase
{
    private readonly IConfigGeneratorService _configGeneratorService;
    private readonly ITopologyService _topologyService;

    /// <summary>
    /// Initializes a new instance of the ConfigGeneratorController class
    /// </summary>
    public ConfigGeneratorController(
        IConfigGeneratorService configGeneratorService,
        ITopologyService topologyService)
    {
        _configGeneratorService = configGeneratorService;
        _topologyService = topologyService;
    }

    /// <summary>
    /// Generates configuration files for a topology
    /// </summary>
    /// <param name="topologyId">The ID of the topology to generate configuration for</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>The generated configuration files</returns>
    [HttpPost("{topologyId}")]
    [ProducesResponseType(typeof(ConfigurationOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigurationOutput>> GenerateConfiguration(
        string topologyId,
        [FromBody] ConfigurationOptions? options = null)
    {
        var topology = await _topologyService.GetTopologyByIdAsync(topologyId);
        if (topology == null)
        {
            return NotFound($"Topology with ID {topologyId} not found");
        }

        var output = await _configGeneratorService.GenerateConfigurationAsync(topology, options);
        return Ok(output);
    }
} 