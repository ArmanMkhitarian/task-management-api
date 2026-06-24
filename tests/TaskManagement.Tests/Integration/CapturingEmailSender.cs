using TaskManagement.Application.Tasks.Abstractions;

namespace TaskManagement.Tests.Integration;

/// <summary>
/// Тестовый дубль <see cref="IEmailSender"/>: вместо лога запоминает факты отправки,
/// чтобы тесты могли проверить «email при назначении». Регистрируется синглтоном.
/// </summary>
public sealed class CapturingEmailSender : IEmailSender
{
    private readonly object _lock = new();
    private readonly List<SentEmail> _sent = new();

    public IReadOnlyList<SentEmail> Sent
    {
        get { lock (_lock) { return _sent.ToList(); } }
    }

    public Task SendAssignmentNotificationAsync(string assigneeEmail, TaskItem task, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _sent.Add(new SentEmail(assigneeEmail, task.Id, task.Title));
        }

        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_lock) { _sent.Clear(); }
    }
}

public sealed record SentEmail(string AssigneeEmail, Guid TaskId, string Title);
