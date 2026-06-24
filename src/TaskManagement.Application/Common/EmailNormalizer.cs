namespace TaskManagement.Application.Common;

/// <summary>
/// Канонизация email для хранения и сравнения: trim + нижний регистр; пустое/пробелы → null.
/// Email регистронезависимы, поэтому нормализуем при записи — тогда сравнения остаются точными (==).
/// </summary>
public static class EmailNormalizer
{
    public static string? Normalize(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}
