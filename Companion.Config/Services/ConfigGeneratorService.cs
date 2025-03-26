using System.Reflection;
using Companion.Core.Models;
using Companion.Core.Services;
using Scriban;
using YamlDotNet.Serialization;

namespace Companion.Config.Services;

/// <summary>
/// Implementation of the configuration generator service
/// </summary>
public class ConfigGeneratorService : IConfigGeneratorService
{
    /// <inheritdoc />
    public async Task<ConfigurationOutput> GenerateConfigurationAsync(
        Topology topology,
        ConfigurationOptions? options = null)
    {
        options ??= new ConfigurationOptions();
        var output = new ConfigurationOutput();

        if (options.IncludeDockerCompose)
        {
            output.DockerComposeYaml = await GenerateFromTemplateAsync(
                "docker-compose.scriban",
                topology,
                options);
        }

        if (options.IncludeKubernetes)
        {
            output.KubernetesYaml = await GenerateFromTemplateAsync(
                "kubernetes.scriban",
                topology,
                options);
        }

        if (options.IncludeProducerCode)
        {
            output.ProducerCode = await GenerateFromTemplateAsync(
                "producer.scriban",
                topology,
                options);
        }

        if (options.IncludeConsumerCode)
        {
            output.ConsumerCode = await GenerateFromTemplateAsync(
                "consumer.scriban",
                topology,
                options);
        }

        return output;
    }

    private async Task<string> GenerateFromTemplateAsync(
        string templateName,
        Topology topology,
        ConfigurationOptions options)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Companion.Config.Templates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Template {templateName} not found");
        }

        using var reader = new StreamReader(stream);
        var templateContent = await reader.ReadToEndAsync();

        var template = Template.Parse(templateContent);
        var model = new
        {
            topology,
            options
        };

        return await template.RenderAsync(model);
    }
} 