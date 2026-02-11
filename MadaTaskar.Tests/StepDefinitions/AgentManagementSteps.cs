using System.Net;
using FluentAssertions;
using MadaTaskar.Tests.Support;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Tests for Agent Management — creating, editing roles, and deactivating agents via API.
/// </summary>
[TestFixture, Category("AgentManagement"), Category("API")]
public class AgentManagementTests
{
    private ApiClient _api = null!;

    [SetUp]
    public void SetUp()
    {
        _api = ApiClient.WithRico(); // Rico has admin role
    }

    [TearDown]
    public void TearDown()
    {
        _api.Dispose();
    }

    [Test]
    public async Task Admin_Can_Register_A_New_Agent()
    {
        // When I register a new agent with name "Skipper" and roles "admin,architect"
        var result = await _api.RegisterAgent("Skipper", "admin,architect");

        // Then the agent should be created successfully
        _api.StatusCode.Should().Be(HttpStatusCode.OK);
        result.GetProperty("name").GetString().Should().Be("Skipper");
        result.GetProperty("roles").GetString().Should().Contain("admin");
        result.GetProperty("roles").GetString().Should().Contain("architect");
    }

    [Test]
    public async Task Admin_Can_Deactivate_An_Agent()
    {
        // Given there is an agent
        var created = await _api.RegisterAgent("DeactivateMe", "worker");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);
        var agentId = created.GetProperty("id").GetInt32();

        // When I deactivate the agent
        await _api.DeactivateAgent(agentId);

        // Then it should succeed
        _api.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Non_Admin_Cannot_Register_Agents()
    {
        // Create a non-admin agent first
        var workerResult = await _api.RegisterAgent("WorkerOnly", "worker");
        var workerKey = workerResult.GetProperty("apiKey").GetString()!;

        // Try to register with the worker agent
        using var workerApi = new ApiClient(TestFixture.Factory.CreateClient(), workerKey);
        await workerApi.RegisterAgent("ShouldFail", "admin");

        // Should be forbidden
        workerApi.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
