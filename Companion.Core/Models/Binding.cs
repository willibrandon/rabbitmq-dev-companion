using System.Text.Json.Serialization;

namespace Companion.Core.Models;

/// <summary>
/// Represents a binding between an exchange and a queue in RabbitMQ
/// </summary>
public class Binding
{
    /// <summary>
    /// Gets or sets the unique identifier for the binding
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the source exchange name
    /// </summary>
    public string SourceExchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination queue name
    /// </summary>
    public string DestinationQueue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing key for the binding
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional arguments for the binding (used primarily with headers exchange)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Arguments { get; set; }
} 