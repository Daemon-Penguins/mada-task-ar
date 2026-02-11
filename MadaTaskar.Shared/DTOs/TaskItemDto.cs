using MadaTaskar.Shared.Models;

namespace MadaTaskar.Shared.DTOs;

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public int Order { get; set; }
    public int ColumnId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public int Order { get; set; }
    public int ColumnId { get; set; }
}

public class UpdateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public int Order { get; set; }
    public int ColumnId { get; set; }
}

public class MoveTaskItemDto
{
    public int TaskId { get; set; }
    public int TargetColumnId { get; set; }
    public int Order { get; set; }
}