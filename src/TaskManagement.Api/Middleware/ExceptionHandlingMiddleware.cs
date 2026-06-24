using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;

namespace TaskManagement.Api.Middleware;

/// <summary>Единая точка обработки исключений: маппинг в ProblemDetails (RFC 7807).</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var problem = MapToProblem(exception, context.Request.Path);

        // Детали 5xx не раскрываем клиенту, но логируем целиком.
        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Необработанное исключение при обработке запроса");
        }

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }

    private static ProblemDetails MapToProblem(Exception exception, string instance)
    {
        switch (exception)
        {
            case NotFoundException notFound:
                return Build(StatusCodes.Status404NotFound, "Ресурс не найден", notFound.Message, instance);

            case InvalidStatusTransitionException transition:
                var problem = Build(StatusCodes.Status409Conflict, "Недопустимый переход статуса", transition.Message, instance);
                problem.Extensions["allowedTransitions"] = transition.AllowedTransitions.Select(s => s.ToString()).ToArray();
                return problem;

            case DbUpdateConcurrencyException:
                return Build(
                    StatusCodes.Status409Conflict,
                    "Конфликт параллельного изменения",
                    "Задача была изменена другим запросом. Обновите данные и повторите операцию.",
                    instance);

            default:
                return Build(
                    StatusCodes.Status500InternalServerError,
                    "Внутренняя ошибка сервера",
                    "Произошла непредвиденная ошибка.",
                    instance);
        }
    }

    private static ProblemDetails Build(int status, string title, string detail, string instance) => new()
    {
        Status = status,
        Title = title,
        Detail = detail,
        Type = $"https://httpstatuses.io/{status}",
        Instance = instance
    };
}
