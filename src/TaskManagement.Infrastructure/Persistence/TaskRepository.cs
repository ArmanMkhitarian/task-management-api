using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Domain;
using TaskManagement.Application.Tasks.Abstractions;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Infrastructure.Persistence;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    // Без AsNoTracking: сущность может изменяться/удаляться в рамках того же запроса.
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetListAsync(
        TaskListFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _db.Tasks.AsNoTracking();

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.AssigneeEmail))
        {
            // Email хранится нормализованным (lower) — сравнение точное.
            query = query.Where(t => t.AssigneeEmail == filter.AssigneeEmail);
        }

        // Count по отфильтрованному запросу до пагинации.
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Skip(filter.Offset)
            .Take(filter.Limit)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(TaskItem task) => _db.Tasks.Add(task);

    public void Remove(TaskItem task) => _db.Tasks.Remove(task);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
