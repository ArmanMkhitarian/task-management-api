using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Tasks.Abstractions;

/// <summary>Отправка уведомлений. Реализация — мок (лог) в Infrastructure.</summary>
public interface IEmailSender
{
    /// <summary>Уведомление исполнителю о назначении задачи. Вызывается синхронно при заполнении/смене AssigneeEmail.</summary>
    Task SendAssignmentNotificationAsync(string assigneeEmail, TaskItem task, CancellationToken cancellationToken);
}
