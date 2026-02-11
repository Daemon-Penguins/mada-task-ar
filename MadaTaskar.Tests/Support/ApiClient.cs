using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MadaTaskar.Tests.Support;

/// <summary>
/// HTTP client wrapper for the Agent REST API.
/// Handles authentication via X-Agent-Key header.
/// </summary>
public class ApiClient : IDisposable
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public HttpResponseMessage? LastResponse { get; private set; }

    public ApiClient(HttpClient client, string apiKey)
    {
        _client = client;
        _client.DefaultRequestHeaders.Remove("X-Agent-Key");
        _client.DefaultRequestHeaders.Add("X-Agent-Key", apiKey);
    }

    public static ApiClient WithTestBot()
    {
        var client = new HttpClient { BaseAddress = new Uri(TestFixture.BaseUrl) };
        return new ApiClient(client, TestData.TestBotApiKey);
    }

    public static ApiClient WithRico()
    {
        var client = new HttpClient { BaseAddress = new Uri(TestFixture.BaseUrl) };
        return new ApiClient(client, TestData.RicoApiKey);
    }

    public static ApiClient WithInvalidKey()
    {
        var client = new HttpClient { BaseAddress = new Uri(TestFixture.BaseUrl) };
        return new ApiClient(client, "invalid-key-does-not-exist");
    }

    // ── Identity ──────────────────────────────────────────────

    public async Task<JsonElement> GetMe()
    {
        LastResponse = await _client.GetAsync("/api/me");
        return await ReadJson();
    }

    public async Task<JsonElement> GetMyBadges()
    {
        LastResponse = await _client.GetAsync("/api/me/badges");
        return await ReadJson();
    }

    // ── Tasks ─────────────────────────────────────────────────

    public async Task<JsonElement> CreateTask(string title, string? description = null, int? columnId = null, bool assignToSelf = false)
    {
        LastResponse = await _client.PostAsJsonAsync("/api/tasks", new
        {
            Title = title,
            Description = description,
            ColumnId = columnId,
            AssignToSelf = assignToSelf
        });
        return await ReadJson();
    }

    public async Task<JsonElement> GetBoard()
    {
        LastResponse = await _client.GetAsync("/api/board");
        return await ReadJson();
    }

    public async Task<JsonElement> AdvancePhase(int taskId, string targetPhase, string? reason = null)
    {
        LastResponse = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/advance", new
        {
            TargetPhase = targetPhase,
            Reason = reason
        });
        return await ReadJson();
    }

    public async Task<JsonElement> AddResearch(int taskId, string url, string title, string? summary = null)
    {
        LastResponse = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/research", new
        {
            Url = url,
            Title = title,
            Summary = summary
        });
        return await ReadJson();
    }

    public async Task<JsonElement> AddProposal(int taskId, string content)
    {
        LastResponse = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/propose", new
        {
            Content = content
        });
        return await ReadJson();
    }

    public async Task<JsonElement> AssignTask(int taskId, int? agentId = null)
    {
        LastResponse = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/assign", agentId.HasValue ? new { AgentId = agentId } : null);
        return await ReadJson();
    }

    public async Task<JsonElement> ApproveTask(int taskId, string? comment = null)
    {
        LastResponse = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/approve", new { Comment = comment });
        return await ReadJson();
    }

    public async Task<JsonElement> UpdateTask(int taskId, object updates)
    {
        LastResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updates);
        return await ReadJson();
    }

    public async Task<JsonElement> GetTransitions(int taskId)
    {
        LastResponse = await _client.GetAsync($"/api/tasks/{taskId}/transitions");
        return await ReadJson();
    }

    public async Task<JsonElement> GetRetrospective(int taskId)
    {
        LastResponse = await _client.GetAsync($"/api/tasks/{taskId}/retrospective");
        return await ReadJson();
    }

    // ── Acceptance Criteria ───────────────────────────────────

    public async Task<JsonElement> AddCriterion(int taskId, string description, int? order = null)
    {
        LastResponse = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/criteria", new
        {
            Description = description,
            Order = order
        });
        return await ReadJson();
    }

    public async Task<JsonElement> UpdateCriterion(int taskId, int criterionId, bool isMet)
    {
        LastResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}/criteria/{criterionId}", new { IsMet = isMet });
        return await ReadJson();
    }

    public async Task<JsonElement> AutoAccept(int taskId)
    {
        LastResponse = await _client.PostAsync($"/api/tasks/{taskId}/auto-accept", null);
        return await ReadJson();
    }

    // ── Agents ────────────────────────────────────────────────

    public async Task<JsonElement> RegisterAgent(string name, string roles = "worker")
    {
        LastResponse = await _client.PostAsJsonAsync("/api/agents/register", new { Name = name, Roles = roles });
        return await ReadJson();
    }

    public async Task<JsonElement> DeactivateAgent(int agentId)
    {
        LastResponse = await _client.DeleteAsync($"/api/agents/{agentId}");
        return await ReadJson();
    }

    // ── Helpers ───────────────────────────────────────────────

    public HttpStatusCode StatusCode => LastResponse?.StatusCode ?? HttpStatusCode.InternalServerError;

    private async Task<JsonElement> ReadJson()
    {
        if (LastResponse == null || LastResponse.Content.Headers.ContentLength == 0)
            return default;

        try
        {
            var content = await LastResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content)) return default;
            return JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        }
        catch
        {
            return default;
        }
    }

    public void Dispose() => _client.Dispose();
}
