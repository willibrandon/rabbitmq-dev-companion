using System.Collections.Concurrent;
using Companion.Core.Models;
using Companion.Infrastructure.RabbitMq;
using Companion.Simulator.Hubs;
using Companion.Simulator.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Companion.Simulator.Services;

/// <summary>
/// Implementation of the message flow service
/// </summary>
public class MessageFlowService : IMessageFlowService, IDisposable
{
    private readonly IRabbitMqManagementClient _managementClient;
    private readonly IHubContext<SimulationHub> _hubContext;
    private readonly ILogger<MessageFlowService> _logger;
    private readonly ConcurrentDictionary<string, SimulationStatus> _simulations;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens;
    private readonly IConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the MessageFlowService class
    /// </summary>
    public MessageFlowService(
        IRabbitMqManagementClient managementClient,
        IHubContext<SimulationHub> hubContext,
        ILogger<MessageFlowService> logger,
        ConnectionFactory connectionFactory)
    {
        _managementClient = managementClient;
        _hubContext = hubContext;
        _logger = logger;
        _simulations = new ConcurrentDictionary<string, SimulationStatus>();
        _cancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        _connection = connectionFactory.CreateConnection();
    }

    /// <inheritdoc />
    public async Task<string> StartSimulationAsync(SimulationConfig config, CancellationToken cancellationToken = default)
    {
        var simulationId = Guid.NewGuid().ToString();
        var status = new SimulationStatus
        {
            SimulationId = simulationId,
            State = SimulationState.Initializing,
            StartTime = DateTimeOffset.UtcNow
        };

        _simulations.TryAdd(simulationId, status);

        try
        {
            // Get the topology to validate exchanges exist
            var topology = await _managementClient.GetTopologyFromBrokerAsync(cancellationToken);
            if (!ValidateTopology(topology, config))
            {
                throw new InvalidOperationException("Invalid topology configuration");
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cancellationTokens.TryAdd(simulationId, cts);

            // Start the simulation in a background task
            _ = Task.Run(() => RunSimulationAsync(simulationId, config, cts.Token), cts.Token);

            return simulationId;
        }
        catch (Exception ex)
        {
            status.State = SimulationState.Failed;
            status.ErrorMessage = ex.Message;
            await UpdateSimulationStatusAsync(status);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopSimulationAsync(string simulationId, CancellationToken cancellationToken = default)
    {
        if (_cancellationTokens.TryRemove(simulationId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();

            if (_simulations.TryGetValue(simulationId, out var status))
            {
                status.State = SimulationState.Stopped;
                status.EndTime = DateTimeOffset.UtcNow;
                await UpdateSimulationStatusAsync(status);
            }
        }
    }

    /// <inheritdoc />
    public Task<SimulationStatus> GetSimulationStatusAsync(string simulationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_simulations.TryGetValue(simulationId, out var status)
            ? status
            : throw new KeyNotFoundException($"Simulation {simulationId} not found"));
    }

    private async Task RunSimulationAsync(string simulationId, SimulationConfig config, CancellationToken cancellationToken)
    {
        var status = _simulations[simulationId];
        status.State = SimulationState.Running;
        await UpdateSimulationStatusAsync(status);

        try
        {
            using var channel = _connection.CreateModel();
            var startTime = DateTimeOffset.UtcNow;
            var messagesSent = 0;
            var messageBody = new byte[config.MessageSizeBytes];
            new Random().NextBytes(messageBody);

            var basicProps = channel.CreateBasicProperties();
            basicProps.Headers = config.Headers;

            var tasks = new List<Task>();
            for (var i = 0; i < config.ConcurrentPublishers; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var publisherMessages = config.MessageCount / config.ConcurrentPublishers;
                    for (var j = 0; j < publisherMessages && !cancellationToken.IsCancellationRequested; j++)
                    {
                        channel.BasicPublish(
                            exchange: config.RoutingKeyPattern.Split('.')[0], // Assuming first part is exchange name
                            routingKey: config.RoutingKeyPattern,
                            basicProperties: basicProps,
                            body: messageBody);

                        Interlocked.Increment(ref messagesSent);
                        status.MessagesPublished = messagesSent;
                        status.PublishRatePerSecond = messagesSent / (DateTimeOffset.UtcNow - startTime).TotalSeconds;

                        if (j % 100 == 0) // Update status every 100 messages
                        {
                            await UpdateSimulationStatusAsync(status);
                        }

                        if (config.PublishRatePerSecond > 0)
                        {
                            await Task.Delay(1000 / config.PublishRatePerSecond, cancellationToken);
                        }
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            status.State = SimulationState.Completed;
            status.EndTime = DateTimeOffset.UtcNow;
            await UpdateSimulationStatusAsync(status);
        }
        catch (OperationCanceledException)
        {
            status.State = SimulationState.Stopped;
            status.EndTime = DateTimeOffset.UtcNow;
            await UpdateSimulationStatusAsync(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running simulation {SimulationId}", simulationId);
            status.State = SimulationState.Failed;
            status.ErrorMessage = ex.Message;
            status.EndTime = DateTimeOffset.UtcNow;
            await UpdateSimulationStatusAsync(status);
        }
    }

    private bool ValidateTopology(Topology topology, SimulationConfig config)
    {
        var exchangeName = config.RoutingKeyPattern.Split('.')[0];
        return topology.Exchanges.Any(e => e.Name == exchangeName);
    }

    private async Task UpdateSimulationStatusAsync(SimulationStatus status)
    {
        await _hubContext.Clients.All.SendAsync("SimulationUpdate", status);
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
            foreach (var cts in _cancellationTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }

            _connection?.Dispose();
        }

        _disposed = true;
    }
} 