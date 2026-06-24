using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>Представление задачи наружу. Entity клиенту не отдаём.</summary>
public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    string? AssigneeEmail,
    DateTime CreatedAt,
    DateTime UpdatedAt);
