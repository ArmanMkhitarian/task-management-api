namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>Страница результатов списка: элементы + общее число и параметры пагинации.</summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Limit,
    int Offset);
