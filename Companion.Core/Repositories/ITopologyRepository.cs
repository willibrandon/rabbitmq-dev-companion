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
    /// <param name="id">The ID of the topology to retrieve</param>
    /// <returns>The topology if found, null otherwise</returns>
    Task<Topology?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all topologies
    /// </summary>
    /// <returns>A list of all topologies</returns>
    Task<IEnumerable<Topology>> GetAllAsync();

    /// <summary>
    /// Creates a new topology
    /// </summary>
    /// <param name="topology">The topology to create</param>
    /// <returns>The created topology with its assigned ID</returns>
    Task<Topology> CreateAsync(Topology topology);

    /// <summary>
    /// Updates an existing topology
    /// </summary>
    /// <param name="id">The ID of the topology to update</param>
    /// <param name="topology">The updated topology data</param>
    /// <returns>The updated topology</returns>
    Task<Topology> UpdateAsync(string id, Topology topology);

    /// <summary>
    /// Deletes a topology
    /// </summary>
    /// <param name="id">The ID of the topology to delete</param>
    /// <returns>True if the topology was deleted, false if it wasn't found</returns>
    Task<bool> DeleteAsync(string id);
} 