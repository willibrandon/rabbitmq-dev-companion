using System.Text.Json.Serialization;

namespace Companion.Core.Models;

/// <summary>
/// Represents a RabbitMQ exchange in the topology
/// </summary>
public class Exchange
{
    /// <summary>
    /// Gets or sets the unique identifier for the exchange
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the unique name of the exchange
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the exchange
    /// </summary>
    public ExchangeType Type { get; set; }

    /// <summary>
    /// Gets or sets whether the exchange is durable (survives broker restart)
    /// </summary>
    public bool Durable { get; set; }

    /// <summary>
    /// Gets or sets whether the exchange is auto-deleted when no longer used
    /// </summary>
    public bool AutoDelete { get; set; }

    /// <summary>
    /// Gets or sets whether the exchange is internal (only for internal messaging)
    /// </summary>
    public bool Internal { get; set; }

    /// <summary>
    /// Gets or sets optional arguments for the exchange
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Arguments { get; set; }
} 