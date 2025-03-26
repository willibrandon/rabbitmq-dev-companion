namespace Companion.Core.Models;

/// <summary>
/// Represents the generated configuration output
/// </summary>
public class ConfigurationOutput
{
    /// <summary>
    /// The Docker Compose YAML configuration
    /// </summary>
    public string DockerComposeYaml { get; set; } = string.Empty;

    /// <summary>
    /// The Kubernetes YAML configuration
    /// </summary>
    public string KubernetesYaml { get; set; } = string.Empty;

    /// <summary>
    /// The .NET producer code
    /// </summary>
    public string ProducerCode { get; set; } = string.Empty;

    /// <summary>
    /// The .NET consumer code
    /// </summary>
    public string ConsumerCode { get; set; } = string.Empty;

    /// <summary>
    /// Any additional configuration files or scripts
    /// </summary>
    public Dictionary<string, string> AdditionalFiles { get; set; } = new();
} 