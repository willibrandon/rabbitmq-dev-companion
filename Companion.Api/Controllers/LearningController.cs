using Microsoft.AspNetCore.Mvc;
using Companion.Learning.Services;
using Companion.Learning.Models;

namespace Companion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LearningController : ControllerBase
{
    private readonly ILearningService _learningService;
    private readonly ILogger<LearningController> _logger;

    public LearningController(ILearningService learningService, ILogger<LearningController> logger)
    {
        _learningService = learningService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LearningModule>>> GetModules()
    {
        try
        {
            var modules = await _learningService.GetModulesAsync();
            return Ok(modules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning modules");
            return StatusCode(500, "An error occurred while retrieving learning modules");
        }
    }

    [HttpGet("{moduleId}")]
    public async Task<ActionResult<LearningModule>> GetModule(string moduleId)
    {
        try
        {
            var module = await _learningService.GetModuleByIdAsync(moduleId);
            if (module == null)
            {
                return NotFound($"Learning module with ID {moduleId} not found");
            }
            return Ok(module);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning module {ModuleId}", moduleId);
            return StatusCode(500, "An error occurred while retrieving the learning module");
        }
    }

    [HttpPost("{moduleId}/steps/{stepIndex}/validate")]
    public async Task<ActionResult<bool>> ValidateStepProgress(string moduleId, int stepIndex, [FromBody] ValidationRequest validationRequest)
    {
        try
        {
            var result = await _learningService.ValidateStepProgressAsync(moduleId, stepIndex, validationRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating step progress for module {ModuleId}, step {StepIndex}", moduleId, stepIndex);
            return StatusCode(500, "An error occurred while validating step progress");
        }
    }

    [HttpPost("{moduleId}/steps/{stepIndex}/progress")]
    public async Task<ActionResult> UpdateProgress(string moduleId, int stepIndex)
    {
        try
        {
            await _learningService.UpdateProgressAsync(moduleId, stepIndex);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for module {ModuleId}, step {StepIndex}", moduleId, stepIndex);
            return StatusCode(500, "An error occurred while updating progress");
        }
    }
} 