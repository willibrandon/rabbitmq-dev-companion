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
    public async Task ValidateTopology_WithValidTopology_ReturnsTrue()
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
        var result = await _service.ValidateTopology(topology);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateTopology_WithMissingName_ReturnsFalse()
    {
        // Arrange
        var topology = new Topology();

        // Act
        var result = await _service.ValidateTopology(topology);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTopology_WithInvalidExchangeName_ReturnsFalse()
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
        var result = await _service.ValidateTopology(topology);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTopology_WithFanoutExchangeAndRoutingKey_ReturnsFalse()
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
        var result = await _service.ValidateTopology(topology);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTopology_WithOrphanedQueue_ReturnsTrue()
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
        var result = await _service.ValidateTopology(topology);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task NormalizeTopology_NormalizesNames()
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
        var normalized = await _service.NormalizeTopology(topology);

        // Assert
        Assert.Equal("test topology", normalized.Name);
        Assert.Equal("test.exchange", normalized.Exchanges[0].Name);
        Assert.Equal("test.queue", normalized.Queues[0].Name);
    }

    [Fact]
    public async Task ValidateTopology_WithInvalidBinding_ReturnsFalse()
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
        var result = await _service.ValidateTopology(topology);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SaveTopologyAsync_WithValidTopology_SavesAndReturnsTopology()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "Test Topology",
            Exchanges = new List<Exchange>
            {
                new Exchange { Name = "test.exchange", Type = ExchangeType.Direct }
            }
        };

        _mockRepository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<Topology>()))
            .ReturnsAsync((Topology t) => t);

        // Act
        var result = await _service.SaveTopologyAsync(topology);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test topology", result.Name);
        _mockRepository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<Topology>()), Times.Once);
    }

    [Fact]
    public async Task SaveTopologyAsync_WithInvalidTopology_ThrowsException()
    {
        // Arrange
        var topology = new Topology(); // Invalid topology with no name

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.SaveTopologyAsync(topology));
        
        _mockRepository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<Topology>()), Times.Never);
    }

    [Fact]
    public async Task GetTopologyByIdAsync_ReturnsTopology()
    {
        // Arrange
        var expectedTopology = new Topology { Name = "Test" };
        _mockRepository.Setup(r => r.GetByIdAsync("test-id"))
            .ReturnsAsync(expectedTopology);

        // Act
        var result = await _service.GetTopologyByIdAsync("test-id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTopology.Name, result.Name);
    }

    [Fact]
    public async Task GetFromBrokerAsync_ThrowsNotImplementedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => 
            _service.GetFromBrokerAsync());
    }

    [Fact]
    public async Task CheckBrokerHealthAsync_ThrowsNotImplementedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => 
            _service.CheckBrokerHealthAsync());
    }
} 