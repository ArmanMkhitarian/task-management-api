using System.Net;
using System.Net.Http.Json;

namespace TaskManagement.Tests.Integration;

public class TasksCrudTests : IntegrationTestBase
{
    public TasksCrudTests(PostgresFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Create_ReturnsCreatedWithLocationAndBody()
    {
        var response = await PostTaskAsync(new
        {
            title = "Написать тесты",
            description = "юнит + интеграционные",
            priority = "High",
            assigneeEmail = "dev@example.com"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await ReadAsync<TaskResponse>(response);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal("Написать тесты", created.Title);
        Assert.Equal(TaskItemStatus.New, created.Status); // новая задача всегда New
        Assert.Equal(TaskPriority.High, created.Priority);
        Assert.Equal("dev@example.com", created.AssigneeEmail);
        Assert.NotEqual(default, created.CreatedAt);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }

    [Fact]
    public async Task Create_NormalizesAssigneeEmail_OnWrite()
    {
        // Email канонизируется при записи: trim + lower.
        var created = await CreateTaskAsync(assigneeEmail: "  Dev@Example.COM ");

        Assert.Equal("dev@example.com", created.AssigneeEmail);
    }

    [Fact]
    public async Task GetById_ExistingTask_ReturnsIt()
    {
        var created = await CreateTaskAsync(title: "Получить по id");

        var response = await Client.GetAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await ReadAsync<TaskResponse>(response);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Получить по id", fetched.Title);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var response = await Client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesEditableFields_AndBumpsUpdatedAt()
    {
        var created = await CreateTaskAsync(title: "Старое", priority: "Low");

        var response = await Client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = "Новое",
            description = "теперь с описанием",
            priority = "Critical",
            assigneeEmail = "lead@example.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await ReadAsync<TaskResponse>(response);
        Assert.Equal("Новое", updated!.Title);
        Assert.Equal("теперь с описанием", updated.Description);
        Assert.Equal(TaskPriority.Critical, updated.Priority);
        Assert.Equal("lead@example.com", updated.AssigneeEmail);
        Assert.Equal(TaskItemStatus.New, updated.Status); // PUT не трогает статус
        Assert.Equal(created.CreatedAt, updated.CreatedAt); // CreatedAt неизменен
        Assert.True(updated.UpdatedAt >= created.UpdatedAt);
    }

    [Fact]
    public async Task Update_UnknownId_Returns404()
    {
        var response = await Client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}", new
        {
            title = "Что-то",
            description = (string?)null,
            priority = "Low",
            assigneeEmail = (string?)null
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTask_Returns204_ThenGone()
    {
        var created = await CreateTaskAsync();

        var deleteResponse = await Client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await Client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode); // hard delete
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var response = await Client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
