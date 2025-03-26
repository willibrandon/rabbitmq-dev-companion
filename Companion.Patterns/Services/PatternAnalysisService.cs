using Companion.Core.Models;
using Companion.Core.Services;

namespace Companion.Patterns.Services;

/// <summary>
/// Implementation of the pattern analysis service
/// </summary>
public class PatternAnalysisService : IPatternAnalysisService
{
    /// <inheritdoc />
    public AnalysisResult AnalyzeTopology(Topology topology)
    {
        var result = new AnalysisResult();

        CheckUnboundExchanges(topology, result);
        CheckUnboundQueues(topology, result);
        CheckUnboundedQueues(topology, result);
        CheckDeadLetterQueues(topology, result);
        CheckQueueNaming(topology, result);
        CheckExchangeTypes(topology, result);
        CheckRoutingPatterns(topology, result);

        return result;
    }

    private void CheckUnboundExchanges(Topology topology, AnalysisResult result)
    {
        var boundExchanges = topology.Bindings.Select(b => b.SourceExchange).Distinct();
        var unboundExchanges = topology.Exchanges
            .Where(e => e.Name != "" && !boundExchanges.Contains(e.Name));

        foreach (var exchange in unboundExchanges)
        {
            result.Findings.Add(new AnalysisFinding
            {
                Type = AnalysisFindingType.Warning,
                Message = $"Exchange '{exchange.Name}' has no bindings",
                SeverityLevel = 2,
                Recommendations = new List<string>
                {
                    $"Consider adding bindings to exchange '{exchange.Name}' or remove it if unused",
                    "Unbound exchanges can lead to message loss if publishers are using them"
                }
            });
        }
    }

    private void CheckUnboundQueues(Topology topology, AnalysisResult result)
    {
        var boundQueues = topology.Bindings.Select(b => b.DestinationQueue).Distinct();
        var unboundQueues = topology.Queues
            .Where(q => !boundQueues.Contains(q.Name));

        foreach (var queue in unboundQueues)
        {
            result.Findings.Add(new AnalysisFinding
            {
                Type = AnalysisFindingType.Warning,
                Message = $"Queue '{queue.Name}' is not bound to any exchange",
                SeverityLevel = 2,
                Recommendations = new List<string>
                {
                    $"Bind queue '{queue.Name}' to an appropriate exchange",
                    "Unbound queues will not receive any messages"
                }
            });
        }
    }

    private void CheckUnboundedQueues(Topology topology, AnalysisResult result)
    {
        var unboundedQueues = topology.Queues
            .Where(q => !q.MaxLength.HasValue);

        foreach (var queue in unboundedQueues)
        {
            result.Findings.Add(new AnalysisFinding
            {
                Type = AnalysisFindingType.Info,
                Message = $"Queue '{queue.Name}' has no message limit (x-max-length)",
                SeverityLevel = 1,
                Recommendations = new List<string>
                {
                    $"Consider setting a maximum length for queue '{queue.Name}'",
                    "Unbounded queues can consume unlimited memory if producers outpace consumers",
                    "Use x-max-length argument to set a safe upper bound"
                }
            });
        }
    }

    private void CheckDeadLetterQueues(Topology topology, AnalysisResult result)
    {
        var queuesWithDLX = topology.Queues
            .Where(q => !string.IsNullOrEmpty(q.DeadLetterExchange));

        foreach (var queue in queuesWithDLX)
        {
            // Check if the DLX exists
            var dlx = topology.Exchanges.FirstOrDefault(e => e.Name == queue.DeadLetterExchange);
            if (dlx == null)
            {
                result.Findings.Add(new AnalysisFinding
                {
                    Type = AnalysisFindingType.Error,
                    Message = $"Dead letter exchange '{queue.DeadLetterExchange}' for queue '{queue.Name}' does not exist",
                    SeverityLevel = 3,
                    Recommendations = new List<string>
                    {
                        $"Create the dead letter exchange '{queue.DeadLetterExchange}'",
                        "Ensure the DLX has appropriate bindings to handle dead-lettered messages"
                    }
                });
            }
        }

        // Check queues without DLX
        var queuesWithoutDLX = topology.Queues
            .Where(q => string.IsNullOrEmpty(q.DeadLetterExchange));

        foreach (var queue in queuesWithoutDLX)
        {
            result.Findings.Add(new AnalysisFinding
            {
                Type = AnalysisFindingType.Info,
                Message = $"Queue '{queue.Name}' has no dead letter exchange configured",
                SeverityLevel = 1,
                Recommendations = new List<string>
                {
                    "Consider configuring a dead letter exchange for handling failed messages",
                    "Dead letter exchanges help prevent message loss and aid in debugging"
                }
            });
        }
    }

    private void CheckQueueNaming(Topology topology, AnalysisResult result)
    {
        foreach (var queue in topology.Queues)
        {
            if (!queue.Name.Contains("."))
            {
                result.Findings.Add(new AnalysisFinding
                {
                    Type = AnalysisFindingType.Info,
                    Message = $"Queue '{queue.Name}' does not follow dot-separated naming convention",
                    SeverityLevel = 1,
                    Recommendations = new List<string>
                    {
                        "Consider using dot-separated names (e.g., 'service.entity.action')",
                        "Consistent naming helps with organization and topic-based routing"
                    }
                });
            }
        }
    }

    private void CheckExchangeTypes(Topology topology, AnalysisResult result)
    {
        var topicExchanges = topology.Exchanges
            .Where(e => e.Type == ExchangeType.Topic);

        foreach (var exchange in topicExchanges)
        {
            var bindings = topology.Bindings
                .Where(b => b.SourceExchange == exchange.Name);

            if (!bindings.Any(b => b.RoutingKey?.Contains("#") == true || 
                                 b.RoutingKey?.Contains("*") == true))
            {
                result.Findings.Add(new AnalysisFinding
                {
                    Type = AnalysisFindingType.Info,
                    Message = $"Topic exchange '{exchange.Name}' has no wildcard bindings",
                    SeverityLevel = 1,
                    Recommendations = new List<string>
                    {
                        "Consider using direct exchange if wildcards are not needed",
                        "Topic exchanges are most useful with wildcard routing patterns"
                    }
                });
            }
        }
    }

    private void CheckRoutingPatterns(Topology topology, AnalysisResult result)
    {
        var queueGroups = topology.Bindings
            .GroupBy(b => b.DestinationQueue)
            .Where(g => g.Count() > 1);

        foreach (var group in queueGroups)
        {
            result.Findings.Add(new AnalysisFinding
            {
                Type = AnalysisFindingType.Info,
                Message = $"Queue '{group.Key}' is bound to multiple exchanges ({group.Count()})",
                SeverityLevel = 1,
                Recommendations = new List<string>
                {
                    "Multiple bindings can make message flow harder to track",
                    "Consider if this is intentional or if the routing could be simplified"
                }
            });
        }
    }
} 