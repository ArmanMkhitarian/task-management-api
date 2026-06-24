using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>Полное обновление редактируемых полей задачи. Статус сюда не входит — он меняется через PATCH /status.</summary>
public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    string? AssigneeEmail);
