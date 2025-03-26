using Companion.Simulator.Models;
using Microsoft.AspNetCore.SignalR;

namespace Companion.Simulator.Hubs;

/// <summary>
/// SignalR hub for real-time simulation updates
/// </summary>
public class SimulationHub : Hub
{
    /// <summary>
    /// Sends a simulation status update to all connected clients
    /// </summary>
    /// <param name="status">The current simulation status</param>
    public async Task SendSimulationUpdate(SimulationStatus status)
    {
        await Clients.All.SendAsync("SimulationUpdate", status);
    }

    /// <summary>
    /// Sends a simulation error to all connected clients
    /// </summary>
    /// <param name="simulationId">The simulation ID</param>
    /// <param name="error">The error message</param>
    public async Task SendSimulationError(string simulationId, string error)
    {
        await Clients.All.SendAsync("SimulationError", simulationId, error);
    }
} 