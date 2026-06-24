using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Tasks.Abstractions;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Api.Controllers;

/// <summary>Управление задачами.</summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>Создать задачу.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var created = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Список задач с фильтрами (status, priority, assigneeEmail) и пагинацией (limit, offset).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TaskResponse>>> GetList([FromQuery] TaskListFilter filter, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetListAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>Получить задачу по идентификатору.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>Полное обновление задачи (без смены статуса).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var updated = await _taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Сменить статус задачи. Недопустимый переход → 409.</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskResponse>> ChangeStatus(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await _taskService.ChangeStatusAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Удалить задачу.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
