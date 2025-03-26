using System.Text.Json.Serialization;

namespace Companion.Debug.Models;

/// <summary>
/// Represents a message that has been dead-lettered in RabbitMQ
/// </summary>
public class DeadLetteredMessage
{
    /// <summary>
    /// Gets or sets the message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original exchange the message was published to
    /// </summary>
    public string OriginalExchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original routing key used
    /// </summary>
    public string OriginalRoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the queue the message was dead-lettered from
    /// </summary>
    public string SourceQueue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the message was dead-lettered
    /// </summary>
    public DateTimeOffset DeadLetteredAt { get; set; }

    /// <summary>
    /// Gets or sets the reason the message was dead-lettered
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of times this message was rejected/requeued
    /// </summary>
    public int RejectionCount { get; set; }

    /// <summary>
    /// Gets or sets the message headers
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the message body as a byte array
    /// </summary>
    public byte[] Body { get; set; } = Array.Empty<byte>();
} 