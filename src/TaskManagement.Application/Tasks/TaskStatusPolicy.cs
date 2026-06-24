using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Domain;

namespace TaskManagement.Application.Tasks;

/// <summary>
/// Конечный автомат статусов задачи. Единственный источник правды о допустимых переходах.
/// Переход в тот же статус считается допустимым (идемпотентный no-op).
/// </summary>
public static class TaskStatusPolicy
{
    private static readonly IReadOnlyDictionary<TaskItemStatus, IReadOnlySet<TaskItemStatus>> Allowed =
        new Dictionary<TaskItemStatus, IReadOnlySet<TaskItemStatus>>
        {
            [TaskItemStatus.New] = new HashSet<TaskItemStatus> { TaskItemStatus.InProgress },
            [TaskItemStatus.InProgress] = new HashSet<TaskItemStatus> { TaskItemStatus.Review, TaskItemStatus.New },
            [TaskItemStatus.Review] = new HashSet<TaskItemStatus> { TaskItemStatus.Done, TaskItemStatus.InProgress },
            [TaskItemStatus.Done] = new HashSet<TaskItemStatus>()
        };

    public static bool CanTransition(TaskItemStatus from, TaskItemStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var targets) && targets.Contains(to));

    public static IReadOnlyCollection<TaskItemStatus> GetAllowedTransitions(TaskItemStatus from) =>
        Allowed.TryGetValue(from, out var targets) ? targets.ToArray() : Array.Empty<TaskItemStatus>();

    /// <summary>Бросает <see cref="InvalidStatusTransitionException"/>, если переход запрещён.</summary>
    public static void EnsureCanTransition(TaskItemStatus from, TaskItemStatus to)
    {
        if (CanTransition(from, to))
        {
            return;
        }

        throw new InvalidStatusTransitionException(from, to, GetAllowedTransitions(from));
    }
}
