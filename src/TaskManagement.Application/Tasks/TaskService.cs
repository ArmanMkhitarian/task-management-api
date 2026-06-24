using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Domain;
using TaskManagement.Application.Tasks.Abstractions;
using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Application.Tasks.Mapping;

namespace TaskManagement.Application.Tasks;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IEmailSender _emailSender;

    public TaskService(ITaskRepository repository, IEmailSender emailSender)
    {
        _repository = repository;
        _emailSender = emailSender;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = TaskItem.Create(request.Title, request.Description, request.Priority, request.AssigneeEmail);

        _repository.Add(task);
        await _repository.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(task.AssigneeEmail))
        {
            await _emailSender.SendAssignmentNotificationAsync(task.AssigneeEmail, task, cancellationToken);
        }

        return task.ToResponse();
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken)
                   ?? throw NotFoundException.ForTask(id);

        return task.ToResponse();
    }

    public async Task<PagedResult<TaskResponse>> GetListAsync(TaskListFilter filter, CancellationToken cancellationToken)
    {
        // Нормализуем email фильтра под канонично хранимое значение — сравнение в SQL точное.
        var normalizedFilter = filter with { AssigneeEmail = EmailNormalizer.Normalize(filter.AssigneeEmail) };

        var (items, totalCount) = await _repository.GetListAsync(normalizedFilter, cancellationToken);

        var responses = items.Select(t => t.ToResponse()).ToList();

        return new PagedResult<TaskResponse>(responses, totalCount, filter.Limit, filter.Offset);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken)
                   ?? throw NotFoundException.ForTask(id);

        var previousAssignee = task.AssigneeEmail;

        task.UpdateDetails(request.Title, request.Description, request.Priority, request.AssigneeEmail);
        await _repository.SaveChangesAsync(cancellationToken);

        if (IsNewAssignment(previousAssignee, task.AssigneeEmail))
        {
            await _emailSender.SendAssignmentNotificationAsync(task.AssigneeEmail!, task, cancellationToken);
        }

        return task.ToResponse();
    }

    public async Task<TaskResponse> ChangeStatusAsync(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken)
                   ?? throw NotFoundException.ForTask(id);

        // Тот же статус — идемпотентный no-op: без записи и без уведомлений.
        if (task.Status == request.Status)
        {
            return task.ToResponse();
        }

        TaskStatusPolicy.EnsureCanTransition(task.Status, request.Status);

        task.ChangeStatus(request.Status);
        await _repository.SaveChangesAsync(cancellationToken);

        return task.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken)
                   ?? throw NotFoundException.ForTask(id);

        _repository.Remove(task);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Уведомляем, только если задача стала назначена на нового непустого исполнителя.
    /// Email уже нормализован в entity (lower), поэтому сравнение точное (ordinal).
    /// </summary>
    private static bool IsNewAssignment(string? previous, string? current) =>
        current is not null && !string.Equals(previous, current, StringComparison.Ordinal);
}
