using TaskManagement.Application.Common;

namespace TaskManagement.Tests.Unit;

public class EmailNormalizerTests
{
    // Канонизация: обрезка пробелов + нижний регистр. Сравнения после записи остаются точными (==).
    [Theory]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData("  user@example.com  ", "user@example.com")]
    [InlineData("User@Example.COM", "user@example.com")]
    [InlineData("\tUSER@EXAMPLE.COM\n", "user@example.com")]
    public void Normalize_TrimsAndLowercases(string input, string expected)
    {
        Assert.Equal(expected, EmailNormalizer.Normalize(input));
    }

    // Пустое/пробельное/null → null: «нет исполнителя».
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Normalize_BlankOrNull_ReturnsNull(string? input)
    {
        Assert.Null(EmailNormalizer.Normalize(input));
    }
}
