using Companion.Core.Models;
using Companion.Core.Repositories;
using Companion.Core.Services;
using Moq;

namespace Companion.Tests.Services;

public class TopologyServiceTests
{
    private readonly Mock<ITopologyRepository> _mockRepository;
    private readonly TopologyService _service;

    public TopologyServiceTests()
    {
        _mockRepository = new Mock<ITopologyRepository>();
        _service = new TopologyService(_mockRepository.Object);
    }

    [Fact]
    public void ValidateTopology_WithValidTopology_ReturnsSuccess()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "Test Topology",
            Exchanges = new List<Exchange>
            {
                new Exchange { Name = "test.exchange", Type = ExchangeType.Direct }
            },
            Queues = new List<Queue>
            {
                new Queue { Name = "test.queue" }
            },
            Bindings = new List<Binding>
            {
                new Binding
                {
                    SourceExchange = "test.exchange",
                    DestinationQueue = "test.queue",
                    RoutingKey = "test.key"
                }
            }
        };

        // Act
        var result = _service.ValidateTopology(topology);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateTopology_WithMissingName_ReturnsError()
    {
        // Arrange
        var topology = new Topology();

        // Act
        var result = _service.ValidateTopology(topology);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("name is required"));
    }

    [Fact]
    public void ValidateTopology_WithInvalidExchangeName_ReturnsError()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "Test",
            Exchanges = new List<Exchange>
            {
                new Exchange { Name = "invalid/exchange", Type = ExchangeType.Direct }
            }
        };

        // Act
        var result = _service.ValidateTopology(topology);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("invalid characters"));
    }

    [Fact]
    public void ValidateTopology_WithFanoutExchangeAndRoutingKey_ReturnsError()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "Test",
            Exchanges = new List<Exchange>
            {
                new Exchange { Name = "test.fanout", Type = ExchangeType.Fanout }
            },
            Queues = new List<Queue>
            {
                new Queue { Name = "test.queue" }
            },
            Bindings = new List<Binding>
            {
                new Binding
                {
                    SourceExchange = "test.fanout",
                    DestinationQueue = "test.queue",
                    RoutingKey = "should.not.have.routing.key"
                }
            }
        };

        // Act
        var result = _service.ValidateTopology(topology);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("should not have a routing key"));
    }

    [Fact]
    public void ValidateTopology_WithOrphanedQueue_ReturnsWarning()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "Test",
            Exchanges = new List<Exchange>
            {
                new Exchange { Name = "test.exchange", Type = ExchangeType.Direct }
            },
            Queues = new List<Queue>
            {
                new Queue { Name = "orphaned.queue" }
            }
        };

        // Act
        var result = _service.ValidateTopology(topology);

        // Assert
        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, w => w.Contains("has no bindings"));
    }

    [Fact]
    public void NormalizeTopology_NormalizesNames()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "  Test Topology  ",
            Exchanges = new List<Exchange>
            {
                new Exchange { Name = "TEST.EXCHANGE  " }
            },
            Queues = new List<Queue>
            {
                new Queue { Name = "  TEST.QUEUE" }
            }
        };

        // Act
        var normalized = _service.NormalizeTopology(topology);

        // Assert
        Assert.Equal("test topology", normalized.Name);
        Assert.Equal("test.exchange", normalized.Exchanges[0].Name);
        Assert.Equal("test.queue", normalized.Queues[0].Name);
    }

    [Fact]
    public void ValidateTopology_WithInvalidBinding_ReturnsError()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "Test Topology",
            Exchanges = new List<Exchange>
            {
                new Exchange { Name = "test.exchange", Type = ExchangeType.Direct }
            },
            Queues = new List<Queue>
            {
                new Queue { Name = "test.queue" }
            },
            Bindings = new List<Binding>
            {
                new Binding
                {
                    SourceExchange = "nonexistent.exchange",
                    DestinationQueue = "test.queue",
                    RoutingKey = "test.key"
                }
            }
        };

        // Act
        var result = _service.ValidateTopology(topology);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("nonexistent.exchange"));
    }
} 