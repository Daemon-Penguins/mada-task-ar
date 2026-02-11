using MadaTaskar.Data;
using MadaTaskar.Services;

namespace MadaTaskar.Api;

public static class AgentApiEndpoints
{
    public static void MapAgentApi(this WebApplication app)
    {
        var api = app.MapGroup("/api").AddEndpointFilter<AgentAuthFilter>();

        // --- Agents ---
        api.MapGet("/agents", async (HttpContext ctx, AgentService agentService, PermissionService perms) =>
        {
            var agent = GetAgent(ctx);
            if (!perms.CanManageAgents(agent)) return Results.Forbid();
            return Results.Ok(await agentService.GetAgentsAsync());
        });

        api.MapPost("/agents/register", async (HttpContext ctx, AgentService agentService, PermissionService perms, RegisterAgentRequest req) =>
        {
            var agent = GetAgent(ctx);
            if (!perms.CanManageAgents(agent)) return Results.Forbid();
            var newAgent = await agentService.RegisterAgentAsync(req.Name, req.Roles ?? "worker");
            await agentService.LogActivityAsync(agent.Id, "register_agent", $"Registered '{newAgent.Name}' ({newAgent.Roles})");
            return Results.Ok(newAgent);
        });

        api.MapDelete("/agents/{id:int}", async (HttpContext ctx, AgentService agentService, PermissionService perms, int id) =>
        {
            var agent = GetAgent(ctx);
            if (!perms.CanManageAgents(agent)) return Results.Forbid();
            var ok = await agentService.DeactivateAgentAsync(id);
            if (!ok) return Results.NotFound();
            await agentService.LogActivityAsync(agent.Id, "deactivate_agent", $"Deactivated agent #{id}");
            return Results.Ok();
        });

        // --- Board ---
        api.MapGet("/board", async (HttpContext ctx, BoardService boardService) =>
        {
            var board = await boardService.GetBoardAsync();
            return board is null ? Results.NotFound() : Results.Ok(board);
        });

        api.MapGet("/board/columns", async (HttpContext ctx, BoardService boardService) =>
        {
            return Results.Ok(await boardService.GetColumnsAsync());
        });

        // --- Tasks ---
        api.MapPost("/tasks", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, CreateTaskRequest req) =>
        {
            var agent = GetAgent(ctx);
            if (!perms.CanCreateTask(agent)) return Results.Forbid();
            var task = new TaskItem
            {
                Title = req.Title,
                Description = req.Description,
                Assignee = req.Assignee ?? agent.Name,
                AssignedAgentId = req.AssignToSelf ? agent.Id : null,
                Priority = req.Priority ?? Priority.Medium,
                ColumnId = req.ColumnId ?? 1,
                Order = req.Order ?? 0,
                Phase = TaskPhase.Research,
                AuthorAgentId = agent.Id
            };
            var created = await boardService.CreateTaskAsync(task);
            await agentService.LogActivityAsync(agent.Id, "create_task", $"Created '{task.Title}'", created.Id);
            return Results.Created($"/api/tasks/{created.Id}", created);
        });

        api.MapPut("/tasks/{id:int}", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id, UpdateTaskRequest req) =>
        {
            var agent = GetAgent(ctx);
            var board = await boardService.GetBoardAsync();
            var existing = board?.Columns.SelectMany(c => c.Tasks).FirstOrDefault(t => t.Id == id);
            if (existing is null) return Results.NotFound();

            if (req.Title is not null) existing.Title = req.Title;
            if (req.Description is not null) existing.Description = req.Description;
            if (req.Assignee is not null) existing.Assignee = req.Assignee;
            if (req.Priority.HasValue) existing.Priority = req.Priority.Value;
            if (req.ColumnId.HasValue) existing.ColumnId = req.ColumnId.Value;
            if (req.Order.HasValue) existing.Order = req.Order.Value;

            await boardService.UpdateTaskAsync(existing);
            await agentService.LogActivityAsync(agent.Id, "update_task", $"Updated '{existing.Title}'", id);
            return Results.Ok(existing);
        });

        api.MapPost("/tasks/{id:int}/move", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id, MoveTaskRequest req) =>
        {
            var agent = GetAgent(ctx);
            await boardService.MoveTaskAsync(id, req.ColumnId, req.Order ?? 0);
            await agentService.LogActivityAsync(agent.Id, "move_task", $"Moved task #{id} to column {req.ColumnId}", id);
            return Results.Ok();
        });

        api.MapPost("/tasks/{id:int}/assign", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id, AssignTaskRequest? req) =>
        {
            var agent = GetAgent(ctx);
            var board = await boardService.GetBoardAsync();
            var task = board?.Columns.SelectMany(c => c.Tasks).FirstOrDefault(t => t.Id == id);
            if (task is null) return Results.NotFound();

            task.AssignedAgentId = req?.AgentId ?? agent.Id;
            task.Assignee = agent.Name;
            await boardService.UpdateTaskAsync(task);
            await agentService.LogActivityAsync(agent.Id, "assign_task", $"Assigned task #{id} to agent #{task.AssignedAgentId}", id);
            return Results.Ok(task);
        });

        api.MapDelete("/tasks/{id:int}", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, int id) =>
        {
            var agent = GetAgent(ctx);
            if (!perms.CanDeleteTask(agent)) return Results.Forbid();
            await boardService.DeleteTaskAsync(id);
            await agentService.LogActivityAsync(agent.Id, "delete_task", $"Deleted task #{id}", id);
            return Results.Ok();
        });

        // --- Pipeline v2 endpoints ---

        api.MapPost("/tasks/{id:int}/research", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, int id, ResearchRequest req) =>
        {
            var agent = GetAgent(ctx);
            if (!perms.CanResearch(agent)) return Results.Forbid();
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var reference = new TaskReference { TaskId = id, AgentId = agent.Id, Url = req.Url, Title = req.Title, Summary = req.Summary };
            await boardService.AddReferenceAsync(reference);
            await agentService.LogActivityAsync(agent.Id, "add_research", $"Added research ref '{req.Title}' to task #{id}", id);
            return Results.Ok(new { reference.Id, reference.Url, reference.Title, reference.Summary, reference.CreatedAt });
        });

        api.MapPost("/tasks/{id:int}/propose", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, int id, ProposeRequest req) =>
        {
            var agent = GetAgent(ctx);
            if (!perms.CanPropose(agent)) return Results.Forbid();
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var comment = new TaskComment { TaskId = id, AgentId = agent.Id, Content = req.Content, Type = CommentType.Proposal };
            await boardService.AddCommentAsync(comment);
            await agentService.LogActivityAsync(agent.Id, "add_proposal", $"Added proposal to task #{id}", id);
            return Results.Ok(new { comment.Id, comment.Content, comment.Type, comment.CreatedAt });
        });

        api.MapPost("/tasks/{id:int}/comment", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id, CommentRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var comment = new TaskComment { TaskId = id, AgentId = agent.Id, Content = req.Content, Type = req.Type ?? CommentType.General };
            await boardService.AddCommentAsync(comment);
            await agentService.LogActivityAsync(agent.Id, "add_comment", $"Commented on task #{id}", id);
            return Results.Ok(new { comment.Id, comment.Content, comment.Type, comment.CreatedAt });
        });

        api.MapPost("/tasks/{id:int}/advance", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, int id, AdvanceRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            if (!perms.CanAdvancePhase(agent, task, req.TargetPhase))
                return Results.Json(new { error = "Forbidden", message = $"Cannot advance from {task.Phase} to {req.TargetPhase}" }, statusCode: 403);

            var fromPhase = task.Phase;
            var log = new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = req.TargetPhase, Reason = req.Reason };
            await boardService.AddPhaseLogAsync(log);

            // Update task phase and column
            task.Phase = req.TargetPhase;
            if (PermissionService.PhaseColumnMap.TryGetValue(req.TargetPhase, out var col))
                task.ColumnId = col;
            await boardService.UpdateTaskAsync(task);

            await agentService.LogActivityAsync(agent.Id, "advance_phase", $"Advanced task #{id} from {fromPhase} to {req.TargetPhase}", id);
            return Results.Ok(new { task.Id, task.Phase, task.ColumnId, from = fromPhase, to = req.TargetPhase });
        });

        api.MapPost("/tasks/{id:int}/approve", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, int id, ApprovalRequest? req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            // Determine target phase based on current
            TaskPhase targetPhase = task.Phase switch
            {
                TaskPhase.AuthorReview => TaskPhase.ReadyToWork,
                TaskPhase.Acceptance => TaskPhase.Completed,
                _ => TaskPhase.Completed
            };

            if (!perms.CanAdvancePhase(agent, task, targetPhase))
                return Results.Json(new { error = "Forbidden" }, statusCode: 403);

            var approval = new TaskApproval { TaskId = id, AgentId = agent.Id, Decision = ApprovalDecision.Approve, Comment = req?.Comment };
            await boardService.AddApprovalAsync(approval);

            var fromPhase = task.Phase;
            await boardService.AddPhaseLogAsync(new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = targetPhase, Reason = "Approved" });

            task.Phase = targetPhase;
            if (PermissionService.PhaseColumnMap.TryGetValue(targetPhase, out var col))
                task.ColumnId = col;
            await boardService.UpdateTaskAsync(task);

            await agentService.LogActivityAsync(agent.Id, "approve_task", $"Approved task #{id}: {fromPhase} → {targetPhase}", id);
            return Results.Ok(new { task.Id, task.Phase, task.ColumnId, from = fromPhase, to = targetPhase });
        });

        api.MapPost("/tasks/{id:int}/reject", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, int id, ApprovalRequest? req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            if (!perms.CanAdvancePhase(agent, task, TaskPhase.Killed))
                return Results.Json(new { error = "Forbidden" }, statusCode: 403);

            var approval = new TaskApproval { TaskId = id, AgentId = agent.Id, Decision = ApprovalDecision.Reject, Comment = req?.Comment };
            await boardService.AddApprovalAsync(approval);

            var fromPhase = task.Phase;
            await boardService.AddPhaseLogAsync(new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = TaskPhase.Killed, Reason = req?.Comment ?? "Rejected" });

            task.Phase = TaskPhase.Killed;
            task.ColumnId = 6;
            await boardService.UpdateTaskAsync(task);

            await agentService.LogActivityAsync(agent.Id, "reject_task", $"Rejected task #{id}", id);
            return Results.Ok(new { task.Id, task.Phase, task.ColumnId });
        });

        api.MapPost("/tasks/{id:int}/request-changes", async (HttpContext ctx, BoardService boardService, AgentService agentService, PermissionService perms, int id, RequestChangesRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            if (task.Phase != TaskPhase.Acceptance || !perms.CanAccept(agent, task))
                return Results.Json(new { error = "Forbidden", message = "Can only request changes from Acceptance phase" }, statusCode: 403);

            var approval = new TaskApproval { TaskId = id, AgentId = agent.Id, Decision = ApprovalDecision.RequestChanges, Comment = req.Comment };
            await boardService.AddApprovalAsync(approval);

            var fromPhase = task.Phase;
            await boardService.AddPhaseLogAsync(new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = TaskPhase.InProgress, Reason = req.Comment });

            task.Phase = TaskPhase.InProgress;
            task.ColumnId = 3;
            await boardService.UpdateTaskAsync(task);

            await agentService.LogActivityAsync(agent.Id, "request_changes", $"Requested changes on task #{id}", id);
            return Results.Ok(new { task.Id, task.Phase, task.ColumnId });
        });

        api.MapGet("/tasks/{id:int}/timeline", async (HttpContext ctx, BoardService boardService, int id) =>
        {
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var timeline = new List<object>();
            foreach (var l in task.PhaseLogs) timeline.Add(new { type = "phase", l.Id, l.FromPhase, l.ToPhase, l.Reason, l.CreatedAt, agent = l.Agent.Name });
            foreach (var c in task.Comments) timeline.Add(new { type = "comment", c.Id, c.Content, commentType = c.Type, c.CreatedAt, agent = c.Agent.Name });
            foreach (var a in task.Approvals) timeline.Add(new { type = "approval", a.Id, a.Decision, a.Comment, a.CreatedAt, agent = a.Agent.Name });

            return Results.Ok(timeline.OrderBy(x => ((dynamic)x).CreatedAt));
        });

        api.MapGet("/tasks/{id:int}/references", async (HttpContext ctx, BoardService boardService, int id) =>
        {
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();
            return Results.Ok(task.References.Select(r => new { r.Id, r.Url, r.Title, r.Summary, r.CreatedAt, agent = r.Agent.Name }));
        });

        api.MapGet("/tasks/{id:int}/proposals", async (HttpContext ctx, BoardService boardService, int id) =>
        {
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();
            return Results.Ok(task.Comments.Where(c => c.Type == CommentType.Proposal).Select(c => new { c.Id, c.Content, c.CreatedAt, agent = c.Agent.Name }));
        });

        // --- Activity ---
        api.MapGet("/activity", async (HttpContext ctx, AgentService agentService, int? limit) =>
        {
            var activities = await agentService.GetActivityLogAsync(limit ?? 50);
            return Results.Ok(activities.Select(a => new
            {
                a.Id,
                a.Action,
                a.Details,
                a.TaskId,
                a.CreatedAt,
                AgentName = a.Agent.Name,
                a.AgentId
            }));
        });

        // --- Who am I ---
        api.MapGet("/me", (HttpContext ctx) =>
        {
            var agent = GetAgent(ctx);
            return Results.Ok(new { agent.Id, agent.Name, agent.Roles, agent.IsActive, agent.CreatedAt, agent.LastSeenAt });
        });
    }

    private static Agent GetAgent(HttpContext ctx) => (Agent)ctx.Items["Agent"]!;
}

// --- Auth Filter ---
public class AgentAuthFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var httpCtx = ctx.HttpContext;
        var apiKey = httpCtx.Request.Headers["X-Agent-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
            return Results.Unauthorized();

        var agentService = httpCtx.RequestServices.GetRequiredService<AgentService>();
        var agent = await agentService.AuthenticateAsync(apiKey);
        if (agent is null)
            return Results.Unauthorized();

        httpCtx.Items["Agent"] = agent;
        return await next(ctx);
    }
}

// --- DTOs ---
public record RegisterAgentRequest(string Name, string? Roles);
public record CreateTaskRequest(string Title, string? Description, string? Assignee, Priority? Priority, int? ColumnId, int? Order, bool AssignToSelf = false);
public record UpdateTaskRequest(string? Title, string? Description, string? Assignee, Priority? Priority, int? ColumnId, int? Order);
public record MoveTaskRequest(int ColumnId, int? Order);
public record AssignTaskRequest(int? AgentId);
public record ResearchRequest(string Url, string Title, string? Summary);
public record ProposeRequest(string Content);
public record CommentRequest(string Content, CommentType? Type);
public record AdvanceRequest(TaskPhase TargetPhase, string? Reason);
public record ApprovalRequest(string? Comment);
public record RequestChangesRequest(string Comment);
