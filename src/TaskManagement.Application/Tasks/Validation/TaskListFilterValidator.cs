using FluentValidation;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Validation;

public class TaskListFilterValidator : AbstractValidator<TaskListFilter>
{
    public TaskListFilterValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Недопустимое значение статуса.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Недопустимое значение приоритета.")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.AssigneeEmail)
            .MaximumLength(254).WithMessage("Email исполнителя не должен превышать 254 символа.")
            .When(x => !string.IsNullOrWhiteSpace(x.AssigneeEmail));

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit должен быть в диапазоне 1..100.");

        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0).WithMessage("Offset не может быть отрицательным.");
    }
}
