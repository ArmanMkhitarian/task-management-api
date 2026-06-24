using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskManagement.Tests.Integration;

/// <summary>
/// База для интеграционных тестов: общий контейнер/клиент из фикстуры,
/// сброс состояния перед каждым тестом и хелперы поверх HTTP API.
/// </summary>
[Collection(IntegrationCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    // Web-дефолты (camelCase, регистронезависимость) + enum строками — как сериализует API.
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected PostgresFixture Fixture { get; }

    protected HttpClient Client => Fixture.Client;

    protected IntegrationTestBase(PostgresFixture fixture) => Fixture = fixture;

    public Task InitializeAsync() => Fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    protected static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(body, Json);
    }

    protected Task<HttpResponseMessage> PostTaskAsync(object body) =>
        Client.PostAsJsonAsync("/api/tasks", body);

    /// <summary>Создаёт задачу и возвращает её представление. Падает, если ответ не 201.</summary>
    protected async Task<TaskResponse> CreateTaskAsync(
        string title = "Задача",
        string priority = "Medium",
        string? assigneeEmail = null,
        string? description = null)
    {
        var response = await PostTaskAsync(new { title, description, priority, assigneeEmail });
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<TaskResponse>(response))!;
    }

    /// <summary>PATCH /status с телом-строкой статуса.</summary>
    protected Task<HttpResponseMessage> ChangeStatusAsync(Guid id, string status) =>
        Client.PatchAsJsonAsync($"/api/tasks/{id}/status", new { status });
}
