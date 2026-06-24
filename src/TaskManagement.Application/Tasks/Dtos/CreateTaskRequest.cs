using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>Входные данные для создания задачи. Статус не задаётся — новая задача всегда New.</summary>
public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    string? AssigneeEmail);
