using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MadaTaskar.Data;

namespace MadaTaskar.Services;

public class AgentService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AgentService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Agent> RegisterAgentAsync(string name, string role = "worker")
    {
        using var db = await _factory.CreateDbContextAsync();
        var agent = new Agent
        {
            Name = name,
            ApiKey = $"pk-{Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLower()}",
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();
        return agent;
    }

    public async Task<Agent?> AuthenticateAsync(string apiKey)
    {
        using var db = await _factory.CreateDbContextAsync();
        var agent = await db.Agents.FirstOrDefaultAsync(a => a.ApiKey == apiKey && a.IsActive);
        if (agent is not null)
        {
            agent.LastSeenAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        return agent;
    }

    public async Task<List<Agent>> GetAgentsAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Agents.OrderBy(a => a.Id).ToListAsync();
    }

    public async Task<bool> DeactivateAgentAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var agent = await db.Agents.FindAsync(id);
        if (agent is null) return false;
        agent.IsActive = false;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task LogActivityAsync(int agentId, string action, string? details = null, int? taskId = null)
    {
        using var db = await _factory.CreateDbContextAsync();
        db.AgentActivities.Add(new AgentActivity
        {
            AgentId = agentId,
            Action = action,
            Details = details,
            TaskId = taskId,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<AgentActivity>> GetActivityLogAsync(int limit = 50)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.AgentActivities
            .Include(a => a.Agent)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
