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

    // Pipeline v2
    public TaskPhase Phase { get; set; } = TaskPhase.Research;
    public int? AuthorAgentId { get; set; }
    public bool ReadyToWorkChecked { get; set; }
    public List<TaskComment> Comments { get; set; } = new();
    public List<TaskReference> References { get; set; } = new();
    public List<TaskPhaseLog> PhaseLogs { get; set; } = new();
    public List<TaskApproval> Approvals { get; set; } = new();
}

public class Agent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Roles { get; set; } = "worker";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public bool HasRole(string role) => Roles.Split(',').Contains(role, StringComparer.OrdinalIgnoreCase);
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

public class TaskComment
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public CommentType Type { get; set; } = CommentType.General;
    public DateTime CreatedAt { get; set; }
}

public class TaskReference
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TaskPhaseLog
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public TaskPhase? FromPhase { get; set; }
    public TaskPhase ToPhase { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TaskApproval
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public ApprovalDecision Decision { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum Priority { Low, Medium, High, Critical }

public enum TaskPhase { Research, Brainstorm, Triage, AuthorReview, ReadyToWork, InProgress, Acceptance, Completed, Killed }

public enum CommentType { Research, Proposal, Remark, Progress, General }

public enum ApprovalDecision { Approve, Reject, RequestChanges }
