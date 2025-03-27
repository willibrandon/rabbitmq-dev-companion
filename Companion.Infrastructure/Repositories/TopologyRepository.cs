using Companion.Core.Models;
using Companion.Core.Repositories;
using Companion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Companion.Infrastructure.Repositories;

public class TopologyRepository : ITopologyRepository
{
    private readonly CompanionDbContext _context;

    public TopologyRepository(CompanionDbContext context)
    {
        _context = context;
    }

    public async Task<Topology?> GetByIdAsync(string topologyId)
    {
        return await _context.Topologies
            .Include(t => t.Exchanges)
            .Include(t => t.Queues)
            .Include(t => t.Bindings)
            .FirstOrDefaultAsync(t => t.Id == topologyId);
    }

    public async Task<Topology> CreateOrUpdateAsync(Topology topology)
    {
        var existing = await _context.Topologies
            .Include(t => t.Exchanges)
            .Include(t => t.Queues)
            .Include(t => t.Bindings)
            .FirstOrDefaultAsync(t => t.Id == topology.Id);

        if (existing == null)
        {
            // Create new topology
            topology.CreatedAt = DateTime.UtcNow;
            topology.UpdatedAt = DateTime.UtcNow;
            _context.Topologies.Add(topology);
        }
        else
        {
            // Update existing topology
            existing.Name = topology.Name;
            existing.Description = topology.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            // Update collections
            _context.Exchanges.RemoveRange(existing.Exchanges);
            _context.Queues.RemoveRange(existing.Queues);
            _context.Bindings.RemoveRange(existing.Bindings);

            existing.Exchanges = topology.Exchanges;
            existing.Queues = topology.Queues;
            existing.Bindings = topology.Bindings;

            topology = existing;
        }

        await _context.SaveChangesAsync();
        return topology;
    }

    public async Task<bool> DeleteAsync(string topologyId)
    {
        var topology = await _context.Topologies.FindAsync(topologyId);
        if (topology == null) return false;

        _context.Topologies.Remove(topology);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Topology>> GetAllAsync()
    {
        return await _context.Topologies
            .Include(t => t.Exchanges)
            .Include(t => t.Queues)
            .Include(t => t.Bindings)
            .ToListAsync();
    }
} 