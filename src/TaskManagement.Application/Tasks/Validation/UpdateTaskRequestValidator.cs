using FluentValidation;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Validation;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(200).WithMessage("Название не должно превышать 200 символов.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Описание не должно превышать 4000 символов.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Недопустимое значение приоритета.");

        RuleFor(x => x.AssigneeEmail)
            .MaximumLength(254).WithMessage("Email исполнителя не должен превышать 254 символа.")
            .EmailAddress().WithMessage("Некорректный формат email исполнителя.")
            .When(x => !string.IsNullOrWhiteSpace(x.AssigneeEmail));
    }
}
