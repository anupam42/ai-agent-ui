using Microsoft.EntityFrameworkCore;

using UiCodeGenerator.Domain.Models;

namespace UiCodeGenerator.Infrastructure.Persistence;

public sealed class DesignStudioDbContext(DbContextOptions<DesignStudioDbContext> options)
    : DbContext(options)
{
    public DbSet<HistorySession> HistorySessions => Set<HistorySession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistorySession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SchemaName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Prompt).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.SchemaJson).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
