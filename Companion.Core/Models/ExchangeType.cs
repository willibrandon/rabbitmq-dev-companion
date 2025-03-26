namespace Companion.Core.Models;

/// <summary>
/// Represents the different types of exchanges available in RabbitMQ
/// </summary>
public enum ExchangeType
{
    /// <summary>
    /// Direct exchange delivers messages to queues based on exact routing key match
    /// </summary>
    Direct,

    /// <summary>
    /// Fanout exchange broadcasts messages to all bound queues
    /// </summary>
    Fanout,

    /// <summary>
    /// Topic exchange routes messages based on wildcard pattern matching of routing keys
    /// </summary>
    Topic,

    /// <summary>
    /// Headers exchange uses message header attributes instead of routing keys
    /// </summary>
    Headers,

    /// <summary>
    /// Consistent hash exchange distributes messages across queues using consistent hashing
    /// </summary>
    ConsistentHash,

    /// <summary>
    /// Dead letter exchange (DLX) receives messages that cannot be delivered
    /// </summary>
    DeadLetter
} 