using Companion.Core.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace Companion.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(CompanionDbContext context)
    {
        // Only seed if no users exist
        if (!await context.Users.AnyAsync())
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
            await context.SaveChangesAsync();
        }
    }
} 