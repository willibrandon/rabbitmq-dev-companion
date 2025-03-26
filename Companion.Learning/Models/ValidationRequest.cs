using System.Text.Json.Serialization;

namespace Companion.Learning.Models;

public class ValidationRequest
{
    [JsonPropertyName("topologyId")]
    public string TopologyId { get; set; } = string.Empty;

    [JsonPropertyName("validationData")]
    public Dictionary<string, string>? AdditionalData { get; set; }
} 