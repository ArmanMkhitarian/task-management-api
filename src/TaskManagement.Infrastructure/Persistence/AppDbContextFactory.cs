using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManagement.Infrastructure.Persistence;

/// <summary>
/// Используется только инструментами EF Core (dotnet ef) во время разработки.
/// Строка подключения здесь нужна лишь для построения модели/миграций; runtime берёт
/// настоящую строку из конфигурации Api.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=taskmanagement;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
