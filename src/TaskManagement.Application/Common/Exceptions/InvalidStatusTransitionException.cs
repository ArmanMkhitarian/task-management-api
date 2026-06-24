using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Common.Exceptions;

/// <summary>Запрещённый переход статуса. Маппится в HTTP 409 (middleware на этапе 5).</summary>
public sealed class InvalidStatusTransitionException : Exception
{
    public TaskItemStatus From { get; }
    public TaskItemStatus To { get; }
    public IReadOnlyCollection<TaskItemStatus> AllowedTransitions { get; }

    public InvalidStatusTransitionException(
        TaskItemStatus from,
        TaskItemStatus to,
        IReadOnlyCollection<TaskItemStatus> allowedTransitions)
        : base(BuildMessage(from, to, allowedTransitions))
    {
        From = from;
        To = to;
        AllowedTransitions = allowedTransitions;
    }

    private static string BuildMessage(
        TaskItemStatus from,
        TaskItemStatus to,
        IReadOnlyCollection<TaskItemStatus> allowed)
    {
        return allowed.Count == 0
            ? $"Переход '{from}' → '{to}' запрещён: '{from}' — терминальный статус."
            : $"Переход '{from}' → '{to}' запрещён. Допустимые переходы из '{from}': {string.Join(", ", allowed)}.";
    }
}
