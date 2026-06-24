using System.Net.Http.Json;

namespace TaskManagement.Tests.Integration;

public class EmailAssignmentTests : IntegrationTestBase
{
    public EmailAssignmentTests(PostgresFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Create_WithAssignee_SendsOneNotification_ToNormalizedEmail()
    {
        var created = await CreateTaskAsync(title: "назначенная", assigneeEmail: "  Dev@Example.COM ");

        var sent = Fixture.EmailSender.Sent;
        Assert.Single(sent);
        Assert.Equal("dev@example.com", sent[0].AssigneeEmail); // нормализованный адрес
        Assert.Equal(created.Id, sent[0].TaskId);
    }

    [Fact]
    public async Task Create_WithoutAssignee_SendsNothing()
    {
        await CreateTaskAsync(title: "без исполнителя", assigneeEmail: null);

        Assert.Empty(Fixture.EmailSender.Sent);
    }

    [Fact]
    public async Task Update_AssignsPreviouslyUnassignedTask_SendsNotification()
    {
        var created = await CreateTaskAsync(title: "сначала ничья", assigneeEmail: null);
        Assert.Empty(Fixture.EmailSender.Sent);

        await Client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = created.Title,
            description = (string?)null,
            priority = "Medium",
            assigneeEmail = "new-owner@example.com"
        });

        var sent = Fixture.EmailSender.Sent;
        Assert.Single(sent);
        Assert.Equal("new-owner@example.com", sent[0].AssigneeEmail);
    }

    [Fact]
    public async Task Update_ChangesAssigneeToDifferentPerson_SendsNotification()
    {
        var created = await CreateTaskAsync(title: "переназначение", assigneeEmail: "first@example.com");
        Assert.Single(Fixture.EmailSender.Sent); // уведомление при создании

        await Client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = created.Title,
            description = (string?)null,
            priority = "Medium",
            assigneeEmail = "second@example.com"
        });

        var sent = Fixture.EmailSender.Sent;
        Assert.Equal(2, sent.Count);
        Assert.Equal("second@example.com", sent[^1].AssigneeEmail);
    }

    [Fact]
    public async Task Update_SameAssignee_DifferentCase_DoesNotResend()
    {
        var created = await CreateTaskAsync(title: "тот же исполнитель", assigneeEmail: "owner@example.com");
        Assert.Single(Fixture.EmailSender.Sent);

        // Тот же исполнитель в другом регистре → после нормализации равен прежнему → без повторной отправки.
        await Client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = created.Title,
            description = (string?)null,
            priority = "High",
            assigneeEmail = "OWNER@EXAMPLE.COM"
        });

        Assert.Single(Fixture.EmailSender.Sent); // по-прежнему одно
    }

    [Fact]
    public async Task Update_RemovesAssignee_DoesNotSend()
    {
        var created = await CreateTaskAsync(title: "снятие исполнителя", assigneeEmail: "owner@example.com");
        Assert.Single(Fixture.EmailSender.Sent);

        await Client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = created.Title,
            description = (string?)null,
            priority = "Low",
            assigneeEmail = (string?)null
        });

        Assert.Single(Fixture.EmailSender.Sent); // снятие — не назначение
    }
}
