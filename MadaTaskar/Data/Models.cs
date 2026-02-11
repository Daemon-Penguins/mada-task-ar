namespace MadaTaskar.Data;

public record AgentCreateModel(string Name, string Roles);
public record UserCreateModel(string Username, string Password, string DisplayName, bool IsAdmin);
