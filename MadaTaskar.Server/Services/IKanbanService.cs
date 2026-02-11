using MadaTaskar.Shared.DTOs;
using MadaTaskar.Shared.Models;

namespace MadaTaskar.Server.Services;

public interface IKanbanService
{
    // Boards
    Task<IEnumerable<BoardDto>> GetBoardsAsync();
    Task<BoardDto?> GetBoardAsync(int id);
    Task<BoardDto?> CreateBoardAsync(CreateBoardDto createBoardDto);
    Task<bool> UpdateBoardAsync(int id, UpdateBoardDto updateBoardDto);
    Task<bool> DeleteBoardAsync(int id);

    // Columns
    Task<IEnumerable<ColumnDto>> GetColumnsAsync();
    Task<ColumnDto?> GetColumnAsync(int id);
    Task<ColumnDto?> CreateColumnAsync(CreateColumnDto createColumnDto);
    Task<bool> UpdateColumnAsync(int id, UpdateColumnDto updateColumnDto);
    Task<bool> DeleteColumnAsync(int id);
    Task<bool> ReorderColumnAsync(int id, int newOrder);

    // Tasks
    Task<IEnumerable<TaskItemDto>> GetTasksAsync(string? assignee = null, Priority? priority = null);
    Task<TaskItemDto?> GetTaskAsync(int id);
    Task<TaskItemDto?> CreateTaskAsync(CreateTaskItemDto createTaskDto);
    Task<bool> UpdateTaskAsync(int id, UpdateTaskItemDto updateTaskDto);
    Task<bool> DeleteTaskAsync(int id);
    Task<bool> MoveTaskAsync(int id, MoveTaskItemDto moveTaskDto);
}