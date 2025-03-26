using Companion.Debug.Models;
using Companion.Debug.Services;
using Microsoft.AspNetCore.Mvc;

namespace Companion.Api.Controllers;

/// <summary>
/// API endpoints for debugging RabbitMQ message flows
/// </summary>
[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly IDebugService _debugService;

    /// <summary>
    /// Initializes a new instance of the DebugController class
    /// </summary>
    public DebugController(IDebugService debugService)
    {
        _debugService = debugService;
    }

    /// <summary>
    /// Gets dead-lettered messages from specified queue or all DLQs
    /// </summary>
    /// <param name="queueName">Optional queue name to check for dead letters</param>
    /// <param name="limit">Maximum number of messages to retrieve</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A list of dead-lettered messages</returns>
    [HttpGet("dead-letters")]
    [ProducesResponseType(typeof(IReadOnlyList<DeadLetteredMessage>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DeadLetteredMessage>>> GetDeadLetters(
        [FromQuery] string? queueName = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var messages = await _debugService.GetDeadLetteredMessagesAsync(queueName, limit, cancellationToken);
        return Ok(messages);
    }

    /// <summary>
    /// Traces a message through the RabbitMQ system
    /// </summary>
    /// <param name="messageId">The message ID to trace</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The message trace information</returns>
    [HttpGet("trace/{messageId}")]
    [ProducesResponseType(typeof(MessageTrace), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageTrace>> TraceMessage(
        string messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            var trace = await _debugService.TraceMessageAsync(messageId, cancellationToken);
            return Ok(trace);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Message {messageId} not found");
        }
    }

    /// <summary>
    /// Requeues a dead-lettered message back to its original queue
    /// </summary>
    /// <param name="messageId">The message ID to requeue</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    [HttpPost("requeue/{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequeueMessage(
        string messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _debugService.RequeueDeadLetteredMessageAsync(messageId, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Message {messageId} not found in any dead letter queue");
        }
    }
} 