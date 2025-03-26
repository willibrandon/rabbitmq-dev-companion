using Companion.Core.Models;
using System.Text.Json;

namespace Companion.Tests.Models;

public class TopologyTests
{
    [Fact]
    public void Topology_CreatesWithDefaultValues()
    {
        // Arrange & Act
        var topology = new Topology();

        // Assert
        Assert.NotNull(topology.Id);
        Assert.NotEqual(Guid.Empty.ToString(), topology.Id);
        Assert.Empty(topology.Name);
        Assert.Empty(topology.Description);
        Assert.Empty(topology.Exchanges);
        Assert.Empty(topology.Queues);
        Assert.Empty(topology.Bindings);
        Assert.True(topology.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.True(topology.UpdatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Topology_SerializesAndDeserializesCorrectly()
    {
        // Arrange
        var topology = new Topology
        {
            Name = "Test Topology",
            Description = "A test topology",
            Exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Name = "test.exchange",
                    Type = ExchangeType.Direct,
                    Durable = true
                }
            },
            Queues = new List<Queue>
            {
                new Queue
                {
                    Name = "test.queue",
                    Durable = true
                }
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
        var json = JsonSerializer.Serialize(topology);
        var deserializedTopology = JsonSerializer.Deserialize<Topology>(json);

        // Assert
        Assert.NotNull(deserializedTopology);
        Assert.Equal(topology.Name, deserializedTopology.Name);
        Assert.Equal(topology.Description, deserializedTopology.Description);
        Assert.Single(deserializedTopology.Exchanges);
        Assert.Single(deserializedTopology.Queues);
        Assert.Single(deserializedTopology.Bindings);
        
        var exchange = deserializedTopology.Exchanges[0];
        Assert.Equal("test.exchange", exchange.Name);
        Assert.Equal(ExchangeType.Direct, exchange.Type);
        Assert.True(exchange.Durable);

        var queue = deserializedTopology.Queues[0];
        Assert.Equal("test.queue", queue.Name);
        Assert.True(queue.Durable);

        var binding = deserializedTopology.Bindings[0];
        Assert.Equal("test.exchange", binding.SourceExchange);
        Assert.Equal("test.queue", binding.DestinationQueue);
        Assert.Equal("test.key", binding.RoutingKey);
    }
} 