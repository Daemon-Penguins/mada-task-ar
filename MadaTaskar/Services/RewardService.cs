using MadaTaskar.Data;
using Microsoft.EntityFrameworkCore;

namespace MadaTaskar.Services;

public class RewardService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public RewardService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<AgentBadge>> AwardBadgesForCompletion(TaskItem task)
    {
        using var db = await _factory.CreateDbContextAsync();
        var awarded = new List<AgentBadge>();

        var contributorIds = new HashSet<int>();
        if (task.PhaseLogs != null) contributorIds.UnionWith(task.PhaseLogs.Select(p => p.AgentId));
        if (task.Comments != null) contributorIds.UnionWith(task.Comments.Select(c => c.AgentId));
        if (task.Approvals != null) contributorIds.UnionWith(task.Approvals.Select(a => a.AgentId));
        if (task.AcceptanceCriteria != null) contributorIds.UnionWith(task.AcceptanceCriteria.Where(c => c.CheckedByAgentId.HasValue).Select(c => c.CheckedByAgentId!.Value));
        if (task.AssignedAgentId.HasValue) contributorIds.Add(task.AssignedAgentId.Value);

        foreach (var agentId in contributorIds)
        {
            var agent = await db.Agents.FindAsync(agentId);
            if (agent is null) continue;

            var existingBadges = await db.AgentBadges.Where(b => b.AgentId == agentId).ToListAsync();

            // 🏆 First Completion
            if (!existingBadges.Any(b => b.Badge == "first_completion"))
            {
                var badge = new AgentBadge { AgentId = agentId, Badge = "first_completion", Name = "First Blood", Emoji = "🏆", TaskTitle = task.Title, TaskId = task.Id, EarnedAt = DateTime.UtcNow };
                db.AgentBadges.Add(badge);
                awarded.Add(badge);
            }

            // ⚡ Speed Demon
            if (task.PhaseLogs?.Any() == true)
            {
                var first = task.PhaseLogs.MinBy(p => p.CreatedAt);
                if (first != null && (DateTime.UtcNow - first.CreatedAt).TotalHours < 1 && !existingBadges.Any(b => b.Badge == "speed_demon"))
                {
                    var badge = new AgentBadge { AgentId = agentId, Badge = "speed_demon", Name = "Speed Demon", Emoji = "⚡", TaskTitle = task.Title, TaskId = task.Id, EarnedAt = DateTime.UtcNow };
                    db.AgentBadges.Add(badge);
                    awarded.Add(badge);
                }
            }

            var activityCompletions = await db.AgentActivities.CountAsync(a => a.AgentId == agentId && a.Action == "advance_phase" && a.Details != null && a.Details.Contains("Completed"));

            // 🌟 5 completions
            if (activityCompletions >= 5 && !existingBadges.Any(b => b.Badge == "5_completions"))
            {
                var badge = new AgentBadge { AgentId = agentId, Badge = "5_completions", Name = "High Five!", Emoji = "🌟", TaskTitle = task.Title, TaskId = task.Id, EarnedAt = DateTime.UtcNow };
                db.AgentBadges.Add(badge);
                awarded.Add(badge);
            }

            // 👑 10 completions
            if (activityCompletions >= 10 && !existingBadges.Any(b => b.Badge == "10_completions"))
            {
                var badge = new AgentBadge { AgentId = agentId, Badge = "10_completions", Name = "Penguin Commander", Emoji = "👑", TaskTitle = task.Title, TaskId = task.Id, EarnedAt = DateTime.UtcNow };
                db.AgentBadges.Add(badge);
                awarded.Add(badge);
            }
        }

        // 🔍 Research Master
        if (task.References?.Any() == true)
        {
            var researchGroups = task.References.GroupBy(r => r.AgentId).Where(g => g.Count() >= 3);
            foreach (var group in researchGroups)
            {
                var existing = await db.AgentBadges.AnyAsync(b => b.AgentId == group.Key && b.Badge == "research_master");
                if (!existing)
                {
                    var badge = new AgentBadge { AgentId = group.Key, Badge = "research_master", Name = "Research Master", Emoji = "🔍", TaskTitle = task.Title, TaskId = task.Id, EarnedAt = DateTime.UtcNow };
                    db.AgentBadges.Add(badge);
                    awarded.Add(badge);
                }
            }
        }

        // 🎯 Perfect Score
        var noRejects = task.Approvals?.All(a => a.Decision != ApprovalDecision.Reject) ?? true;
        var noChangeRequests = task.Approvals?.All(a => a.Decision != ApprovalDecision.RequestChanges) ?? true;
        if (noRejects && noChangeRequests && task.AssignedAgentId.HasValue)
        {
            var existing = await db.AgentBadges.AnyAsync(b => b.AgentId == task.AssignedAgentId.Value && b.Badge == "perfect_score" && b.TaskId == task.Id);
            if (!existing)
            {
                var badge = new AgentBadge { AgentId = task.AssignedAgentId.Value, Badge = "perfect_score", Name = "Perfect Score", Emoji = "🎯", TaskTitle = task.Title, TaskId = task.Id, EarnedAt = DateTime.UtcNow };
                db.AgentBadges.Add(badge);
                awarded.Add(badge);
            }
        }

        await db.SaveChangesAsync();
        return awarded;
    }

    public async Task<List<AgentBadge>> GetBadgesForAgent(int agentId)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.AgentBadges.Where(b => b.AgentId == agentId).OrderByDescending(b => b.EarnedAt).ToListAsync();
    }
}
