using Companion.Core.Models;
using Companion.Core.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace Companion.Infrastructure.Data;

public class CompanionDbContext : DbContext
{
    public CompanionDbContext(DbContextOptions<CompanionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Topology> Topologies { get; set; } = null!;
    public DbSet<Exchange> Exchanges { get; set; } = null!;
    public DbSet<Queue> Queues { get; set; } = null!;
    public DbSet<Binding> Bindings { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Use a collation compatible with the template database
        // modelBuilder.UseCollation("und-x-icu");

        modelBuilder.Entity<Topology>(entity =>
        {
            entity.ToTable("topologies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Metadata).HasColumnName("metadata")
                .HasColumnType("jsonb");
            
            entity.HasMany(e => e.Exchanges)
                .WithOne()
                .HasForeignKey("TopologyId")
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Queues)
                .WithOne()
                .HasForeignKey("TopologyId")
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Bindings)
                .WithOne()
                .HasForeignKey("TopologyId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Exchange>(entity =>
        {
            entity.ToTable("exchanges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Durable).HasColumnName("durable");
            entity.Property(e => e.AutoDelete).HasColumnName("auto_delete");
            entity.Property(e => e.Internal).HasColumnName("internal");
            entity.Property(e => e.Arguments).HasColumnName("arguments")
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<Queue>(entity =>
        {
            entity.ToTable("queues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Durable).HasColumnName("durable");
            entity.Property(e => e.AutoDelete).HasColumnName("auto_delete");
            entity.Property(e => e.Exclusive).HasColumnName("exclusive");
            entity.Property(e => e.Arguments).HasColumnName("arguments")
                .HasColumnType("jsonb");
            entity.Property(e => e.MaxLength).HasColumnName("max_length");
            entity.Property(e => e.MessageTtl).HasColumnName("message_ttl");
            entity.Property(e => e.DeadLetterExchange).HasColumnName("dead_letter_exchange");
            entity.Property(e => e.DeadLetterRoutingKey).HasColumnName("dead_letter_routing_key");
        });

        modelBuilder.Entity<Binding>(entity =>
        {
            entity.ToTable("bindings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceExchange).HasColumnName("source_exchange").IsRequired();
            entity.Property(e => e.DestinationQueue).HasColumnName("destination_queue").IsRequired();
            entity.Property(e => e.RoutingKey).HasColumnName("routing_key");
            entity.Property(e => e.Arguments).HasColumnName("arguments")
                .HasColumnType("jsonb");
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
} 