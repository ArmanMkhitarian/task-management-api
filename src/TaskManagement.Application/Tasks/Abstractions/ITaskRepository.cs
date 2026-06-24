using TaskManagement.Application.Domain;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Abstractions;

/// <summary>
/// Доступ к данным задач. Специализированные методы — IQueryable наружу не отдаём.
/// Фильтрация/сортировка/пагинация выполняются в SQL внутри реализации (Infrastructure).
/// </summary>
public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Страница задач по фильтру + общее число подходящих записей (для пагинации).</summary>
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetListAsync(
        TaskListFilter filter,
        CancellationToken cancellationToken);

    void Add(TaskItem task);

    void Remove(TaskItem task);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
