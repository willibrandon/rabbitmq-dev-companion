using System.Text.Json;
using System.Text.RegularExpressions;
using Companion.Core.Models;
using Companion.Learning.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Companion.Learning.Services;

/// <summary>
/// Implementation of the learning service
/// </summary>
public class LearningService : ILearningService
{
    private readonly ILogger<LearningService> _logger;
    private readonly ConcurrentDictionary<string, LearningModule> _modules;

    /// <summary>
    /// Initializes a new instance of the LearningService class
    /// </summary>
    public LearningService(ILogger<LearningService> logger)
    {
        _logger = logger;
        _modules = new ConcurrentDictionary<string, LearningModule>();
        LoadModules();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LearningModule>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<LearningModule>>(_modules.Values.ToList());
    }

    /// <inheritdoc />
    public Task<LearningModule?> GetModuleByIdAsync(string moduleId, CancellationToken cancellationToken = default)
    {
        _modules.TryGetValue(moduleId, out var module);
        return Task.FromResult(module);
    }

    /// <inheritdoc />
    public Task<bool> ValidateStepProgressAsync(
        string moduleId,
        int stepIndex,
        ValidationRequest validationRequest,
        CancellationToken cancellationToken = default)
    {
        if (!_modules.TryGetValue(moduleId, out var module))
        {
            _logger.LogWarning("Module {ModuleId} not found", moduleId);
            return Task.FromResult(false);
        }

        if (stepIndex < 0 || stepIndex >= module.Steps.Count)
        {
            _logger.LogWarning("Invalid step index {StepIndex} for module {ModuleId}", stepIndex, moduleId);
            return Task.FromResult(false);
        }

        var step = module.Steps[stepIndex];
        
        // TODO: Implement actual validation logic using the topology ID and additional data
        // For now, we'll just return true to simulate successful validation
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task UpdateProgressAsync(
        string moduleId,
        int stepIndex,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement progress tracking
        _logger.LogInformation("Progress updated for module {ModuleId}, step {StepIndex}, user {UserId}",
            moduleId, stepIndex, userId ?? "anonymous");
        return Task.CompletedTask;
    }

    private void LoadModules()
    {
        var publishSubscribeModule = new LearningModule
        {
            ModuleId = "pub-sub-basics",
            Title = "Publish/Subscribe Basics",
            Description = "Learn the fundamentals of publish/subscribe messaging with RabbitMQ",
            Difficulty = DifficultyLevel.Beginner,
            EstimatedTimeMinutes = 30,
            Prerequisites = new List<string> { "Basic RabbitMQ knowledge" },
            Tags = new List<string> { "messaging", "pub/sub", "rabbitmq" },
            Steps = new List<LearningStep>
            {
                new LearningStep
                {
                    Title = "Create a Fanout Exchange",
                    Description = "Create your first fanout exchange to broadcast messages to multiple queues",
                    ValidationCriteria = "Verify that a fanout exchange has been created",
                    CodeSnippet = "rabbitmqctl declare exchange my-fanout fanout",
                    Hints = new List<string> { "Use the RabbitMQ Management UI or CLI to create the exchange" }
                },
                new LearningStep
                {
                    Title = "Create Multiple Queues",
                    Description = "Create two queues that will receive messages from the fanout exchange",
                    ValidationCriteria = "Verify that two queues exist and are properly bound to the exchange",
                    CodeSnippet = "rabbitmqctl declare queue queue1\nrabbitmqctl declare queue queue2",
                    Hints = new List<string> { "Make sure to bind both queues to the fanout exchange" }
                }
            }
        };

        var topicRoutingModule = new LearningModule
        {
            ModuleId = "topic-routing",
            Title = "Topic Exchange Routing",
            Description = "Master topic exchanges and routing patterns in RabbitMQ",
            Difficulty = DifficultyLevel.Intermediate,
            EstimatedTimeMinutes = 45,
            Prerequisites = new List<string> { "Publish/Subscribe Basics" },
            Tags = new List<string> { "messaging", "routing", "topics", "rabbitmq" },
            Steps = new List<LearningStep>
            {
                new LearningStep
                {
                    Title = "Create a Topic Exchange",
                    Description = "Create a topic exchange for routing messages based on patterns",
                    ValidationCriteria = "Verify that a topic exchange has been created",
                    CodeSnippet = "rabbitmqctl declare exchange my-topics topic",
                    Hints = new List<string> { "Topic exchanges use routing keys with dot-separated words" }
                },
                new LearningStep
                {
                    Title = "Bind Queues with Patterns",
                    Description = "Create queues and bind them with different routing patterns",
                    ValidationCriteria = "Verify that queues are bound with correct routing patterns",
                    CodeSnippet = "rabbitmqctl bind my-topics queue1 *.important.*\nrabbitmqctl bind my-topics queue2 #.error",
                    Hints = new List<string> { "Use * for exactly one word", "Use # for zero or more words" }
                }
            }
        };

        _modules.TryAdd(publishSubscribeModule.ModuleId, publishSubscribeModule);
        _modules.TryAdd(topicRoutingModule.ModuleId, topicRoutingModule);
    }

    private bool ValidateTopologyAgainstCriteria(Topology topology, string criteria)
    {
        try
        {
            // This is a simplified validation that uses basic string matching
            // In a real implementation, you would want to use a proper expression evaluator

            // Check for exchange validation
            if (criteria.Contains("exchange."))
            {
                foreach (var exchange in topology.Exchanges)
                {
                    if (ValidateExchange(exchange, criteria))
                        return true;
                }
                return false;
            }

            // Check for queue validation
            if (criteria.Contains("queues.any"))
            {
                var queuePattern = new Regex(@"queues\.any\(q => q\.name == '([^']+)'\)");
                var matches = queuePattern.Matches(criteria);
                foreach (Match match in matches)
                {
                    var queueName = match.Groups[1].Value;
                    if (!topology.Queues.Any(q => q.Name == queueName))
                        return false;
                }
                return true;
            }

            // Check for binding validation
            if (criteria.Contains("bindings."))
            {
                if (criteria.Contains("bindings.count"))
                {
                    var countPattern = new Regex(@"bindings\.count\(b => b\.sourceExchange == '([^']+)'\) == (\d+)");
                    var match = countPattern.Match(criteria);
                    if (match.Success)
                    {
                        var exchangeName = match.Groups[1].Value;
                        var count = int.Parse(match.Groups[2].Value);
                        return topology.Bindings.Count(b => b.SourceExchange == exchangeName) == count;
                    }
                }

                if (criteria.Contains("bindings.any"))
                {
                    if (criteria.Contains("routingKey.endsWith"))
                    {
                        var pattern = new Regex(@"\.endsWith\('([^']+)'\)");
                        var matches = pattern.Matches(criteria);
                        foreach (Match match in matches)
                        {
                            var suffix = match.Groups[1].Value;
                            if (!topology.Bindings.Any(b => b.RoutingKey.EndsWith(suffix)))
                                return false;
                        }
                        return true;
                    }
                }
            }

            _logger.LogWarning("Unsupported validation criteria: {Criteria}", criteria);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating topology against criteria: {Criteria}", criteria);
            return false;
        }
    }

    private bool ValidateExchange(Exchange exchange, string criteria)
    {
        var namePattern = new Regex(@"exchange\.name == '([^']+)'");
        var typePattern = new Regex(@"exchange\.type == '([^']+)'");

        var nameMatch = namePattern.Match(criteria);
        var typeMatch = typePattern.Match(criteria);

        if (nameMatch.Success && typeMatch.Success)
        {
            var expectedName = nameMatch.Groups[1].Value;
            var expectedType = typeMatch.Groups[1].Value;
            return exchange.Name == expectedName && 
                   exchange.Type.ToString().ToLower() == expectedType.ToLower();
        }

        return false;
    }
} 