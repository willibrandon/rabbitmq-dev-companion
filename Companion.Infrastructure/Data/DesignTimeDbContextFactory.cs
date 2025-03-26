using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Companion.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CompanionDbContext>
{
    public CompanionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanionDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=companiondb;Username=postgres;Password=postgres");

        return new CompanionDbContext(optionsBuilder.Options);
    }
} 