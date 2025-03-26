namespace Companion.Simulator.Models;

/// <summary>
/// Configuration for a message flow simulation
/// </summary>
public class SimulationConfig
{
    /// <summary>
    /// Gets or sets the unique identifier of the topology to simulate
    /// </summary>
    public string TopologyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of messages to publish
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// Gets or sets the size of each message in bytes
    /// </summary>
    public int MessageSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the routing key pattern for messages (e.g., "logs.*" for topic exchanges)
    /// </summary>
    public string RoutingKeyPattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of concurrent publishers
    /// </summary>
    public int ConcurrentPublishers { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to simulate consumer failures
    /// </summary>
    public bool SimulateConsumerFailures { get; set; }

    /// <summary>
    /// Gets or sets the failure rate for consumers (0.0 to 1.0)
    /// </summary>
    public double ConsumerFailureRate { get; set; }

    /// <summary>
    /// Gets or sets the message publishing rate per second (0 for unlimited)
    /// </summary>
    public int PublishRatePerSecond { get; set; }

    /// <summary>
    /// Gets or sets custom message headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }
} 