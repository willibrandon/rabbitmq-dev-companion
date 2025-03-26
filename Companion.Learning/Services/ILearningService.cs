using Companion.Learning.Models;

namespace Companion.Learning.Services;

/// <summary>
/// Service for managing learning modules and user progress
/// </summary>
public interface ILearningService
{
    /// <summary>
    /// Gets all available learning modules
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A list of learning modules</returns>
    Task<IReadOnlyList<LearningModule>> GetModulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific learning module by ID
    /// </summary>
    /// <param name="moduleId">The ID of the module to retrieve</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The learning module if found, null otherwise</returns>
    Task<LearningModule?> GetModuleByIdAsync(string moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a user's progress in a module step
    /// </summary>
    /// <param name="moduleId">The ID of the module</param>
    /// <param name="stepIndex">The index of the step to validate</param>
    /// <param name="validationRequest">The validation request containing topology ID and additional data</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>True if the step is completed successfully, false otherwise</returns>
    Task<bool> ValidateStepProgressAsync(
        string moduleId,
        int stepIndex,
        ValidationRequest validationRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's progress in a module
    /// </summary>
    /// <param name="moduleId">The ID of the module</param>
    /// <param name="stepIndex">The index of the completed step</param>
    /// <param name="userId">The ID of the user (optional)</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task UpdateProgressAsync(
        string moduleId,
        int stepIndex,
        string? userId = null,
        CancellationToken cancellationToken = default);
} 