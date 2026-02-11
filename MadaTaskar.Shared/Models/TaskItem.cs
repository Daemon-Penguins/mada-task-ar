namespace MadaTaskar.Shared.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public int Order { get; set; }
    public int ColumnId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Column? Column { get; set; }
}