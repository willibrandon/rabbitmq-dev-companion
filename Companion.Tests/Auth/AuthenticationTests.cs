using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Companion.Core.Models.Auth;
using Microsoft.Extensions.DependencyInjection;
using Companion.Infrastructure.Services;

namespace Companion.Tests.Auth;

public class AuthenticationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new { Username = "admin", Password = "admin123" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token?.Token);
    }

    [Fact]
    public async Task SecuredEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/topologies");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SecuredEndpoint_WithValidToken_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAuthToken(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/topologies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithNonAdminToken_Returns403()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAuthToken(client, "editor", "editor123");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/admin/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> GetAuthToken(HttpClient client, string username = "admin", string password = "admin123")
    {
        var loginRequest = new { Username = username, Password = password };
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/auth/login", content);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return token?.Token ?? string.Empty;
    }

    private record TokenResponse(string Token);
} 