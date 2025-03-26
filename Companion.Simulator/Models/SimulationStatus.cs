namespace Companion.Simulator.Models;

/// <summary>
/// Status of a running or completed simulation
/// </summary>
public class SimulationStatus
{
    /// <summary>
    /// Gets or sets the unique identifier of the simulation
    /// </summary>
    public string SimulationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current state of the simulation
    /// </summary>
    public SimulationState State { get; set; }

    /// <summary>
    /// Gets or sets the total number of messages published
    /// </summary>
    public int MessagesPublished { get; set; }

    /// <summary>
    /// Gets or sets the total number of messages consumed
    /// </summary>
    public int MessagesConsumed { get; set; }

    /// <summary>
    /// Gets or sets the number of failed messages (e.g., rejected by consumers)
    /// </summary>
    public int FailedMessages { get; set; }

    /// <summary>
    /// Gets or sets the average publish rate per second
    /// </summary>
    public double PublishRatePerSecond { get; set; }

    /// <summary>
    /// Gets or sets the average consume rate per second
    /// </summary>
    public double ConsumeRatePerSecond { get; set; }

    /// <summary>
    /// Gets or sets the start time of the simulation
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the simulation (if completed)
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets any error message if the simulation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Possible states of a simulation
/// </summary>
public enum SimulationState
{
    /// <summary>
    /// The simulation is initializing
    /// </summary>
    Initializing,

    /// <summary>
    /// The simulation is running
    /// </summary>
    Running,

    /// <summary>
    /// The simulation has completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// The simulation was stopped by the user
    /// </summary>
    Stopped,

    /// <summary>
    /// The simulation failed
    /// </summary>
    Failed
} 