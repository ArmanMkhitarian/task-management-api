using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Middleware;
using TaskManagement.Api.Validation;
using TaskManagement.Application;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Postgres.");

builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        // Enum — строками в JSON.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Task Management API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Авто-применение миграций при старте (демо/проверка — зафиксировано в AI_DECISIONS).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Единый обработчик ошибок — самым первым в конвейере.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger доступен всегда — внутренний сервис, удобно для проверки.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString()
            })
        };
        await context.Response.WriteAsJsonAsync(payload);
    }
});

app.Run();

// Делает класс Program доступным для интеграционных тестов (WebApplicationFactory).
public partial class Program;
