using Companion.Core.Models;

namespace Companion.Core.Services;

/// <summary>
/// Service for generating configuration files from RabbitMQ topologies
/// </summary>
public interface IConfigGeneratorService
{
    /// <summary>
    /// Generates configuration files for a given topology
    /// </summary>
    /// <param name="topology">The topology to generate configuration for</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>The generated configuration output</returns>
    Task<ConfigurationOutput> GenerateConfigurationAsync(
        Topology topology,
        ConfigurationOptions? options = null);
}

/// <summary>
/// Options for configuration generation
/// </summary>
public class ConfigurationOptions
{
    /// <summary>
    /// The RabbitMQ username
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// The RabbitMQ password
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// The RabbitMQ virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Whether to generate Docker Compose configuration
    /// </summary>
    public bool IncludeDockerCompose { get; set; } = true;

    /// <summary>
    /// Whether to generate Kubernetes configuration
    /// </summary>
    public bool IncludeKubernetes { get; set; } = true;

    /// <summary>
    /// Whether to generate .NET producer code
    /// </summary>
    public bool IncludeProducerCode { get; set; } = true;

    /// <summary>
    /// Whether to generate .NET consumer code
    /// </summary>
    public bool IncludeConsumerCode { get; set; } = true;

    /// <summary>
    /// The namespace to use for generated .NET code
    /// </summary>
    public string Namespace { get; set; } = "RabbitMQ.Generated";
} 