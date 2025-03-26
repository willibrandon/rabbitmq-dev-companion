using Companion.Core.Models;
using Companion.Core.Repositories;
using System.Text.RegularExpressions;

namespace Companion.Core.Services;

/// <summary>
/// Implementation of the topology service for managing and validating RabbitMQ topologies
/// </summary>
public class TopologyService : ITopologyService
{
    private static readonly Regex ValidNameRegex = new("^[a-zA-Z0-9-_.]+$", RegexOptions.Compiled);
    private readonly ITopologyRepository _topologyRepository;

    public TopologyService(ITopologyRepository topologyRepository)
    {
        _topologyRepository = topologyRepository;
    }

    /// <inheritdoc />
    public ValidationResult ValidateTopology(Topology topology)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (topology == null)
        {
            return ValidationResult.Failed("Topology cannot be null");
        }

        // Validate basic topology properties
        if (string.IsNullOrWhiteSpace(topology.Name))
        {
            errors.Add("Topology name is required");
        }

        // Validate exchanges
        foreach (var exchange in topology.Exchanges)
        {
            ValidateExchange(exchange, errors, warnings);
        }

        // Validate queues
        foreach (var queue in topology.Queues)
        {
            ValidateQueue(queue, errors, warnings);
        }

        // Validate bindings
        foreach (var binding in topology.Bindings)
        {
            ValidateBinding(binding, topology, errors);
        }

        // Check for orphaned components
        ValidateOrphanedComponents(topology, warnings);

        return errors.Any() 
            ? ValidationResult.Failed(errors.ToArray()) 
            : warnings.Any() 
                ? ValidationResult.SuccessWithWarnings(warnings.ToArray()) 
                : ValidationResult.Success();
    }

    /// <inheritdoc />
    public Topology NormalizeTopology(Topology topology)
    {
        if (topology == null)
        {
            throw new ArgumentNullException(nameof(topology));
        }

        return new Topology
        {
            Id = topology.Id,
            Name = NormalizeName(topology.Name),
            Description = topology.Description?.Trim() ?? string.Empty,
            Exchanges = topology.Exchanges.Select(NormalizeExchange).ToList(),
            Queues = topology.Queues.Select(NormalizeQueue).ToList(),
            Bindings = topology.Bindings.Select(NormalizeBinding).ToList(),
            CreatedAt = topology.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
            Metadata = topology.Metadata
        };
    }

    private void ValidateExchange(Exchange exchange, List<string> errors, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(exchange.Name))
        {
            errors.Add("Exchange name is required");
            return;
        }

        if (!ValidNameRegex.IsMatch(exchange.Name))
        {
            errors.Add($"Exchange name '{exchange.Name}' contains invalid characters");
        }

        // Specific exchange type validations
        switch (exchange.Type)
        {
            case ExchangeType.Headers when exchange.Arguments == null || !exchange.Arguments.Any():
                warnings.Add($"Headers exchange '{exchange.Name}' has no header arguments defined");
                break;
            case ExchangeType.ConsistentHash when !exchange.Arguments?.ContainsKey("hash-header") ?? true:
                errors.Add($"Consistent hash exchange '{exchange.Name}' requires a 'hash-header' argument");
                break;
        }
    }

    private void ValidateQueue(Queue queue, List<string> errors, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(queue.Name))
        {
            errors.Add("Queue name is required");
            return;
        }

        if (!ValidNameRegex.IsMatch(queue.Name))
        {
            errors.Add($"Queue name '{queue.Name}' contains invalid characters");
        }

        if (queue.MaxLength.HasValue && queue.MaxLength.Value <= 0)
        {
            errors.Add($"Queue '{queue.Name}' max length must be greater than 0");
        }

        if (queue.MessageTtl.HasValue && queue.MessageTtl.Value <= 0)
        {
            errors.Add($"Queue '{queue.Name}' message TTL must be greater than 0");
        }

        if (!string.IsNullOrEmpty(queue.DeadLetterExchange) && queue.DeadLetterRoutingKey == null)
        {
            warnings.Add($"Queue '{queue.Name}' has a dead letter exchange but no routing key specified");
        }
    }

    private void ValidateBinding(Binding binding, Topology topology, List<string> errors)
    {
        // Validate exchange exists
        if (!topology.Exchanges.Any(e => e.Name == binding.SourceExchange))
        {
            errors.Add($"Binding references non-existent exchange '{binding.SourceExchange}'");
        }

        // Validate queue exists
        if (!topology.Queues.Any(q => q.Name == binding.DestinationQueue))
        {
            errors.Add($"Binding references non-existent queue '{binding.DestinationQueue}'");
        }

        // Validate routing key requirements based on exchange type
        var exchange = topology.Exchanges.FirstOrDefault(e => e.Name == binding.SourceExchange);
        if (exchange != null)
        {
            switch (exchange.Type)
            {
                case ExchangeType.Direct when string.IsNullOrEmpty(binding.RoutingKey):
                    errors.Add($"Direct exchange binding '{binding.SourceExchange}' requires a routing key");
                    break;
                case ExchangeType.Headers when binding.Arguments == null || !binding.Arguments.Any():
                    errors.Add($"Headers exchange binding '{binding.SourceExchange}' requires header arguments");
                    break;
                case ExchangeType.Fanout when !string.IsNullOrEmpty(binding.RoutingKey):
                    errors.Add($"Fanout exchange binding '{binding.SourceExchange}' should not have a routing key");
                    break;
            }
        }
    }

    private void ValidateOrphanedComponents(Topology topology, List<string> warnings)
    {
        // Check for exchanges with no bindings
        var exchangesWithBindings = topology.Bindings.Select(b => b.SourceExchange).ToHashSet();
        foreach (var exchange in topology.Exchanges)
        {
            if (!exchangesWithBindings.Contains(exchange.Name) && exchange.Type != ExchangeType.DeadLetter)
            {
                warnings.Add($"Exchange '{exchange.Name}' has no bindings");
            }
        }

        // Check for queues with no bindings
        var queuesWithBindings = topology.Bindings.Select(b => b.DestinationQueue).ToHashSet();
        foreach (var queue in topology.Queues)
        {
            if (!queuesWithBindings.Contains(queue.Name))
            {
                warnings.Add($"Queue '{queue.Name}' has no bindings");
            }
        }
    }

    private static string NormalizeName(string name) =>
        string.IsNullOrWhiteSpace(name) 
            ? string.Empty 
            : name.Trim().ToLowerInvariant();

    private static Exchange NormalizeExchange(Exchange exchange) => new()
    {
        Name = NormalizeName(exchange.Name),
        Type = exchange.Type,
        Durable = exchange.Durable,
        AutoDelete = exchange.AutoDelete,
        Internal = exchange.Internal,
        Arguments = exchange.Arguments
    };

    private static Queue NormalizeQueue(Queue queue) => new()
    {
        Name = NormalizeName(queue.Name),
        Durable = queue.Durable,
        Exclusive = queue.Exclusive,
        AutoDelete = queue.AutoDelete,
        Arguments = queue.Arguments,
        MaxLength = queue.MaxLength,
        MessageTtl = queue.MessageTtl,
        DeadLetterExchange = NormalizeName(queue.DeadLetterExchange ?? string.Empty),
        DeadLetterRoutingKey = queue.DeadLetterRoutingKey
    };

    private static Binding NormalizeBinding(Binding binding) => new()
    {
        SourceExchange = NormalizeName(binding.SourceExchange),
        DestinationQueue = NormalizeName(binding.DestinationQueue),
        RoutingKey = binding.RoutingKey?.Trim() ?? string.Empty,
        Arguments = binding.Arguments
    };

    public async Task<Topology?> GetTopologyByIdAsync(string topologyId)
    {
        return await _topologyRepository.GetByIdAsync(topologyId);
    }
} 