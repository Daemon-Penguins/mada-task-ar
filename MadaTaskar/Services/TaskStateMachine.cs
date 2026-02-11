using MadaTaskar.Data;

namespace MadaTaskar.Services;

public class TaskStateMachine
{
    private static readonly Dictionary<(TaskPhase From, TaskPhase To), PhaseTransition> _transitions = new()
    {
        [(TaskPhase.Research, TaskPhase.Brainstorm)] = new("Research completed, open for brainstorming", "researcher", "architect", "admin"),
        [(TaskPhase.Brainstorm, TaskPhase.Triage)] = new("Brainstorming done, ready for triage", "architect", "admin"),
        [(TaskPhase.Triage, TaskPhase.AuthorReview)] = new("Triage complete, awaiting author review", "architect", "admin"),
        [(TaskPhase.AuthorReview, TaskPhase.ReadyToWork)] = new("Author approved, ready to work", "author", "admin"),
        [(TaskPhase.ReadyToWork, TaskPhase.InProgress)] = new("Work started", "worker", "admin"),
        [(TaskPhase.InProgress, TaskPhase.Acceptance)] = new("Work completed, awaiting acceptance", "worker", "admin"),
        [(TaskPhase.Acceptance, TaskPhase.Completed)] = new("Accepted and done", "author", "reviewer", "admin"),
        [(TaskPhase.Acceptance, TaskPhase.InProgress)] = new("Changes requested, back to work", "author", "reviewer", "admin"),
        [(TaskPhase.Acceptance, TaskPhase.Research)] = new("Too many issues, restarting with gathered knowledge", "author", "admin"),
        // Any phase can go to Killed
        [(TaskPhase.Research, TaskPhase.Killed)] = new("Idea rejected", "author", "admin"),
        [(TaskPhase.Brainstorm, TaskPhase.Killed)] = new("Idea rejected", "author", "admin"),
        [(TaskPhase.Triage, TaskPhase.Killed)] = new("Idea rejected", "author", "admin"),
        [(TaskPhase.AuthorReview, TaskPhase.Killed)] = new("Idea rejected", "author", "admin"),
        [(TaskPhase.ReadyToWork, TaskPhase.Killed)] = new("Idea rejected", "author", "admin"),
        [(TaskPhase.InProgress, TaskPhase.Killed)] = new("Task killed", "author", "admin"),
        [(TaskPhase.Acceptance, TaskPhase.Killed)] = new("Task rejected permanently", "author", "admin"),
    };

    public TransitionResult TryTransition(TaskItem task, TaskPhase targetPhase, Agent agent)
    {
        if (task.Phase == targetPhase)
            return TransitionResult.Fail($"Task is already in '{targetPhase}' phase.");

        var key = (task.Phase, targetPhase);
        if (!_transitions.TryGetValue(key, out var transition))
        {
            var allowed = GetAllowedTransitions(task.Phase)
                .Select(t => t.ToString())
                .ToList();
            var allowedStr = allowed.Any() ? string.Join(", ", allowed) : "none";
            return TransitionResult.Fail(
                $"Invalid transition: '{task.Phase}' → '{targetPhase}'. " +
                $"Allowed transitions from '{task.Phase}': [{allowedStr}].");
        }

        var hasPermission = transition.AllowedRoles.Any(role =>
        {
            if (role == "author") return agent.Id == task.AuthorAgentId;
            return agent.HasRole(role);
        });

        if (!hasPermission)
        {
            return TransitionResult.Fail(
                $"Permission denied: '{task.Phase}' → '{targetPhase}' requires role(s): [{string.Join(", ", transition.AllowedRoles)}]. " +
                $"Agent '{agent.Name}' has roles: [{agent.Roles}]" +
                (transition.AllowedRoles.Contains("author") ? $". Task author agent ID: {task.AuthorAgentId ?? 0}, your ID: {agent.Id}" : "") +
                ".");
        }

        if (targetPhase == TaskPhase.ReadyToWork && !task.ReadyToWorkChecked)
            return TransitionResult.Fail("Cannot advance to 'ReadyToWork': the 'ReadyToWorkChecked' flag must be set to true first.");

        if (targetPhase == TaskPhase.InProgress && task.Phase == TaskPhase.ReadyToWork && task.AssignedAgentId is null)
            return TransitionResult.Fail("Cannot start work: task must be assigned to an agent first. Use POST /api/tasks/{id}/assign.");

        return TransitionResult.Ok(transition.Description);
    }

    public List<TaskPhase> GetAllowedTransitions(TaskPhase currentPhase)
    {
        return _transitions.Keys
            .Where(k => k.From == currentPhase)
            .Select(k => k.To)
            .ToList();
    }

    public PhaseTransition? GetTransitionInfo(TaskPhase from, TaskPhase to)
    {
        _transitions.TryGetValue((from, to), out var t);
        return t;
    }
}

public record PhaseTransition(string Description, params string[] AllowedRoles);

public class TransitionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static TransitionResult Ok(string message) => new() { Success = true, Message = message };
    public static TransitionResult Fail(string message) => new() { Success = false, Message = message };
}
