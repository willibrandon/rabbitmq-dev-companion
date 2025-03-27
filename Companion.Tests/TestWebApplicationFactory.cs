using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Companion.Infrastructure.Data;
using Companion.Core.Models.Auth;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Companion.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CompanionDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database
            services.AddDbContext<CompanionDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb")
                    .EnableSensitiveDataLogging();
            });

            // Create a scope to obtain a reference to the database context
            using var scope = services.BuildServiceProvider().CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<CompanionDbContext>();

            // Ensure database is created and clean
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed test data
            SeedTestData(context);
        });

        builder.UseEnvironment("Testing");
    }

    private void SeedTestData(CompanionDbContext context)
    {
        // Add admin user
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        // Add editor user
        var editorUser = new User
        {
            Username = "editor",
            Email = "editor@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("editor123"),
            Role = "Editor",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        context.Users.Add(editorUser);
        context.SaveChanges();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Remove the app's DbContext configuration
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CompanionDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database
            services.AddDbContext<CompanionDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb")
                    .EnableSensitiveDataLogging();
            });
        });

        return base.CreateHost(builder);
    }
} 