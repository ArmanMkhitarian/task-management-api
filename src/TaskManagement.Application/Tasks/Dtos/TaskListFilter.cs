using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Фильтры и пагинация для списка задач. Все фильтры опциональны и комбинируются.
/// Обычный объект Application — без зависимости от EF.
/// </summary>
public record TaskListFilter
{
    public TaskItemStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public string? AssigneeEmail { get; init; }
    public int Limit { get; init; } = 20;
    public int Offset { get; init; } = 0;
}
