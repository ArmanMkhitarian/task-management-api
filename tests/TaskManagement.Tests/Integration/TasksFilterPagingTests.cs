using System.Net;

namespace TaskManagement.Tests.Integration;

public class TasksFilterPagingTests : IntegrationTestBase
{
    public TasksFilterPagingTests(PostgresFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetList_NoFilters_ReturnsAllWithPagingMeta()
    {
        await CreateTaskAsync(title: "A");
        await CreateTaskAsync(title: "B");
        await CreateTaskAsync(title: "C");

        var page = await GetListAsync("/api/tasks");

        Assert.Equal(3, page!.TotalCount);
        Assert.Equal(3, page.Items.Count);
        Assert.Equal(20, page.Limit); // дефолт
        Assert.Equal(0, page.Offset);
    }

    [Fact]
    public async Task GetList_FiltersByStatus()
    {
        var toProgress = await CreateTaskAsync(title: "В работе");
        await ChangeStatusAsync(toProgress.Id, "InProgress");
        await CreateTaskAsync(title: "Новая 1");
        await CreateTaskAsync(title: "Новая 2");

        var inProgress = await GetListAsync("/api/tasks?status=InProgress");
        Assert.Equal(1, inProgress!.TotalCount);
        Assert.Equal(toProgress.Id, inProgress.Items.Single().Id);

        var news = await GetListAsync("/api/tasks?status=New");
        Assert.Equal(2, news!.TotalCount);
    }

    [Fact]
    public async Task GetList_FiltersByPriority()
    {
        await CreateTaskAsync(title: "крит", priority: "Critical");
        await CreateTaskAsync(title: "низ 1", priority: "Low");
        await CreateTaskAsync(title: "низ 2", priority: "Low");

        var critical = await GetListAsync("/api/tasks?priority=Critical");
        Assert.Equal(1, critical!.TotalCount);

        var low = await GetListAsync("/api/tasks?priority=Low");
        Assert.Equal(2, low!.TotalCount);
    }

    [Fact]
    public async Task GetList_FiltersByAssigneeEmail_CaseInsensitive()
    {
        await CreateTaskAsync(title: "назначена", assigneeEmail: "Owner@Example.com");
        await CreateTaskAsync(title: "чужая", assigneeEmail: "other@example.com");
        await CreateTaskAsync(title: "без исполнителя");

        // Фильтр нормализуется так же, как хранимое значение → разный регистр находит задачу.
        var page = await GetListAsync("/api/tasks?assigneeEmail=OWNER@EXAMPLE.COM");

        Assert.Equal(1, page!.TotalCount);
        Assert.Equal("owner@example.com", page.Items.Single().AssigneeEmail);
    }

    [Fact]
    public async Task GetList_CombinesFilters()
    {
        var target = await CreateTaskAsync(title: "цель", priority: "High", assigneeEmail: "a@example.com");
        await CreateTaskAsync(title: "другой приоритет", priority: "Low", assigneeEmail: "a@example.com");
        await CreateTaskAsync(title: "другой исполнитель", priority: "High", assigneeEmail: "b@example.com");

        var page = await GetListAsync("/api/tasks?priority=High&assigneeEmail=a@example.com");

        Assert.Equal(1, page!.TotalCount);
        Assert.Equal(target.Id, page.Items.Single().Id);
    }

    [Fact]
    public async Task GetList_Pagination_LimitAndOffset_TotalCountIsFullSet()
    {
        for (var i = 0; i < 5; i++)
        {
            await CreateTaskAsync(title: $"T{i}");
        }

        var firstPage = await GetListAsync("/api/tasks?limit=2&offset=0");
        Assert.Equal(5, firstPage!.TotalCount); // всего, не размер страницы
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(2, firstPage.Limit);
        Assert.Equal(0, firstPage.Offset);

        var lastPage = await GetListAsync("/api/tasks?limit=2&offset=4");
        Assert.Equal(5, lastPage!.TotalCount);
        Assert.Single(lastPage.Items); // остаётся один элемент
    }

    [Fact]
    public async Task GetList_OrdersByCreatedAtDescending()
    {
        var first = await CreateTaskAsync(title: "первая");
        var second = await CreateTaskAsync(title: "вторая");
        var third = await CreateTaskAsync(title: "третья");

        var page = await GetListAsync("/api/tasks");

        // Свежесозданные — выше: сортировка created_at DESC (тай-брейк по убыванию Id).
        Assert.Equal(
            new[] { third.Id, second.Id, first.Id },
            page!.Items.Select(i => i.Id).ToArray());
    }

    private async Task<PagedResult<TaskResponse>?> GetListAsync(string url)
    {
        var response = await Client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadAsync<PagedResult<TaskResponse>>(response);
    }
}
