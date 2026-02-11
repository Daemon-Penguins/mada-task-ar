using MadaTaskar.Data;
using MadaTaskar.Services;

namespace MadaTaskar.Api;

public static class AgentApiEndpoints
{
    public static void MapAgentApi(this WebApplication app)
    {
        var api = app.MapGroup("/api").AddEndpointFilter<AgentAuthFilter>();

        // --- Agents ---
        api.MapGet("/agents", async (HttpContext ctx, AgentService agentService) =>
        {
            var agent = GetAgent(ctx);
            if (agent.Role != "admin") return Results.Forbid();
            return Results.Ok(await agentService.GetAgentsAsync());
        });

        api.MapPost("/agents/register", async (HttpContext ctx, AgentService agentService, RegisterAgentRequest req) =>
        {
            var agent = GetAgent(ctx);
            if (agent.Role != "admin") return Results.Forbid();
            var newAgent = await agentService.RegisterAgentAsync(req.Name, req.Role ?? "worker");
            await agentService.LogActivityAsync(agent.Id, "register_agent", $"Registered '{newAgent.Name}' ({newAgent.Role})");
            return Results.Ok(newAgent);
        });

        api.MapDelete("/agents/{id:int}", async (HttpContext ctx, AgentService agentService, int id) =>
        {
            var agent = GetAgent(ctx);
            if (agent.Role != "admin") return Results.Forbid();
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
        api.MapPost("/tasks", async (HttpContext ctx, BoardService boardService, AgentService agentService, CreateTaskRequest req) =>
        {
            var agent = GetAgent(ctx);
            var task = new TaskItem
            {
                Title = req.Title,
                Description = req.Description,
                Assignee = req.Assignee ?? agent.Name,
                AssignedAgentId = req.AssignToSelf ? agent.Id : null,
                Priority = req.Priority ?? Priority.Medium,
                ColumnId = req.ColumnId ?? 1,
                Order = req.Order ?? 0
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

        api.MapDelete("/tasks/{id:int}", async (HttpContext ctx, BoardService boardService, AgentService agentService, int id) =>
        {
            var agent = GetAgent(ctx);
            await boardService.DeleteTaskAsync(id);
            await agentService.LogActivityAsync(agent.Id, "delete_task", $"Deleted task #{id}", id);
            return Results.Ok();
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
            return Results.Ok(new { agent.Id, agent.Name, agent.Role, agent.IsActive, agent.CreatedAt, agent.LastSeenAt });
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
public record RegisterAgentRequest(string Name, string? Role);
public record CreateTaskRequest(string Title, string? Description, string? Assignee, Priority? Priority, int? ColumnId, int? Order, bool AssignToSelf = false);
public record UpdateTaskRequest(string? Title, string? Description, string? Assignee, Priority? Priority, int? ColumnId, int? Order);
public record MoveTaskRequest(int ColumnId, int? Order);
public record AssignTaskRequest(int? AgentId);
