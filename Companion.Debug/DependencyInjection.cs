using Companion.Debug.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Companion.Debug;

/// <summary>
/// Extension methods for setting up debugging services in an IServiceCollection
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds RabbitMQ debugging services to the service collection
    /// </summary>
    public static IServiceCollection AddRabbitMqDebugging(this IServiceCollection services)
    {
        services.AddScoped<IDebugService, DebugService>();
        return services;
    }
} 