using TaskManagement.Application.Common.Exceptions;

namespace TaskManagement.Tests.Unit;

public class TaskStatusPolicyTests
{
    // Допустимые переходы по матрице: New→InProgress, InProgress→{Review,New}, Review→{Done,InProgress}.
    [Theory]
    [InlineData(TaskItemStatus.New, TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.Review)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.New)]
    [InlineData(TaskItemStatus.Review, TaskItemStatus.Done)]
    [InlineData(TaskItemStatus.Review, TaskItemStatus.InProgress)]
    public void CanTransition_AllowedTransitions_ReturnsTrue(TaskItemStatus from, TaskItemStatus to)
    {
        Assert.True(TaskStatusPolicy.CanTransition(from, to));
    }

    // Перескок через этап и любой переход из терминального Done запрещены.
    [Theory]
    [InlineData(TaskItemStatus.New, TaskItemStatus.Review)]
    [InlineData(TaskItemStatus.New, TaskItemStatus.Done)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.Done)]
    [InlineData(TaskItemStatus.Review, TaskItemStatus.New)]
    [InlineData(TaskItemStatus.Done, TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.Done, TaskItemStatus.Review)]
    [InlineData(TaskItemStatus.Done, TaskItemStatus.New)]
    public void CanTransition_ForbiddenTransitions_ReturnsFalse(TaskItemStatus from, TaskItemStatus to)
    {
        Assert.False(TaskStatusPolicy.CanTransition(from, to));
    }

    // Переход в тот же статус — допустимый идемпотентный no-op для любого статуса, включая Done.
    [Theory]
    [InlineData(TaskItemStatus.New)]
    [InlineData(TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.Review)]
    [InlineData(TaskItemStatus.Done)]
    public void CanTransition_SameStatus_ReturnsTrue(TaskItemStatus status)
    {
        Assert.True(TaskStatusPolicy.CanTransition(status, status));
    }

    [Fact]
    public void GetAllowedTransitions_ReturnsExactSetPerStatus()
    {
        Assert.Equal(
            new[] { TaskItemStatus.InProgress },
            TaskStatusPolicy.GetAllowedTransitions(TaskItemStatus.New));

        Assert.Equal(
            new[] { TaskItemStatus.New, TaskItemStatus.Review },
            TaskStatusPolicy.GetAllowedTransitions(TaskItemStatus.InProgress).OrderBy(s => s));

        Assert.Equal(
            new[] { TaskItemStatus.InProgress, TaskItemStatus.Done },
            TaskStatusPolicy.GetAllowedTransitions(TaskItemStatus.Review).OrderBy(s => s));
    }

    [Fact]
    public void GetAllowedTransitions_Done_IsTerminal_ReturnsEmpty()
    {
        Assert.Empty(TaskStatusPolicy.GetAllowedTransitions(TaskItemStatus.Done));
    }

    [Fact]
    public void EnsureCanTransition_ValidTransition_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            TaskStatusPolicy.EnsureCanTransition(TaskItemStatus.New, TaskItemStatus.InProgress));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanTransition_ForbiddenTransition_ThrowsWithAllowedSet()
    {
        var exception = Assert.Throws<InvalidStatusTransitionException>(() =>
            TaskStatusPolicy.EnsureCanTransition(TaskItemStatus.New, TaskItemStatus.Done));

        Assert.Equal(TaskItemStatus.New, exception.From);
        Assert.Equal(TaskItemStatus.Done, exception.To);
        Assert.Equal(new[] { TaskItemStatus.InProgress }, exception.AllowedTransitions);
    }

    [Fact]
    public void EnsureCanTransition_FromTerminalDone_ThrowsWithEmptyAllowedSet()
    {
        var exception = Assert.Throws<InvalidStatusTransitionException>(() =>
            TaskStatusPolicy.EnsureCanTransition(TaskItemStatus.Done, TaskItemStatus.InProgress));

        Assert.Empty(exception.AllowedTransitions);
        Assert.Contains("терминальный", exception.Message);
    }
}
