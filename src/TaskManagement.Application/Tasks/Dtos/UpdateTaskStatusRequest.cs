using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>Смена статуса задачи. Допустимость перехода проверяет TaskStatusPolicy.</summary>
public record UpdateTaskStatusRequest(TaskItemStatus Status);
