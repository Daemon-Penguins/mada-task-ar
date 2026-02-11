using MadaTaskar.Data;
using MadaTaskar.Services;
using Microsoft.EntityFrameworkCore;

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
            var columnId = req.ColumnId ?? 1;
            if (columnId is not (1 or 2))
                return Results.BadRequest(new { error = "Tasks can only be created in Ideas (1) or Backlog (2) columns. Use the phase pipeline to advance tasks." });
            var task = new TaskItem
            {
                Title = req.Title,
                Description = req.Description,
                Assignee = req.Assignee ?? agent.Name,
                AssignedAgentId = req.AssignToSelf ? agent.Id : null,
                Priority = req.Priority ?? Priority.Medium,
                ColumnId = columnId,
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

        api.MapGet("/tasks/{id:int}/transitions", async (HttpContext ctx, BoardService boardService, TaskStateMachine stateMachine, int id) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var possibleTargets = stateMachine.GetAllowedTransitions(task.Phase);
            var transitions = possibleTargets.Select(target =>
            {
                var result = stateMachine.TryTransition(task, target, agent);
                var info = stateMachine.GetTransitionInfo(task.Phase, target);
                return new
                {
                    targetPhase = target.ToString(),
                    description = info?.Description ?? "",
                    allowed = result.Success,
                    reason = result.Success ? (string?)null : result.Message
                };
            });

            return Results.Ok(new { currentPhase = task.Phase.ToString(), availableTransitions = transitions });
        });

        api.MapPost("/tasks/{id:int}/advance", async (HttpContext ctx, BoardService boardService, AgentService agentService, TaskStateMachine stateMachine, PermissionService perms, RewardService rewardService, int id, AdvancePhaseRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound(new { error = $"Task #{id} not found." });

            var result = stateMachine.TryTransition(task, req.TargetPhase, agent);
            if (!result.Success)
                return Results.BadRequest(new { error = result.Message, currentPhase = task.Phase.ToString() });

            var fromPhase = task.Phase;
            task.Phase = req.TargetPhase;

            var columnMap = new Dictionary<TaskPhase, int>
            {
                [TaskPhase.Research] = 2, [TaskPhase.Brainstorm] = 2, [TaskPhase.Triage] = 2,
                [TaskPhase.AuthorReview] = 4, [TaskPhase.ReadyToWork] = 2,
                [TaskPhase.InProgress] = 3, [TaskPhase.Acceptance] = 4,
                [TaskPhase.Completed] = 5, [TaskPhase.Killed] = 6
            };
            if (columnMap.TryGetValue(req.TargetPhase, out var colId))
                task.ColumnId = colId;

            task.UpdatedAt = DateTime.UtcNow;
            await boardService.UpdateTaskAsync(task);
            await boardService.AddPhaseLogAsync(new TaskPhaseLog
            {
                TaskId = id, AgentId = agent.Id,
                FromPhase = fromPhase, ToPhase = req.TargetPhase,
                Reason = req.Reason ?? result.Message,
                CreatedAt = DateTime.UtcNow
            });
            await agentService.LogActivityAsync(agent.Id, "advance_phase", $"{fromPhase} → {req.TargetPhase}: {result.Message}", id);

            // Award badges when completing
            object[]? badgesAwarded = null;
            if (req.TargetPhase == TaskPhase.Completed)
            {
                var freshTask = await boardService.GetTaskWithDetailsAsync(id);
                if (freshTask != null)
                {
                    var badges = await rewardService.AwardBadgesForCompletion(freshTask);
                    if (badges.Count > 0)
                    {
                        // Need agent names - load them
                        using var scope = ctx.RequestServices.CreateScope();
                        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                        using var db = await factory.CreateDbContextAsync();
                        badgesAwarded = badges.Select(b =>
                        {
                            var agentName = db.Agents.Find(b.AgentId)?.Name ?? "Unknown";
                            return (object)new { agent = agentName, badge = b.Name, emoji = b.Emoji };
                        }).ToArray();
                    }
                }
            }

            return Results.Ok(new
            {
                taskId = id,
                from = fromPhase.ToString(),
                to = req.TargetPhase.ToString(),
                message = result.Message,
                columnId = task.ColumnId,
                badgesAwarded
            });
        });

        api.MapPost("/tasks/{id:int}/approve", async (HttpContext ctx, BoardService boardService, AgentService agentService, TaskStateMachine stateMachine, int id, ApprovalRequest? req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            TaskPhase targetPhase = task.Phase switch
            {
                TaskPhase.AuthorReview => TaskPhase.ReadyToWork,
                TaskPhase.Acceptance => TaskPhase.Completed,
                _ => TaskPhase.Completed
            };

            var result = stateMachine.TryTransition(task, targetPhase, agent);
            if (!result.Success)
                return Results.BadRequest(new { error = result.Message, currentPhase = task.Phase.ToString() });

            var approval = new TaskApproval { TaskId = id, AgentId = agent.Id, Decision = ApprovalDecision.Approve, Comment = req?.Comment };
            await boardService.AddApprovalAsync(approval);

            var fromPhase = task.Phase;
            await boardService.AddPhaseLogAsync(new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = targetPhase, Reason = "Approved" });

            task.Phase = targetPhase;
            if (PermissionService.PhaseColumnMap.TryGetValue(targetPhase, out var col))
                task.ColumnId = col;
            task.UpdatedAt = DateTime.UtcNow;
            await boardService.UpdateTaskAsync(task);

            await agentService.LogActivityAsync(agent.Id, "approve_task", $"Approved task #{id}: {fromPhase} → {targetPhase}", id);
            return Results.Ok(new { task.Id, task.Phase, task.ColumnId, from = fromPhase, to = targetPhase });
        });

        api.MapPost("/tasks/{id:int}/reject", async (HttpContext ctx, BoardService boardService, AgentService agentService, TaskStateMachine stateMachine, int id, ApprovalRequest? req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var result = stateMachine.TryTransition(task, TaskPhase.Killed, agent);
            if (!result.Success)
                return Results.BadRequest(new { error = result.Message, currentPhase = task.Phase.ToString() });

            var approval = new TaskApproval { TaskId = id, AgentId = agent.Id, Decision = ApprovalDecision.Reject, Comment = req?.Comment };
            await boardService.AddApprovalAsync(approval);

            var fromPhase = task.Phase;
            await boardService.AddPhaseLogAsync(new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = TaskPhase.Killed, Reason = req?.Comment ?? "Rejected" });

            task.Phase = TaskPhase.Killed;
            task.ColumnId = 6;
            task.UpdatedAt = DateTime.UtcNow;
            await boardService.UpdateTaskAsync(task);

            await agentService.LogActivityAsync(agent.Id, "reject_task", $"Rejected task #{id}", id);
            return Results.Ok(new { task.Id, task.Phase, task.ColumnId });
        });

        api.MapPost("/tasks/{id:int}/request-changes", async (HttpContext ctx, BoardService boardService, AgentService agentService, TaskStateMachine stateMachine, int id, RequestChangesRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var result = stateMachine.TryTransition(task, TaskPhase.InProgress, agent);
            if (!result.Success)
                return Results.BadRequest(new { error = result.Message, currentPhase = task.Phase.ToString() });

            var approval = new TaskApproval { TaskId = id, AgentId = agent.Id, Decision = ApprovalDecision.RequestChanges, Comment = req.Comment };
            await boardService.AddApprovalAsync(approval);

            var fromPhase = task.Phase;
            await boardService.AddPhaseLogAsync(new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = TaskPhase.InProgress, Reason = req.Comment });

            task.Phase = TaskPhase.InProgress;
            task.ColumnId = 3;
            task.UpdatedAt = DateTime.UtcNow;
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

        // --- Acceptance Criteria ---
        api.MapGet("/tasks/{id:int}/criteria", async (HttpContext ctx, BoardService boardService, int id) =>
        {
            var criteria = await boardService.GetAcceptanceCriteriaAsync(id);
            return Results.Ok(criteria.Select(c => new { c.Id, c.TaskId, c.Description, c.IsMet, c.Order, c.CreatedAt, c.CheckedAt, CheckedByAgent = c.CheckedByAgent?.Name }));
        });

        api.MapPost("/tasks/{id:int}/criteria", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id, AddCriterionRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();
            var criterion = new AcceptanceCriterion { TaskId = id, Description = req.Description, Order = req.Order ?? 0 };
            await boardService.AddAcceptanceCriterionAsync(criterion);
            await agentService.LogActivityAsync(agent.Id, "add_criterion", $"Added criterion '{req.Description}' to task #{id}", id);
            return Results.Created($"/api/tasks/{id}/criteria/{criterion.Id}", new { criterion.Id, criterion.TaskId, criterion.Description, criterion.Order, criterion.CreatedAt });
        });

        api.MapPut("/tasks/{id:int}/criteria/{cid:int}", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id, int cid, UpdateCriterionRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();
            var criterion = task.AcceptanceCriteria.FirstOrDefault(c => c.Id == cid);
            if (criterion is null) return Results.NotFound();

            await boardService.UpdateAcceptanceCriterionAsync(cid, req.IsMet, agent.Id);
            await agentService.LogActivityAsync(agent.Id, "update_criterion", $"{(req.IsMet ? "Checked" : "Unchecked")} criterion '{criterion.Description}' on task #{id}", id);

            // Check if all criteria are now met and task is in Acceptance phase
            if (req.IsMet)
            {
                var allCriteria = await boardService.GetAcceptanceCriteriaAsync(id);
                // Refresh the one we just updated
                var allMet = allCriteria.All(c => c.Id == cid ? true : c.IsMet);
                if (allMet && allCriteria.Count > 0 && task.Phase == TaskPhase.Acceptance)
                {
                    var comment = new TaskComment { TaskId = id, AgentId = agent.Id, Content = "All acceptance criteria met — ready for auto-accept", Type = CommentType.General };
                    await boardService.AddCommentAsync(comment);
                }
            }

            return Results.Ok(new { cid, isMet = req.IsMet });
        });

        api.MapDelete("/tasks/{id:int}/criteria/{cid:int}", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id, int cid) =>
        {
            var agent = GetAgent(ctx);
            await boardService.DeleteAcceptanceCriterionAsync(cid);
            await agentService.LogActivityAsync(agent.Id, "delete_criterion", $"Deleted criterion #{cid} from task #{id}", id);
            return Results.Ok();
        });

        api.MapPost("/tasks/{id:int}/auto-accept", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id) =>
        {
            var agent = GetAgent(ctx);
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var criteria = await boardService.GetAcceptanceCriteriaAsync(id);
            if (criteria.Count == 0 || !criteria.All(c => c.IsMet))
                return Results.Json(new { error = "Not all acceptance criteria are met" }, statusCode: 400);

            var fromPhase = task.Phase;
            await boardService.AddPhaseLogAsync(new TaskPhaseLog { TaskId = id, AgentId = agent.Id, FromPhase = fromPhase, ToPhase = TaskPhase.Completed, Reason = "Auto-accepted: all criteria met" });
            task.Phase = TaskPhase.Completed;
            task.ColumnId = 5; // Done
            await boardService.UpdateTaskAsync(task);
            await agentService.LogActivityAsync(agent.Id, "auto_accept", $"Auto-accepted task #{id}: all criteria met", id);
            return Results.Ok(new { task.Id, task.Phase, task.ColumnId, from = fromPhase, to = TaskPhase.Completed });
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

        // --- Retrospective ---
        api.MapGet("/tasks/{id:int}/retrospective", async (HttpContext ctx, BoardService boardService, int id) =>
        {
            var task = await boardService.GetTaskWithDetailsAsync(id);
            if (task is null) return Results.NotFound();

            var phases = new List<object>();
            var phaseLogs = task.PhaseLogs.OrderBy(p => p.CreatedAt).ToList();
            for (int i = 0; i < phaseLogs.Count; i++)
            {
                var log = phaseLogs[i];
                var nextTime = i + 1 < phaseLogs.Count ? phaseLogs[i + 1].CreatedAt : DateTime.UtcNow;
                var duration = nextTime - log.CreatedAt;
                phases.Add(new { phase = log.ToPhase.ToString(), duration = FormatDuration(duration), agent = log.Agent.Name });
            }

            var totalDuration = phaseLogs.Count > 0 ? DateTime.UtcNow - phaseLogs.First().CreatedAt : TimeSpan.Zero;

            var contributors = new HashSet<string>();
            foreach (var p in task.PhaseLogs) contributors.Add(p.Agent.Name);
            foreach (var c in task.Comments) contributors.Add(c.Agent.Name);
            foreach (var a in task.Approvals) contributors.Add(a.Agent.Name);
            foreach (var ac in task.AcceptanceCriteria.Where(x => x.CheckedByAgent != null)) contributors.Add(ac.CheckedByAgent!.Name);

            var rejectsCount = task.Approvals.Count(a => a.Decision == ApprovalDecision.Reject);
            var requestChangesCount = task.Approvals.Count(a => a.Decision == ApprovalDecision.RequestChanges);
            var criteriaMet = task.AcceptanceCriteria.Count(c => c.IsMet);
            var criteriaTotal = task.AcceptanceCriteria.Count;

            return Results.Ok(new
            {
                taskId = task.Id,
                title = task.Title,
                totalDuration = FormatDuration(totalDuration),
                phases,
                researchReferences = task.References.Select(r => new { r.Id, r.Url, r.Title, r.Summary, r.CreatedAt, agent = r.Agent.Name }),
                proposals = task.Comments.Where(c => c.Type == CommentType.Proposal).Select(c => new { c.Id, c.Content, c.CreatedAt, agent = c.Agent.Name }),
                acceptanceCriteria = task.AcceptanceCriteria.Select(c => new { c.Description, met = c.IsMet, checkedBy = c.CheckedByAgent?.Name }),
                approvals = task.Approvals.Select(a => new { a.Decision, a.Comment, a.CreatedAt, agent = a.Agent.Name }),
                comments = task.Comments.Select(c => new { c.Id, c.Content, c.Type, c.CreatedAt, agent = c.Agent.Name }),
                contributors = contributors.ToList(),
                phaseTransitions = task.PhaseLogs.Select(p => new { from = p.FromPhase?.ToString(), to = p.ToPhase.ToString(), p.Reason, p.CreatedAt, agent = p.Agent.Name }),
                lessonsLearned = new
                {
                    totalPhaseChanges = task.PhaseLogs.Count,
                    rejectsCount,
                    requestChangesCount,
                    firstTimeRight = rejectsCount == 0 && requestChangesCount == 0,
                    criteriaMet = $"{criteriaMet}/{criteriaTotal}"
                }
            });
        });

        // --- Badges ---
        api.MapGet("/agents/{id:int}/badges", async (HttpContext ctx, RewardService rewardService, int id) =>
        {
            var badges = await rewardService.GetBadgesForAgent(id);
            return Results.Ok(badges.Select(b => new { b.Id, b.Badge, b.Name, b.Emoji, b.TaskTitle, b.TaskId, b.EarnedAt }));
        });

        api.MapGet("/me/badges", async (HttpContext ctx, RewardService rewardService) =>
        {
            var agent = GetAgent(ctx);
            var badges = await rewardService.GetBadgesForAgent(agent.Id);
            return Results.Ok(badges.Select(b => new { b.Id, b.Badge, b.Name, b.Emoji, b.TaskTitle, b.TaskId, b.EarnedAt }));
        });

        // --- Who am I ---
        api.MapGet("/me", (HttpContext ctx) =>
        {
            var agent = GetAgent(ctx);
            return Results.Ok(new { agent.Id, agent.Name, agent.Roles, agent.IsActive, agent.CreatedAt, agent.LastSeenAt });
        });
    }

    private static Agent GetAgent(HttpContext ctx) => (Agent)ctx.Items["Agent"]!;

    private static string FormatDuration(TimeSpan ts)
    {
        if (ts.TotalDays >= 1) return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
        if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        return $"{(int)ts.TotalMinutes}m";
    }
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
public record AdvancePhaseRequest(TaskPhase TargetPhase, string? Reason);
public record ApprovalRequest(string? Comment);
public record RequestChangesRequest(string Comment);
public record AddCriterionRequest(string Description, int? Order);
public record UpdateCriterionRequest(bool IsMet);
