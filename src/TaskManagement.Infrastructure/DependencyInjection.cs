using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Tasks.Abstractions;
using TaskManagement.Infrastructure.Email;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Регистрирует DbContext (PostgreSQL), репозиторий и email-мок. Строку подключения передаёт Api из конфигурации.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
