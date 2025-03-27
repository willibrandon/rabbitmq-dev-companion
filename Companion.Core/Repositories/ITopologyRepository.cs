using Companion.Core.Models;

namespace Companion.Core.Repositories;

/// <summary>
/// Repository for managing topology persistence
/// </summary>
public interface ITopologyRepository
{
    /// <summary>
    /// Gets a topology by its ID
    /// </summary>
    /// <param name="topologyId">The ID of the topology to retrieve</param>
    /// <returns>The topology if found, null otherwise</returns>
    Task<Topology?> GetByIdAsync(string topologyId);

    /// <summary>
    /// Creates a new topology or updates an existing one
    /// </summary>
    /// <param name="topology">The topology to create or update</param>
    /// <returns>The created or updated topology</returns>
    Task<Topology> CreateOrUpdateAsync(Topology topology);

    /// <summary>
    /// Deletes a topology
    /// </summary>
    /// <param name="topologyId">The ID of the topology to delete</param>
    /// <returns>True if the topology was deleted, false if it wasn't found</returns>
    Task<bool> DeleteAsync(string topologyId);

    /// <summary>
    /// Gets all topologies
    /// </summary>
    /// <returns>A list of all topologies</returns>
    Task<IEnumerable<Topology>> GetAllAsync();
} 