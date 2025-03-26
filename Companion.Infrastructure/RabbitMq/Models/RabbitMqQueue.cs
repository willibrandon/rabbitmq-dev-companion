using System.Text.Json.Serialization;

namespace Companion.Infrastructure.RabbitMq.Models;

/// <summary>
/// Represents a queue as returned by the RabbitMQ Management API
/// </summary>
internal class RabbitMqQueue
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;

    [JsonPropertyName("durable")]
    public bool Durable { get; set; }

    [JsonPropertyName("auto_delete")]
    public bool AutoDelete { get; set; }

    [JsonPropertyName("exclusive")]
    public bool Exclusive { get; set; }

    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }

    [JsonPropertyName("messages")]
    public int Messages { get; set; }

    [JsonPropertyName("messages_ready")]
    public int MessagesReady { get; set; }

    [JsonPropertyName("messages_unacknowledged")]
    public int MessagesUnacknowledged { get; set; }

    [JsonPropertyName("consumers")]
    public int Consumers { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
} 