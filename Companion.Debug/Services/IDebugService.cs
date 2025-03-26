using Companion.Debug.Models;

namespace Companion.Debug.Services;

/// <summary>
/// Service for debugging RabbitMQ message flows and dead letters
/// </summary>
public interface IDebugService
{
    /// <summary>
    /// Gets all dead-lettered messages from the specified queue
    /// </summary>
    /// <param name="queueName">The queue name to check for dead letters. If null, checks all dead letter queues.</param>
    /// <param name="limit">Maximum number of messages to retrieve</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A list of dead-lettered messages</returns>
    Task<IReadOnlyList<DeadLetteredMessage>> GetDeadLetteredMessagesAsync(
        string? queueName = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Traces a message through the RabbitMQ system by its ID
    /// </summary>
    /// <param name="messageId">The message ID to trace</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The message trace information</returns>
    Task<MessageTrace> TraceMessageAsync(
        string messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requeues a dead-lettered message back to its original queue
    /// </summary>
    /// <param name="messageId">The message ID to requeue</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task RequeueDeadLetteredMessageAsync(
        string messageId,
        CancellationToken cancellationToken = default);
} 