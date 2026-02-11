using MadaTaskar.Data;

namespace MadaTaskar.Services;

public class PermissionService
{
    public bool CanCreateTask(Agent agent) => agent.HasRole("admin") || agent.HasRole("researcher") || agent.HasRole("architect") || agent.HasRole("worker") || agent.HasRole("reviewer");
    public bool CanResearch(Agent agent) => agent.HasRole("admin") || agent.HasRole("researcher") || agent.HasRole("architect");
    public bool CanPropose(Agent agent) => agent.HasRole("admin") || agent.HasRole("researcher") || agent.HasRole("architect") || agent.HasRole("worker");
    public bool CanTriage(Agent agent) => agent.HasRole("admin") || agent.HasRole("architect");
    public bool CanApproveReview(Agent agent, TaskItem task) => agent.HasRole("admin") || agent.Id == task.AuthorAgentId || agent.HasRole("architect");
    public bool CanTakeWork(Agent agent) => agent.HasRole("admin") || agent.HasRole("worker");
    public bool CanAccept(Agent agent, TaskItem task) => agent.HasRole("admin") || agent.Id == task.AuthorAgentId || agent.HasRole("reviewer");
    public bool CanDeleteTask(Agent agent) => agent.HasRole("admin");
    public bool CanManageAgents(Agent agent) => agent.HasRole("admin");
    public bool CanComment(Agent agent) => true;

    public bool CanAdvancePhase(Agent agent, TaskItem task, TaskPhase targetPhase)
    {
        return (task.Phase, targetPhase) switch
        {
            (TaskPhase.Research, TaskPhase.Brainstorm) => CanResearch(agent),
            (TaskPhase.Brainstorm, TaskPhase.Triage) => CanTriage(agent),
            (TaskPhase.Triage, TaskPhase.AuthorReview) => CanTriage(agent),
            (TaskPhase.AuthorReview, TaskPhase.ReadyToWork) => agent.HasRole("admin") || agent.Id == task.AuthorAgentId,
            (TaskPhase.ReadyToWork, TaskPhase.InProgress) => CanTakeWork(agent),
            (TaskPhase.InProgress, TaskPhase.Acceptance) => CanTakeWork(agent),
            (TaskPhase.Acceptance, TaskPhase.Completed) => CanAccept(agent, task),
            (TaskPhase.Acceptance, TaskPhase.InProgress) => CanAccept(agent, task),
            (TaskPhase.Acceptance, TaskPhase.Research) => agent.HasRole("admin") || agent.Id == task.AuthorAgentId,
            (_, TaskPhase.Killed) => agent.HasRole("admin") || agent.Id == task.AuthorAgentId,
            _ => false
        };
    }

    public static readonly Dictionary<TaskPhase, int> PhaseColumnMap = new()
    {
        [TaskPhase.Research] = 2,
        [TaskPhase.Brainstorm] = 2,
        [TaskPhase.Triage] = 2,
        [TaskPhase.AuthorReview] = 4,
        [TaskPhase.ReadyToWork] = 2,
        [TaskPhase.InProgress] = 3,
        [TaskPhase.Acceptance] = 4,
        [TaskPhase.Completed] = 5,
        [TaskPhase.Killed] = 6,
    };
}
