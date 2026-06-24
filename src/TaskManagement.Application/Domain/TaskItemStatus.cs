namespace TaskManagement.Application.Domain;

/// <summary>Жизненный цикл задачи. Допустимые переходы задаёт TaskStatusPolicy.</summary>
public enum TaskItemStatus
{
    New,
    InProgress,
    Review,
    Done
}
