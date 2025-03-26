using System.Text.Json.Serialization;

namespace Companion.Debug.Models;

/// <summary>
/// Represents a trace of a message through the RabbitMQ system
/// </summary>
public class MessageTrace
{
    /// <summary>
    /// Gets or sets the message ID being traced
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the message was first published
    /// </summary>
    public DateTimeOffset PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of exchanges the message passed through
    /// </summary>
    public List<string> ExchangesVisited { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of queues the message was enqueued in
    /// </summary>
    public List<string> QueuesVisited { get; set; } = new();

    /// <summary>
    /// Gets or sets the final destination queue (if any)
    /// </summary>
    public string? FinalQueue { get; set; }

    /// <summary>
    /// Gets or sets whether the message was dead-lettered
    /// </summary>
    public bool WasDeadLettered { get; set; }

    /// <summary>
    /// Gets or sets the dead letter details if the message was dead-lettered
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DeadLetteredMessage? DeadLetterDetails { get; set; }

    /// <summary>
    /// Gets or sets any exception that occurred during message processing
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MessageProcessingException? Exception { get; set; }
}

/// <summary>
/// Represents an exception that occurred during message processing
/// </summary>
public class MessageProcessingException
{
    /// <summary>
    /// Gets or sets the type of the exception
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exception message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the exception occurred
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// Gets or sets the queue where the exception occurred
    /// </summary>
    public string Queue { get; set; } = string.Empty;
} 