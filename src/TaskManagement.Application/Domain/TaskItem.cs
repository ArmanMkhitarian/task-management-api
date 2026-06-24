using TaskManagement.Application.Common;

namespace TaskManagement.Application.Domain;

/// <summary>
/// Задача. Имя TaskItem (не Task) во избежание конфликта с System.Threading.Tasks.Task.
/// Сеттеры закрыты: состояние меняется только через доменные методы.
/// CreatedAt/UpdatedAt проставляет AppDbContext в SaveChanges (UTC).
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public string? AssigneeEmail { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Для EF (материализация). Не использовать в коде приложения.
    private TaskItem() { }

    /// <summary>Создаёт новую задачу в статусе New с последовательным Id.</summary>
    public static TaskItem Create(string title, string? description, TaskPriority priority, string? assigneeEmail)
    {
        return new TaskItem
        {
            Id = SequentialGuid.NewGuid(),
            Title = title,
            Description = description,
            Priority = priority,
            AssigneeEmail = assigneeEmail,
            Status = TaskItemStatus.New
        };
    }

    /// <summary>Обновляет редактируемые поля (без статуса — он меняется отдельно).</summary>
    public void UpdateDetails(string title, string? description, TaskPriority priority, string? assigneeEmail)
    {
        Title = title;
        Description = description;
        Priority = priority;
        AssigneeEmail = assigneeEmail;
    }

    /// <summary>Устанавливает статус. Допустимость перехода проверяет вызывающий слой (TaskStatusPolicy).</summary>
    public void ChangeStatus(TaskItemStatus newStatus)
    {
        Status = newStatus;
    }
}
