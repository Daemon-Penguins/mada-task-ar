using System.Net;
using System.Text.Json;
using FluentAssertions;
using MadaTaskar.Tests.Support;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Tests for the Agent REST API — authentication, task CRUD, retrospectives, and badges.
/// </summary>
[TestFixture, Category("ApiIntegration"), Category("API")]
public class ApiIntegrationTests
{
    [Test]
    public async Task Agent_Can_Authenticate_With_Valid_API_Key()
    {
        using var api = ApiClient.WithRico();

        // When I call GET /api/me
        var me = await api.GetMe();

        // Then I should receive my agent details
        api.StatusCode.Should().Be(HttpStatusCode.OK);
        me.GetProperty("name").GetString().Should().Be("Rico");
        me.GetProperty("roles").GetString().Should().Contain("admin");
    }

    [Test]
    public async Task Invalid_API_Key_Returns_401_Unauthorized()
    {
        using var api = ApiClient.WithInvalidKey();

        // When I call GET /api/me with invalid key
        await api.GetMe();

        // Then I should receive 401
        api.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Agent_Can_Create_A_Task()
    {
        using var api = ApiClient.WithRico();

        // When I create a task
        var task = await api.CreateTask("API Test Task");

        // Then I should get 201 Created
        api.StatusCode.Should().Be(HttpStatusCode.Created);
        task.GetProperty("title").GetString().Should().Be("API Test Task");
    }

    [Test]
    public async Task Agent_Cannot_Create_Task_In_Restricted_Column()
    {
        using var api = ApiClient.WithRico();

        // When I try to create a task in column 3 (In Progress)
        await api.CreateTask("Restricted Task", columnId: 3);

        // Then I should get 400 Bad Request
        api.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Agent_Can_Get_Task_Retrospective()
    {
        using var api = ApiClient.WithRico();

        // Create and complete a task through the full lifecycle
        var task = await api.CreateTask("Retro Test");
        var taskId = task.GetProperty("id").GetInt32();

        await api.AdvancePhase(taskId, "Brainstorm");
        await api.AdvancePhase(taskId, "Triage");
        await api.AdvancePhase(taskId, "AuthorReview");
        await api.UpdateTask(taskId, new { ReadyToWorkChecked = true });
        await api.AdvancePhase(taskId, "ReadyToWork");
        await api.AssignTask(taskId);
        await api.AdvancePhase(taskId, "InProgress");
        await api.AdvancePhase(taskId, "Acceptance");
        await api.ApproveTask(taskId);

        // When I get the retrospective
        var retro = await api.GetRetrospective(taskId);

        // Then I should see the lifecycle summary
        api.StatusCode.Should().Be(HttpStatusCode.OK);
        retro.GetProperty("title").GetString().Should().Be("Retro Test");
        retro.GetProperty("lessonsLearned").GetProperty("totalPhaseChanges").GetInt32().Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Agent_Earns_First_Blood_Badge_On_First_Completion()
    {
        using var api = ApiClient.WithRico();

        // Complete a task (badges are awarded on completion)
        var task = await api.CreateTask("Badge Test");
        var taskId = task.GetProperty("id").GetInt32();

        await api.AdvancePhase(taskId, "Brainstorm");
        await api.AdvancePhase(taskId, "Triage");
        await api.AdvancePhase(taskId, "AuthorReview");
        await api.UpdateTask(taskId, new { ReadyToWorkChecked = true });
        await api.AdvancePhase(taskId, "ReadyToWork");
        await api.AssignTask(taskId);
        await api.AdvancePhase(taskId, "InProgress");
        await api.AdvancePhase(taskId, "Acceptance");
        await api.ApproveTask(taskId);

        // When I check my badges
        var badges = await api.GetMyBadges();

        // Then I should have badges (including First Blood for first completion)
        api.StatusCode.Should().Be(HttpStatusCode.OK);
        var badgeList = badges.EnumerateArray().ToList();
        badgeList.Should().Contain(b => b.GetProperty("name").GetString() == "First Blood",
            "agent should earn 'First Blood' badge on first task completion");
    }

    [Test]
    public async Task Agent_Can_View_Task_Transitions()
    {
        using var api = ApiClient.WithRico();

        // Create a task
        var task = await api.CreateTask("Transitions Test");
        var taskId = task.GetProperty("id").GetInt32();

        // Get available transitions
        var transitions = await api.GetTransitions(taskId);

        // Should show current phase and available transitions
        api.StatusCode.Should().Be(HttpStatusCode.OK);
        transitions.GetProperty("currentPhase").GetString().Should().Be("Research");
    }

    [Test]
    public async Task Agent_Can_Get_Board()
    {
        using var api = ApiClient.WithRico();

        var board = await api.GetBoard();

        api.StatusCode.Should().Be(HttpStatusCode.OK);
        board.GetProperty("name").GetString().Should().Be("Operations Board");
        board.GetProperty("columns").GetArrayLength().Should().Be(6);
    }
}
