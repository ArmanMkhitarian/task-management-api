using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Domain;

namespace TaskManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>Проставляет CreatedAt/UpdatedAt в UTC — источником времени остаётся сервер, не клиент.</summary>
    private void ApplyAuditTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<TaskItem>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(t => t.CreatedAt).CurrentValue = now;
                    entry.Property(t => t.UpdatedAt).CurrentValue = now;
                    break;
                case EntityState.Modified:
                    entry.Property(t => t.UpdatedAt).CurrentValue = now;
                    break;
            }
        }
    }
}
