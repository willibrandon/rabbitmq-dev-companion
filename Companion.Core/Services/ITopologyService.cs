using Companion.Core.Models;

namespace Companion.Core.Services;

/// <summary>
/// Service for managing RabbitMQ topologies
/// </summary>
public interface ITopologyService
{
    /// <summary>
    /// Validates a topology
    /// </summary>
    /// <param name="topology">The topology to validate</param>
    /// <returns>A validation result indicating any errors or warnings</returns>
    ValidationResult ValidateTopology(Topology topology);

    /// <summary>
    /// Normalizes a topology by ensuring consistent naming and formatting
    /// </summary>
    /// <param name="topology">The topology to normalize</param>
    /// <returns>The normalized topology</returns>
    Topology NormalizeTopology(Topology topology);

    /// <summary>
    /// Gets a topology by its ID
    /// </summary>
    /// <param name="topologyId">The ID of the topology to retrieve</param>
    /// <returns>The topology if found, null otherwise</returns>
    Task<Topology?> GetTopologyByIdAsync(string topologyId);
} 