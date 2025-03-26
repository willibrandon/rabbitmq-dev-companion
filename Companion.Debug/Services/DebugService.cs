using System.Text;
using Companion.Core.Models;
using Companion.Debug.Models;
using Companion.Infrastructure.RabbitMq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Companion.Debug.Services;

/// <summary>
/// Implementation of the debug service for RabbitMQ message flows and dead letters
/// </summary>
public class DebugService : IDebugService, IDisposable
{
    private readonly IRabbitMqManagementClient _managementClient;
    private readonly IConnection _connection;
    private readonly ILogger<DebugService> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the DebugService class
    /// </summary>
    public DebugService(
        IRabbitMqManagementClient managementClient,
        ConnectionFactory connectionFactory,
        ILogger<DebugService> logger)
    {
        _managementClient = managementClient;
        _connection = connectionFactory.CreateConnection();
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeadLetteredMessage>> GetDeadLetteredMessagesAsync(
        string? queueName = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<DeadLetteredMessage>();
        var topology = await _managementClient.GetTopologyFromBrokerAsync(cancellationToken);
        var deadLetterQueues = GetDeadLetterQueues(topology, queueName);

        using var channel = _connection.CreateModel();
        foreach (var dlq in deadLetterQueues)
        {
            var dlqMessages = GetMessagesFromQueue(channel, dlq, limit - messages.Count);
            messages.AddRange(dlqMessages);

            if (messages.Count >= limit)
                break;
        }

        return messages;
    }

    /// <inheritdoc />
    public async Task<MessageTrace> TraceMessageAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        var trace = new MessageTrace
        {
            MessageId = messageId,
            PublishedAt = DateTimeOffset.UtcNow // We'll update this when we find the message
        };

        var topology = await _managementClient.GetTopologyFromBrokerAsync(cancellationToken);
        
        // First check dead letter queues
        var deadLetterQueues = GetDeadLetterQueues(topology);
        using var channel = _connection.CreateModel();
        
        foreach (var dlq in deadLetterQueues)
        {
            var dlMessage = GetMessagesFromQueue(channel, dlq, 1, messageId).FirstOrDefault();
            if (dlMessage != null)
            {
                trace.WasDeadLettered = true;
                trace.DeadLetterDetails = dlMessage;
                trace.FinalQueue = dlq;

                if (dlMessage.Headers?.TryGetValue("x-death", out var xDeath) == true)
                {
                    // x-death header contains routing history
                    var xDeathList = xDeath as List<object>;
                    if (xDeathList != null)
                    {
                        foreach (Dictionary<string, object> death in xDeathList)
                        {
                            if (death.TryGetValue("exchange", out var exchange))
                                trace.ExchangesVisited.Add(exchange.ToString() ?? string.Empty);
                            if (death.TryGetValue("queue", out var queue))
                                trace.QueuesVisited.Add(queue.ToString() ?? string.Empty);
                        }
                    }
                }

                break;
            }
        }

        // If not found in DLQ, check regular queues
        if (!trace.WasDeadLettered)
        {
            foreach (var queue in topology.Queues)
            {
                var message = GetMessagesFromQueue(channel, queue.Name, 1, messageId, true).FirstOrDefault();
                if (message != null)
                {
                    trace.FinalQueue = queue.Name;
                    // Add exchange/queue info from headers if available
                    if (message.Headers != null)
                    {
                        if (message.Headers.TryGetValue("x-first-death-exchange", out var exchange))
                            trace.ExchangesVisited.Add(exchange.ToString() ?? string.Empty);
                        if (message.Headers.TryGetValue("x-first-death-queue", out var q))
                            trace.QueuesVisited.Add(q.ToString() ?? string.Empty);
                    }
                    break;
                }
            }
        }

        return trace;
    }

    /// <inheritdoc />
    public async Task RequeueDeadLetteredMessageAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        var topology = await _managementClient.GetTopologyFromBrokerAsync(cancellationToken);
        var deadLetterQueues = GetDeadLetterQueues(topology);

        using var channel = _connection.CreateModel();
        foreach (var dlq in deadLetterQueues)
        {
            var message = GetMessagesFromQueue(channel, dlq, 1, messageId).FirstOrDefault();
            if (message != null && message.Headers?.ContainsKey("x-death") == true)
            {
                var xDeath = message.Headers["x-death"] as List<object>;
                if (xDeath?.Count > 0)
                {
                    var lastDeath = xDeath[0] as Dictionary<string, object>;
                    if (lastDeath != null)
                    {
                        var originalQueue = lastDeath["queue"]?.ToString();
                        var originalExchange = lastDeath["exchange"]?.ToString();
                        var originalRoutingKey = message.OriginalRoutingKey;

                        if (!string.IsNullOrEmpty(originalExchange) && !string.IsNullOrEmpty(originalRoutingKey))
                        {
                            var props = channel.CreateBasicProperties();
                            props.MessageId = message.MessageId;
                            foreach (var header in message.Headers.Where(h => !h.Key.StartsWith("x-death")))
                            {
                                props.Headers ??= new Dictionary<string, object>();
                                props.Headers[header.Key] = header.Value;
                            }

                            channel.BasicPublish(
                                exchange: originalExchange,
                                routingKey: originalRoutingKey,
                                basicProperties: props,
                                body: message.Body);

                            _logger.LogInformation(
                                "Requeued message {MessageId} to exchange {Exchange} with routing key {RoutingKey}",
                                messageId, originalExchange, originalRoutingKey);
                            return;
                        }
                    }
                }
            }
        }

        throw new KeyNotFoundException($"Message {messageId} not found in any dead letter queue");
    }

    private static IEnumerable<string> GetDeadLetterQueues(Topology topology, string? specificQueue = null)
    {
        if (specificQueue != null)
            return new[] { specificQueue };

        return topology.Queues
            .Where(q => q.Arguments?.ContainsKey("x-dead-letter-exchange") == true ||
                       q.Name.EndsWith(".dlq", StringComparison.OrdinalIgnoreCase) ||
                       q.Name.EndsWith(".dead", StringComparison.OrdinalIgnoreCase))
            .Select(q => q.Name);
    }

    private static IEnumerable<DeadLetteredMessage> GetMessagesFromQueue(
        IModel channel,
        string queueName,
        int limit,
        string? specificMessageId = null,
        bool requeue = false)
    {
        var messages = new List<DeadLetteredMessage>();
        var count = 0;

        while (count < limit)
        {
            var result = channel.BasicGet(queueName, !requeue);
            if (result == null)
                break;

            var message = new DeadLetteredMessage
            {
                MessageId = result.BasicProperties.MessageId ?? string.Empty,
                Headers = result.BasicProperties.Headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Body = result.Body.ToArray(),
                DeadLetteredAt = DateTimeOffset.UtcNow, // Approximate time
                SourceQueue = queueName
            };

            if (result.BasicProperties.Headers != null)
            {
                if (result.BasicProperties.Headers.TryGetValue("x-death", out var xDeath))
                {
                    var xDeathList = xDeath as List<object>;
                    if (xDeathList?.Count > 0)
                    {
                        var lastDeath = xDeathList[0] as Dictionary<string, object>;
                        if (lastDeath != null)
                        {
                            message.OriginalExchange = lastDeath["exchange"]?.ToString() ?? string.Empty;
                            message.OriginalRoutingKey = lastDeath["routing-keys"] is List<object> routingKeys && routingKeys.Count > 0
                                ? routingKeys[0]?.ToString() ?? string.Empty
                                : string.Empty;
                            message.RejectionCount = xDeathList.Count;
                            message.Reason = lastDeath["reason"]?.ToString() ?? string.Empty;
                        }
                    }
                }
            }

            if (specificMessageId == null || message.MessageId == specificMessageId)
            {
                messages.Add(message);
                count++;
            }

            if (specificMessageId != null && message.MessageId == specificMessageId)
                break;
        }

        return messages;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of managed and unmanaged resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _connection?.Dispose();
        }

        _disposed = true;
    }
} 