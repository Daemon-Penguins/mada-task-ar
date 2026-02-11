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
    public int? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }
    public Priority Priority { get; set; }
    public int Order { get; set; }
    public int ColumnId { get; set; }
    public Column Column { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Agent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Role { get; set; } = "worker";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

public class AgentActivity
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int? TaskId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}
