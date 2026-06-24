using TaskManagement.Application.Tasks.Validation;

namespace TaskManagement.Tests.Unit;

public class ValidatorTests
{
    private static readonly CreateTaskRequestValidator Create = new();
    private static readonly UpdateTaskRequestValidator Update = new();
    private static readonly UpdateTaskStatusRequestValidator UpdateStatus = new();
    private static readonly TaskListFilterValidator Filter = new();

    private static bool HasError(FluentValidation.Results.ValidationResult result, string property) =>
        result.Errors.Any(e => e.PropertyName == property);

    // --- CreateTaskRequest ---

    [Fact]
    public void Create_ValidRequest_Passes()
    {
        var result = Create.Validate(new CreateTaskRequest("Починить баг", "описание", TaskPriority.High, "dev@example.com"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_NullAssigneeAndDescription_Passes()
    {
        // Исполнитель и описание опциональны: правило email применяется только к непустому значению.
        var result = Create.Validate(new CreateTaskRequest("Заголовок", null, TaskPriority.Low, null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_EmptyTitle_FailsOnTitle()
    {
        var result = Create.Validate(new CreateTaskRequest("", null, TaskPriority.Low, null));

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(CreateTaskRequest.Title)));
    }

    [Fact]
    public void Create_TitleTooLong_FailsOnTitle()
    {
        var result = Create.Validate(new CreateTaskRequest(new string('a', 201), null, TaskPriority.Low, null));

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(CreateTaskRequest.Title)));
    }

    [Fact]
    public void Create_DescriptionTooLong_FailsOnDescription()
    {
        var result = Create.Validate(new CreateTaskRequest("Заголовок", new string('a', 4001), TaskPriority.Low, null));

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(CreateTaskRequest.Description)));
    }

    [Fact]
    public void Create_InvalidEmail_FailsOnAssigneeEmail()
    {
        var result = Create.Validate(new CreateTaskRequest("Заголовок", null, TaskPriority.Low, "not-an-email"));

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(CreateTaskRequest.AssigneeEmail)));
    }

    [Fact]
    public void Create_PriorityOutOfEnumRange_FailsOnPriority()
    {
        var result = Create.Validate(new CreateTaskRequest("Заголовок", null, (TaskPriority)999, null));

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(CreateTaskRequest.Priority)));
    }

    // --- UpdateTaskRequest (та же матрица правил, что и Create) ---

    [Fact]
    public void Update_ValidRequest_Passes()
    {
        var result = Update.Validate(new UpdateTaskRequest("Новый заголовок", null, TaskPriority.Critical, "lead@example.com"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_EmptyTitle_FailsOnTitle()
    {
        var result = Update.Validate(new UpdateTaskRequest("", null, TaskPriority.Low, null));

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(UpdateTaskRequest.Title)));
    }

    // --- UpdateTaskStatusRequest ---

    [Fact]
    public void UpdateStatus_ValidStatus_Passes()
    {
        var result = UpdateStatus.Validate(new UpdateTaskStatusRequest(TaskItemStatus.InProgress));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateStatus_StatusOutOfEnumRange_FailsOnStatus()
    {
        var result = UpdateStatus.Validate(new UpdateTaskStatusRequest((TaskItemStatus)999));

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(UpdateTaskStatusRequest.Status)));
    }

    // --- TaskListFilter ---

    [Fact]
    public void Filter_Defaults_Pass()
    {
        // Дефолты Limit=20, Offset=0, без фильтров — валидны.
        var result = Filter.Validate(new TaskListFilter());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Filter_LimitOutOfRange_FailsOnLimit(int limit)
    {
        var result = Filter.Validate(new TaskListFilter { Limit = limit });

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(TaskListFilter.Limit)));
    }

    [Fact]
    public void Filter_NegativeOffset_FailsOnOffset()
    {
        var result = Filter.Validate(new TaskListFilter { Offset = -1 });

        Assert.False(result.IsValid);
        Assert.True(HasError(result, nameof(TaskListFilter.Offset)));
    }
}
