using Microsoft.Extensions.Logging;
using TaskManagement.Application.Domain;
using TaskManagement.Application.Tasks.Abstractions;

namespace TaskManagement.Infrastructure.Email;

/// <summary>Мок отправки email: реальных писем нет, факт уведомления пишется в лог.</summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAssignmentNotificationAsync(string assigneeEmail, TaskItem task, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Email (mock): задача {TaskId} \"{Title}\" назначена на {AssigneeEmail}",
            task.Id, task.Title, assigneeEmail);

        return Task.CompletedTask;
    }
}
