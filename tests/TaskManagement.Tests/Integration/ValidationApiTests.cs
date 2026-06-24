using System.Net;
using System.Text.Json;

namespace TaskManagement.Tests.Integration;

public class ValidationApiTests : IntegrationTestBase
{
    public ValidationApiTests(PostgresFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Create_EmptyTitle_Returns400_ValidationProblem()
    {
        var response = await PostTaskAsync(new { title = "", description = (string?)null, priority = "Low", assigneeEmail = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadValidationErrorsAsync(response);
        Assert.Contains(nameof(CreateTaskRequest.Title), errors);
    }

    [Fact]
    public async Task Create_TitleTooLong_Returns400()
    {
        var response = await PostTaskAsync(new { title = new string('a', 201), description = (string?)null, priority = "Low", assigneeEmail = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(nameof(CreateTaskRequest.Title), await ReadValidationErrorsAsync(response));
    }

    [Fact]
    public async Task Create_InvalidEmail_Returns400()
    {
        var response = await PostTaskAsync(new { title = "ок", description = (string?)null, priority = "Low", assigneeEmail = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(nameof(CreateTaskRequest.AssigneeEmail), await ReadValidationErrorsAsync(response));
    }

    [Fact]
    public async Task GetList_LimitOutOfRange_Returns400()
    {
        var response = await Client.GetAsync("/api/tasks?limit=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(nameof(TaskListFilter.Limit), await ReadValidationErrorsAsync(response));
    }

    [Fact]
    public async Task GetList_NegativeOffset_Returns400()
    {
        var response = await Client.GetAsync("/api/tasks?offset=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(nameof(TaskListFilter.Offset), await ReadValidationErrorsAsync(response));
    }

    // ValidationProblemDetails: ключи объекта "errors" — имена свойств с ошибками.
    private static async Task<IReadOnlyCollection<string>> ReadValidationErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement
            .GetProperty("errors")
            .EnumerateObject()
            .Select(p => p.Name)
            .ToArray();
    }
}
