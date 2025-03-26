using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Companion.Core.Models;
using Companion.Infrastructure.Configuration;
using Companion.Infrastructure.RabbitMq.Models;
using Microsoft.Extensions.Options;

namespace Companion.Infrastructure.RabbitMq;

/// <summary>
/// Implementation of the RabbitMQ Management API client
/// </summary>
public class RabbitMqManagementClient : IRabbitMqManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly RabbitMqSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _encodedVhost;

    /// <summary>
    /// Initializes a new instance of the RabbitMqManagementClient class
    /// </summary>
    public RabbitMqManagementClient(HttpClient httpClient, IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
        _settings.Validate();

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.UserName}:{_settings.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _encodedVhost = HttpUtility.UrlEncode(_settings.VirtualHost);
    }

    /// <inheritdoc />
    public async Task<Topology> GetTopologyFromBrokerAsync(CancellationToken cancellationToken = default)
    {
        var exchanges = await GetExchangesAsync(cancellationToken);
        var queues = await GetQueuesAsync(cancellationToken);
        var bindings = await GetBindingsAsync(cancellationToken);

        return new Topology
        {
            Name = $"Imported from {_settings.BaseUrl} at {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}",
            Description = $"Topology imported from RabbitMQ broker at {_settings.BaseUrl}",
            Exchanges = exchanges.ToList(),
            Queues = queues.ToList(),
            Bindings = bindings.ToList(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Exchange>> GetExchangesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"exchanges/{_encodedVhost}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var rabbitExchanges = JsonSerializer.Deserialize<List<RabbitMqExchange>>(content, _jsonOptions);

        if (rabbitExchanges == null)
            return Array.Empty<Exchange>();

        return rabbitExchanges.Select(e => new Exchange
        {
            Name = e.Name,
            Type = ParseExchangeType(e.Type),
            Durable = e.Durable,
            AutoDelete = e.AutoDelete,
            Internal = e.Internal,
            Arguments = e.Arguments
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Queue>> GetQueuesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"queues/{_encodedVhost}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var rabbitQueues = JsonSerializer.Deserialize<List<RabbitMqQueue>>(content, _jsonOptions);

        if (rabbitQueues == null)
            return Array.Empty<Queue>();

        return rabbitQueues.Select(q => new Queue
        {
            Name = q.Name,
            Durable = q.Durable,
            AutoDelete = q.AutoDelete,
            Exclusive = q.Exclusive,
            Arguments = q.Arguments,
            MaxLength = q.Arguments?.TryGetValue("x-max-length", out var maxLength) == true ? Convert.ToInt32(maxLength) : null,
            MessageTtl = q.Arguments?.TryGetValue("x-message-ttl", out var ttl) == true ? Convert.ToInt32(ttl) : null,
            DeadLetterExchange = q.Arguments?.TryGetValue("x-dead-letter-exchange", out var dlx) == true ? dlx.ToString() : null,
            DeadLetterRoutingKey = q.Arguments?.TryGetValue("x-dead-letter-routing-key", out var dlrk) == true ? dlrk.ToString() : null
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Binding>> GetBindingsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"bindings/{_encodedVhost}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var rabbitBindings = JsonSerializer.Deserialize<List<RabbitMqBinding>>(content, _jsonOptions);

        if (rabbitBindings == null)
            return Array.Empty<Binding>();

        return rabbitBindings
            .Where(b => b.DestinationType == "queue") // Only include exchange-to-queue bindings for now
            .Select(b => new Binding
            {
                SourceExchange = b.Source,
                DestinationQueue = b.Destination,
                RoutingKey = b.RoutingKey,
                Arguments = b.Arguments
            }).ToList();
    }

    /// <inheritdoc />
    public async Task<bool> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("health/checks/alarms", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static ExchangeType ParseExchangeType(string type) => type.ToLowerInvariant() switch
    {
        "direct" => ExchangeType.Direct,
        "fanout" => ExchangeType.Fanout,
        "topic" => ExchangeType.Topic,
        "headers" => ExchangeType.Headers,
        "x-consistent-hash" => ExchangeType.ConsistentHash,
        "x-dead-letter" => ExchangeType.DeadLetter,
        _ => throw new ArgumentException($"Unknown exchange type: {type}")
    };
} 