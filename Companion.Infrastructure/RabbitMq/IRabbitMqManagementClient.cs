using Companion.Core.Models;

namespace Companion.Infrastructure.RabbitMq;

/// <summary>
/// Client for interacting with the RabbitMQ Management API
/// </summary>
public interface IRabbitMqManagementClient
{
    /// <summary>
    /// Gets the current topology from the RabbitMQ broker
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A topology representing the current state of the broker</returns>
    Task<Topology> GetTopologyFromBrokerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all exchanges from the broker
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A list of exchanges</returns>
    Task<IReadOnlyList<Exchange>> GetExchangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all queues from the broker
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A list of queues</returns>
    Task<IReadOnlyList<Queue>> GetQueuesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all bindings from the broker
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A list of bindings</returns>
    Task<IReadOnlyList<Binding>> GetBindingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of the RabbitMQ broker
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>True if the broker is healthy, false otherwise</returns>
    Task<bool> GetHealthStatusAsync(CancellationToken cancellationToken = default);
} 