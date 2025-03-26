using System.Text.Json.Serialization;

namespace Companion.Core.Models;

/// <summary>
/// Represents a complete RabbitMQ topology with exchanges, queues, and their bindings
/// </summary>
public class Topology
{
    /// <summary>
    /// Gets or sets the unique identifier for the topology
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the topology
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the topology
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of exchanges in the topology
    /// </summary>
    public List<Exchange> Exchanges { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of queues in the topology
    /// </summary>
    public List<Queue> Queues { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of bindings in the topology
    /// </summary>
    public List<Binding> Bindings { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation date of the topology
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the last modification date of the topology
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets optional metadata for the topology
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
} 