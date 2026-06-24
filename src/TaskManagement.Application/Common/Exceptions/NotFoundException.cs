namespace TaskManagement.Application.Common.Exceptions;

/// <summary>Запрошенный ресурс не найден. Маппится в HTTP 404 (middleware на этапе 5).</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException ForTask(Guid id) =>
        new($"Задача с Id '{id}' не найдена.");
}
