namespace Companion.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for RabbitMQ Management API
/// </summary>
public class RabbitMqSettings
{
    /// <summary>
    /// Gets or sets the base URL for the RabbitMQ Management API (e.g., http://localhost:15672/api)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username for authentication
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the virtual host to connect to (defaults to '/')
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the timeout in seconds for API requests (defaults to 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates the settings
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required settings are missing or invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("BaseUrl is required", nameof(BaseUrl));
        
        if (string.IsNullOrWhiteSpace(UserName))
            throw new ArgumentException("UserName is required", nameof(UserName));
        
        if (string.IsNullOrWhiteSpace(Password))
            throw new ArgumentException("Password is required", nameof(Password));

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            throw new ArgumentException("BaseUrl must be a valid URI", nameof(BaseUrl));
    }
} 