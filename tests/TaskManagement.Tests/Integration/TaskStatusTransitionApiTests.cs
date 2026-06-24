using System.Net;
using System.Text.Json;

namespace TaskManagement.Tests.Integration;

public class TaskStatusTransitionApiTests : IntegrationTestBase
{
    public TaskStatusTransitionApiTests(PostgresFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ChangeStatus_AllowedTransition_Returns200AndPersists()
    {
        var task = await CreateTaskAsync(); // New

        var response = await ChangeStatusAsync(task.Id, "InProgress");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await ReadAsync<TaskResponse>(response);
        Assert.Equal(TaskItemStatus.InProgress, updated!.Status);

        // Перечитываем — переход сохранён.
        var refetched = await ReadAsync<TaskResponse>(await Client.GetAsync($"/api/tasks/{task.Id}"));
        Assert.Equal(TaskItemStatus.InProgress, refetched!.Status);
    }

    [Fact]
    public async Task ChangeStatus_WalkFullHappyPath_NewToDone()
    {
        var task = await CreateTaskAsync();

        foreach (var next in new[] { "InProgress", "Review", "Done" })
        {
            var response = await ChangeStatusAsync(task.Id, next);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var final = await ReadAsync<TaskResponse>(await Client.GetAsync($"/api/tasks/{task.Id}"));
        Assert.Equal(TaskItemStatus.Done, final!.Status);
    }

    [Fact]
    public async Task ChangeStatus_SameStatus_IsIdempotentNoOp_Returns200()
    {
        var task = await CreateTaskAsync(); // New

        var response = await ChangeStatusAsync(task.Id, "New");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadAsync<TaskResponse>(response);
        Assert.Equal(TaskItemStatus.New, result!.Status);
    }

    [Fact]
    public async Task ChangeStatus_ForbiddenTransition_Returns409_WithAllowedTransitions()
    {
        var task = await CreateTaskAsync(); // New

        // New → Done запрещён, минуя InProgress/Review.
        var response = await ChangeStatusAsync(task.Id, "Done");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var allowed = await ReadAllowedTransitionsAsync(response);
        Assert.Equal(new[] { "InProgress" }, allowed);
    }

    [Fact]
    public async Task ChangeStatus_FromTerminalDone_Returns409_WithEmptyAllowedTransitions()
    {
        var task = await CreateTaskAsync();
        foreach (var next in new[] { "InProgress", "Review", "Done" })
        {
            await ChangeStatusAsync(task.Id, next);
        }

        var response = await ChangeStatusAsync(task.Id, "InProgress");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var allowed = await ReadAllowedTransitionsAsync(response);
        Assert.Empty(allowed); // Done — терминальный
    }

    [Fact]
    public async Task ChangeStatus_UnknownId_Returns404()
    {
        var response = await ChangeStatusAsync(Guid.NewGuid(), "InProgress");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ProblemDetails читаем как сырой JSON: allowedTransitions лежит в extension-поле.
    private static async Task<string[]> ReadAllowedTransitionsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement
            .GetProperty("allowedTransitions")
            .EnumerateArray()
            .Select(e => e.GetString()!)
            .ToArray();
    }
}
