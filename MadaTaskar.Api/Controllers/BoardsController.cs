using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MadaTaskar.Data;
using MadaTaskar.Shared.DTOs;
using MadaTaskar.Shared.Models;

namespace MadaTaskar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly KanbanDbContext _context;

    public BoardsController(KanbanDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards()
    {
        var boards = await _context.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
            .OrderBy(b => b.CreatedAt)
            .Select(b => new BoardDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                Columns = b.Columns.OrderBy(c => c.Order).Select(c => new ColumnDto
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
                }).ToList()
            })
            .ToListAsync();

        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BoardDto>> GetBoard(int id)
    {
        var board = await _context.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board == null)
            return NotFound();

        var boardDto = new BoardDto
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            CreatedAt = board.CreatedAt,
            UpdatedAt = board.UpdatedAt,
            Columns = board.Columns.OrderBy(c => c.Order).Select(c => new ColumnDto
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
            }).ToList()
        };

        return Ok(boardDto);
    }

    [HttpPost]
    public async Task<ActionResult<BoardDto>> CreateBoard(CreateBoardDto createBoardDto)
    {
        var board = new Board
        {
            Name = createBoardDto.Name,
            Description = createBoardDto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var boardDto = new BoardDto
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            CreatedAt = board.CreatedAt,
            UpdatedAt = board.UpdatedAt,
            Columns = new List<ColumnDto>()
        };

        return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, boardDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(int id, UpdateBoardDto updateBoardDto)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board == null)
            return NotFound();

        board.Name = updateBoardDto.Name;
        board.Description = updateBoardDto.Description;
        board.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board == null)
            return NotFound();

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}