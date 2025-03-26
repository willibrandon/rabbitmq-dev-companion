using Companion.Core.Models;
using Companion.Patterns.Services;

namespace Companion.Tests.Services;

public class PatternAnalysisServiceTests
{
    private readonly PatternAnalysisService _service;

    public PatternAnalysisServiceTests()
    {
        _service = new PatternAnalysisService();
    }

    [Fact]
    public void AnalyzeTopology_WithUnboundExchange_ReturnsWarning()
    {
        // Arrange
        var topology = new Topology
        {
            Exchanges = new List<Exchange>
            {
                new() { Name = "test.exchange", Type = ExchangeType.Direct }
            },
            Bindings = new List<Binding>()
        };

        // Act
        var result = _service.AnalyzeTopology(topology);

        // Assert
        Assert.Contains(result.Findings, f => 
            f.Type == AnalysisFindingType.Warning &&
            f.Message.Contains("test.exchange") &&
            f.Message.Contains("no bindings"));
    }

    [Fact]
    public void AnalyzeTopology_WithUnboundQueue_ReturnsWarning()
    {
        // Arrange
        var topology = new Topology
        {
            Queues = new List<Queue>
            {
                new() { Name = "test.queue" }
            },
            Bindings = new List<Binding>()
        };

        // Act
        var result = _service.AnalyzeTopology(topology);

        // Assert
        Assert.Contains(result.Findings, f => 
            f.Type == AnalysisFindingType.Warning &&
            f.Message.Contains("test.queue") &&
            f.Message.Contains("not bound to any exchange"));
    }

    [Fact]
    public void AnalyzeTopology_WithUnboundedQueue_ReturnsInfo()
    {
        // Arrange
        var topology = new Topology
        {
            Queues = new List<Queue>
            {
                new() { Name = "test.queue", MaxLength = null }
            }
        };

        // Act
        var result = _service.AnalyzeTopology(topology);

        // Assert
        Assert.Contains(result.Findings, f => 
            f.Type == AnalysisFindingType.Info &&
            f.Message.Contains("test.queue") &&
            f.Message.Contains("no message limit"));
    }

    [Fact]
    public void AnalyzeTopology_WithMissingDLX_ReturnsError()
    {
        // Arrange
        var topology = new Topology
        {
            Exchanges = new List<Exchange>(),
            Queues = new List<Queue>
            {
                new() 
                { 
                    Name = "test.queue",
                    DeadLetterExchange = "test.dlx"
                }
            }
        };

        // Act
        var result = _service.AnalyzeTopology(topology);

        // Assert
        Assert.Contains(result.Findings, f => 
            f.Type == AnalysisFindingType.Error &&
            f.Message.Contains("test.dlx") &&
            f.Message.Contains("does not exist"));
    }

    [Fact]
    public void AnalyzeTopology_WithTopicExchangeNoWildcards_ReturnsInfo()
    {
        // Arrange
        var topology = new Topology
        {
            Exchanges = new List<Exchange>
            {
                new() { Name = "test.topic", Type = ExchangeType.Topic }
            },
            Bindings = new List<Binding>
            {
                new() 
                { 
                    SourceExchange = "test.topic",
                    DestinationQueue = "test.queue",
                    RoutingKey = "exact.match"
                }
            }
        };

        // Act
        var result = _service.AnalyzeTopology(topology);

        // Assert
        Assert.Contains(result.Findings, f => 
            f.Type == AnalysisFindingType.Info &&
            f.Message.Contains("test.topic") &&
            f.Message.Contains("no wildcard bindings"));
    }

    [Fact]
    public void AnalyzeTopology_WithMultipleBindingsToQueue_ReturnsInfo()
    {
        // Arrange
        var topology = new Topology
        {
            Bindings = new List<Binding>
            {
                new() 
                { 
                    SourceExchange = "exchange1",
                    DestinationQueue = "test.queue"
                },
                new() 
                { 
                    SourceExchange = "exchange2",
                    DestinationQueue = "test.queue"
                }
            }
        };

        // Act
        var result = _service.AnalyzeTopology(topology);

        // Assert
        Assert.Contains(result.Findings, f => 
            f.Type == AnalysisFindingType.Info &&
            f.Message.Contains("test.queue") &&
            f.Message.Contains("multiple exchanges"));
    }
} 