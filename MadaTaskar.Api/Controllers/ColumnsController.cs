using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MadaTaskar.Data;
using MadaTaskar.Shared.DTOs;
using MadaTaskar.Shared.Models;

namespace MadaTaskar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ColumnsController : ControllerBase
{
    private readonly KanbanDbContext _context;

    public ColumnsController(KanbanDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ColumnDto>>> GetColumns()
    {
        var columns = await _context.Columns
            .Include(c => c.Tasks)
            .OrderBy(c => c.Order)
            .Select(c => new ColumnDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Order = c.Order,
                BoardId = c.BoardId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Tasks = c.Tasks.OrderBy(t => t.Order).Select(t => new TaskItemDto
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
                }).ToList()
            })
            .ToListAsync();

        return Ok(columns);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ColumnDto>> GetColumn(int id)
    {
        var column = await _context.Columns
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (column == null)
            return NotFound();

        var columnDto = new ColumnDto
        {
            Id = column.Id,
            Name = column.Name,
            Description = column.Description,
            Order = column.Order,
            BoardId = column.BoardId,
            CreatedAt = column.CreatedAt,
            UpdatedAt = column.UpdatedAt,
            Tasks = column.Tasks.OrderBy(t => t.Order).Select(t => new TaskItemDto
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
            }).ToList()
        };

        return Ok(columnDto);
    }

    [HttpPost]
    public async Task<ActionResult<ColumnDto>> CreateColumn(CreateColumnDto createColumnDto)
    {
        var column = new Column
        {
            Name = createColumnDto.Name,
            Description = createColumnDto.Description,
            Order = createColumnDto.Order,
            BoardId = createColumnDto.BoardId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        var columnDto = new ColumnDto
        {
            Id = column.Id,
            Name = column.Name,
            Description = column.Description,
            Order = column.Order,
            BoardId = column.BoardId,
            CreatedAt = column.CreatedAt,
            UpdatedAt = column.UpdatedAt,
            Tasks = new List<TaskItemDto>()
        };

        return CreatedAtAction(nameof(GetColumn), new { id = column.Id }, columnDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateColumn(int id, UpdateColumnDto updateColumnDto)
    {
        var column = await _context.Columns.FindAsync(id);
        if (column == null)
            return NotFound();

        column.Name = updateColumnDto.Name;
        column.Description = updateColumnDto.Description;
        column.Order = updateColumnDto.Order;
        column.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteColumn(int id)
    {
        var column = await _context.Columns.FindAsync(id);
        if (column == null)
            return NotFound();

        _context.Columns.Remove(column);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/reorder")]
    public async Task<IActionResult> ReorderColumn(int id, [FromBody] int newOrder)
    {
        var column = await _context.Columns.FindAsync(id);
        if (column == null)
            return NotFound();

        column.Order = newOrder;
        column.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}