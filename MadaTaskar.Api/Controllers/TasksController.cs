using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MadaTaskar.Data;
using MadaTaskar.Shared.DTOs;
using MadaTaskar.Shared.Models;

namespace MadaTaskar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly KanbanDbContext _context;

    public TasksController(KanbanDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks([FromQuery] string? assignee = null, [FromQuery] Priority? priority = null)
    {
        var query = _context.TaskItems.AsQueryable();

        if (!string.IsNullOrEmpty(assignee))
        {
            query = query.Where(t => t.Assignee != null && t.Assignee.Contains(assignee));
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        var tasks = await query
            .OrderBy(t => t.ColumnId)
            .ThenBy(t => t.Order)
            .Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Assignee = t.Assignee,
                Priority = t.Priority,
                Order = t.Order,
                ColumnId = t.ColumnId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetTask(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);

        if (task == null)
            return NotFound();

        var taskDto = new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Assignee = task.Assignee,
            Priority = task.Priority,
            Order = task.Order,
            ColumnId = task.ColumnId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };

        return Ok(taskDto);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask(CreateTaskItemDto createTaskDto)
    {
        var task = new TaskItem
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Assignee = createTaskDto.Assignee,
            Priority = createTaskDto.Priority,
            Order = createTaskDto.Order,
            ColumnId = createTaskDto.ColumnId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        var taskDto = new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Assignee = task.Assignee,
            Priority = task.Priority,
            Order = task.Order,
            ColumnId = task.ColumnId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskItemDto updateTaskDto)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null)
            return NotFound();

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.Assignee = updateTaskDto.Assignee;
        task.Priority = updateTaskDto.Priority;
        task.Order = updateTaskDto.Order;
        task.ColumnId = updateTaskDto.ColumnId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null)
            return NotFound();

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/move")]
    public async Task<IActionResult> MoveTask(int id, MoveTaskItemDto moveTaskDto)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null)
            return NotFound();

        task.ColumnId = moveTaskDto.TargetColumnId;
        task.Order = moveTaskDto.Order;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}