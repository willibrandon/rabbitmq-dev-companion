using Companion.Simulator.Models;

namespace Companion.Simulator.Services;

/// <summary>
/// Service for simulating message flows in RabbitMQ
/// </summary>
public interface IMessageFlowService
{
    /// <summary>
    /// Starts a new simulation with the given configuration
    /// </summary>
    /// <param name="config">The simulation configuration</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The simulation ID</returns>
    Task<string> StartSimulationAsync(SimulationConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a running simulation
    /// </summary>
    /// <param name="simulationId">The simulation ID to stop</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task StopSimulationAsync(string simulationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a simulation
    /// </summary>
    /// <param name="simulationId">The simulation ID to check</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The current simulation status</returns>
    Task<SimulationStatus> GetSimulationStatusAsync(string simulationId, CancellationToken cancellationToken = default);
} 