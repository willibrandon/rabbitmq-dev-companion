using Companion.Simulator.Models;
using Companion.Simulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace Companion.Api.Controllers;

/// <summary>
/// API endpoints for managing message flow simulations
/// </summary>
[ApiController]
[Route("api/simulations")]
public class SimulationsController : ControllerBase
{
    private readonly IMessageFlowService _messageFlowService;

    /// <summary>
    /// Initializes a new instance of the SimulationsController class
    /// </summary>
    public SimulationsController(IMessageFlowService messageFlowService)
    {
        _messageFlowService = messageFlowService;
    }

    /// <summary>
    /// Starts a new simulation
    /// </summary>
    /// <param name="config">The simulation configuration</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The simulation ID</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> StartSimulation(
        SimulationConfig config,
        CancellationToken cancellationToken)
    {
        try
        {
            var simulationId = await _messageFlowService.StartSimulationAsync(config, cancellationToken);
            return Ok(simulationId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Stops a running simulation
    /// </summary>
    /// <param name="simulationId">The simulation ID to stop</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    [HttpPost("{simulationId}/stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StopSimulation(
        string simulationId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _messageFlowService.StopSimulationAsync(simulationId, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Gets the current status of a simulation
    /// </summary>
    /// <param name="simulationId">The simulation ID to check</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The current simulation status</returns>
    [HttpGet("{simulationId}/status")]
    [ProducesResponseType(typeof(SimulationStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulationStatus>> GetSimulationStatus(
        string simulationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var status = await _messageFlowService.GetSimulationStatusAsync(simulationId, cancellationToken);
            return Ok(status);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
} 