using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Tasks;
using TaskManagement.Application.Tasks.Abstractions;

namespace TaskManagement.Application;

public static class DependencyInjection
{
    /// <summary>Регистрирует прикладные сервисы и валидаторы. Инфраструктурные зависимости (репозиторий, email) регистрируются отдельно.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
