using Companion.Core.Models;
using Companion.Core.Repositories;
using Companion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Companion.Infrastructure.Repositories;

public class TopologyRepository : ITopologyRepository
{
    private readonly CompanionDbContext _dbContext;

    public TopologyRepository(CompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Topology?> GetByIdAsync(string id)
    {
        return await _dbContext.Topologies
            .Include(t => t.Exchanges)
            .Include(t => t.Queues)
            .Include(t => t.Bindings)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Topology>> GetAllAsync()
    {
        return await _dbContext.Topologies
            .Include(t => t.Exchanges)
            .Include(t => t.Queues)
            .Include(t => t.Bindings)
            .ToListAsync();
    }

    public async Task<Topology> CreateAsync(Topology topology)
    {
        _dbContext.Topologies.Add(topology);
        await _dbContext.SaveChangesAsync();
        return topology;
    }

    public async Task<Topology> UpdateAsync(string id, Topology topology)
    {
        var existingTopology = await GetByIdAsync(id) 
            ?? throw new KeyNotFoundException($"Topology with ID {id} not found");

        // Update properties
        existingTopology.Name = topology.Name;
        existingTopology.Description = topology.Description;

        // Update exchanges
        _dbContext.Exchanges.RemoveRange(existingTopology.Exchanges);
        existingTopology.Exchanges = topology.Exchanges;

        // Update queues
        _dbContext.Queues.RemoveRange(existingTopology.Queues);
        existingTopology.Queues = topology.Queues;

        // Update bindings
        _dbContext.Bindings.RemoveRange(existingTopology.Bindings);
        existingTopology.Bindings = topology.Bindings;

        await _dbContext.SaveChangesAsync();
        return existingTopology;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var topology = await _dbContext.Topologies.FindAsync(id);
        if (topology == null)
        {
            return false;
        }

        _dbContext.Topologies.Remove(topology);
        await _dbContext.SaveChangesAsync();
        return true;
    }
} 