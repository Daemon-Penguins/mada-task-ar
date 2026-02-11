namespace MadaTaskar.Data;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Column> Columns { get; set; } = new();
}

public class Column
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
    public List<TaskItem> Tasks { get; set; } = new();
}

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public Priority Priority { get; set; }
    public int Order { get; set; }
    public int ColumnId { get; set; }
    public Column Column { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}
