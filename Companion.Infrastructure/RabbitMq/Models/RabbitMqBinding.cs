using System.Text.Json.Serialization;

namespace Companion.Infrastructure.RabbitMq.Models;

/// <summary>
/// Represents a binding as returned by the RabbitMQ Management API
/// </summary>
internal class RabbitMqBinding
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("destination_type")]
    public string DestinationType { get; set; } = string.Empty;

    [JsonPropertyName("routing_key")]
    public string RoutingKey { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }

    [JsonPropertyName("properties_key")]
    public string PropertiesKey { get; set; } = string.Empty;
} 