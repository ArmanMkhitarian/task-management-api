using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManagement.Application.Tasks.Abstractions;
using TaskManagement.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TaskManagement.Tests.Integration;

/// <summary>
/// Поднимает реальный PostgreSQL в контейнере и запускает приложение через WebApplicationFactory.
/// Один контейнер на всю коллекцию интеграционных тестов; <see cref="ResetAsync"/> чистит состояние перед каждым тестом.
/// Подменяет только email-отправитель (на capturing-дубль) — остальной граф сервисов реальный.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("taskmanagement")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    public CapturingEmailSender EmailSender { get; } = new();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // ConfigureTestServices выполняется ПОСЛЕ регистраций приложения, поэтому здесь
            // надёжно подменяем и БД (на контейнер), и email-отправитель (на дубль).
            // Перенастройка через конфигурацию ненадёжна: appsettings.json грузится позже
            // тестового ConfigureAppConfiguration и перебивает строку подключения.
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(_container.GetConnectionString()));

                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(EmailSender);
            });
        });

        // Первый CreateClient поднимает хост и применяет миграции (db.Database.Migrate в Program).
        Client = _factory.CreateClient();
    }

    /// <summary>Очищает таблицу задач и историю отправленных писем — изоляция между тестами.</summary>
    public async Task ResetAsync()
    {
        EmailSender.Clear();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE tasks");
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "integration";
}
