using Microsoft.EntityFrameworkCore;
using MadaTaskar.Data;

namespace MadaTaskar.Services;

public class BoardService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public BoardService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Board?> GetBoardAsync(int boardId = 1)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Boards
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .ThenInclude(c => c.Tasks.OrderBy(t => t.Order))
            .FirstOrDefaultAsync(b => b.Id == boardId);
    }

    public async Task<List<Column>> GetColumnsAsync(int boardId = 1)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Columns
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Order)
            .Include(c => c.Tasks.OrderBy(t => t.Order))
            .ToListAsync();
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        using var db = await _factory.CreateDbContextAsync();
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Tasks.FindAsync(task.Id);
        if (existing is null) return;

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Assignee = task.Assignee;
        existing.Priority = task.Priority;
        existing.ColumnId = task.ColumnId;
        existing.Order = task.Order;
        existing.Phase = task.Phase;
        existing.AuthorAgentId = task.AuthorAgentId;
        existing.ReadyToWorkChecked = task.ReadyToWorkChecked;
        existing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task MoveTaskAsync(int taskId, int newColumnId, int newOrder)
    {
        using var db = await _factory.CreateDbContextAsync();
        var task = await db.Tasks.FindAsync(taskId);
        if (task is null) return;

        task.ColumnId = newColumnId;
        task.Order = newOrder;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int taskId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var task = await db.Tasks.FindAsync(taskId);
        if (task is null) return;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
    }

    public async Task<TaskItem?> GetTaskWithDetailsAsync(int taskId)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Tasks
            .Include(t => t.Comments).ThenInclude(c => c.Agent)
            .Include(t => t.References).ThenInclude(r => r.Agent)
            .Include(t => t.PhaseLogs).ThenInclude(l => l.Agent)
            .Include(t => t.Approvals).ThenInclude(a => a.Agent)
            .Include(t => t.AcceptanceCriteria).ThenInclude(a => a.CheckedByAgent)
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task AddCommentAsync(TaskComment comment)
    {
        using var db = await _factory.CreateDbContextAsync();
        comment.CreatedAt = DateTime.UtcNow;
        db.TaskComments.Add(comment);
        await db.SaveChangesAsync();
    }

    public async Task AddReferenceAsync(TaskReference reference)
    {
        using var db = await _factory.CreateDbContextAsync();
        reference.CreatedAt = DateTime.UtcNow;
        db.TaskReferences.Add(reference);
        await db.SaveChangesAsync();
    }

    public async Task AddPhaseLogAsync(TaskPhaseLog log)
    {
        using var db = await _factory.CreateDbContextAsync();
        log.CreatedAt = DateTime.UtcNow;
        db.TaskPhaseLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task AddApprovalAsync(TaskApproval approval)
    {
        using var db = await _factory.CreateDbContextAsync();
        approval.CreatedAt = DateTime.UtcNow;
        db.TaskApprovals.Add(approval);
        await db.SaveChangesAsync();
    }

    public async Task AddAcceptanceCriterionAsync(AcceptanceCriterion criterion)
    {
        using var db = await _factory.CreateDbContextAsync();
        criterion.CreatedAt = DateTime.UtcNow;
        db.AcceptanceCriteria.Add(criterion);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAcceptanceCriterionAsync(int criterionId, bool isMet, int? agentId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var criterion = await db.AcceptanceCriteria.FindAsync(criterionId);
        if (criterion is null) return;
        criterion.IsMet = isMet;
        criterion.CheckedByAgentId = isMet ? agentId : null;
        criterion.CheckedAt = isMet ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAcceptanceCriterionAsync(int criterionId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var criterion = await db.AcceptanceCriteria.FindAsync(criterionId);
        if (criterion is null) return;
        db.AcceptanceCriteria.Remove(criterion);
        await db.SaveChangesAsync();
    }

    public async Task<List<AcceptanceCriterion>> GetAcceptanceCriteriaAsync(int taskId)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.AcceptanceCriteria
            .Where(ac => ac.TaskId == taskId)
            .Include(ac => ac.CheckedByAgent)
            .OrderBy(ac => ac.Order)
            .ToListAsync();
    }
}
