namespace MadaTaskar.Tests.Support;

/// <summary>
/// Shared test data — known credentials, API keys, and IDs used across all tests.
/// </summary>
public static class TestData
{
    // Default user seeded by the application
    public const string DefaultUsername = "user";
    public const string DefaultPassword = "password";

    // Rico agent — seeded by the application
    public const string RicoApiKey = "penguin-rico-key-change-me";
    public const int RicoAgentId = 1;

    // TestBot agent — created during test setup
    public static string TestBotApiKey { get; set; } = string.Empty;
    public static int TestBotId { get; set; }

    // Board columns
    public const int IdeasColumnId = 1;
    public const int BacklogColumnId = 2;
    public const int InProgressColumnId = 3;
    public const int AcceptanceColumnId = 4;
    public const int DoneColumnId = 5;
    public const int RejectedColumnId = 6;
}
