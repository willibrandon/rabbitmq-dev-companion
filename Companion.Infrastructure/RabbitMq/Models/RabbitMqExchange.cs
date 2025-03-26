using System.Text.Json.Serialization;

namespace Companion.Infrastructure.RabbitMq.Models;

/// <summary>
/// Represents an exchange as returned by the RabbitMQ Management API
/// </summary>
internal class RabbitMqExchange
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("durable")]
    public bool Durable { get; set; }

    [JsonPropertyName("auto_delete")]
    public bool AutoDelete { get; set; }

    [JsonPropertyName("internal")]
    public bool Internal { get; set; }

    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }
} 