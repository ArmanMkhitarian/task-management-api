using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskManagement.Api.Validation;

/// <summary>
/// Глобальный фильтр: для каждого аргумента экшена ищет зарегистрированный IValidator&lt;T&gt;
/// и валидирует. При ошибках возвращает 400 с ValidationProblemDetails (RFC 7807).
/// Без устаревшего auto-validation из FluentValidation.AspNetCore.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            if (result.IsValid)
            {
                continue;
            }

            foreach (var error in result.Errors)
            {
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
            return;
        }

        await next();
    }
}
