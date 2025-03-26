using System.Text.Json.Serialization;

namespace Companion.Core.Models;

/// <summary>
/// Represents a RabbitMQ queue in the topology
/// </summary>
public class Queue
{
    /// <summary>
    /// Gets or sets the unique identifier for the queue
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the unique name of the queue
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the queue is durable (survives broker restart)
    /// </summary>
    public bool Durable { get; set; }

    /// <summary>
    /// Gets or sets whether the queue is exclusive to one connection
    /// </summary>
    public bool Exclusive { get; set; }

    /// <summary>
    /// Gets or sets whether the queue is auto-deleted when no longer used
    /// </summary>
    public bool AutoDelete { get; set; }

    /// <summary>
    /// Gets or sets optional arguments for the queue
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Arguments { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of messages in the queue
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the message time-to-live in milliseconds
    /// </summary>
    public int? MessageTtl { get; set; }

    /// <summary>
    /// Gets or sets the dead letter exchange name
    /// </summary>
    public string? DeadLetterExchange { get; set; }

    /// <summary>
    /// Gets or sets the dead letter routing key
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }
} 