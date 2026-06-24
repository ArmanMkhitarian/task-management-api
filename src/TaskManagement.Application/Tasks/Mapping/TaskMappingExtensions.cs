using TaskManagement.Application.Domain;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Mapping;

/// <summary>Ручной маппинг entity → DTO (без AutoMapper).</summary>
public static class TaskMappingExtensions
{
    public static TaskResponse ToResponse(this TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority,
        task.AssigneeEmail,
        task.CreatedAt,
        task.UpdatedAt);
}
