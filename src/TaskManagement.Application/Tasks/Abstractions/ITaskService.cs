using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Abstractions;

/// <summary>Прикладные операции над задачами. Используется контроллерами.</summary>
public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);

    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<TaskResponse>> GetListAsync(TaskListFilter filter, CancellationToken cancellationToken);

    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken);

    Task<TaskResponse> ChangeStatusAsync(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
