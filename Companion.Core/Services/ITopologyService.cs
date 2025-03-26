using Companion.Core.Models;

namespace Companion.Core.Services;

/// <summary>
/// Service for managing and validating RabbitMQ topologies
/// </summary>
public interface ITopologyService
{
    /// <summary>
    /// Validates a topology for correctness and consistency
    /// </summary>
    /// <param name="topology">The topology to validate</param>
    /// <returns>A validation result indicating success or failure with errors</returns>
    ValidationResult ValidateTopology(Topology topology);

    /// <summary>
    /// Normalizes a topology by ensuring consistent naming and formatting
    /// </summary>
    /// <param name="topology">The topology to normalize</param>
    /// <returns>A normalized copy of the topology</returns>
    Topology NormalizeTopology(Topology topology);
} 