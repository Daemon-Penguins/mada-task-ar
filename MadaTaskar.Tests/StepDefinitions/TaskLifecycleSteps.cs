using System.Net;
using System.Text.Json;
using FluentAssertions;
using MadaTaskar.Tests.Support;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Tests for the Task Lifecycle Pipeline — phase transitions, validation, and acceptance criteria.
/// Exercises the full Research → Brainstorm → Triage → AuthorReview → ReadyToWork → InProgress → Acceptance → Completed flow.
/// </summary>
[TestFixture, Category("TaskLifecycle"), Category("API")]
public class TaskLifecycleTests
{
    private ApiClient _api = null!;

    [SetUp]
    public void SetUp()
    {
        _api = ApiClient.WithRico();
    }

    [TearDown]
    public void TearDown()
    {
        _api.Dispose();
    }

    [Test]
    public async Task New_Task_Starts_In_Research_Phase_And_Backlog_Column()
    {
        // Given I create a task via API
        var task = await _api.CreateTask("Pipeline Test");
        _api.StatusCode.Should().Be(HttpStatusCode.Created);

        // Then the task should be in Research phase
        task.GetProperty("phase").GetString().Should().Be("Research");

        // And in the Backlog column (columnId 2 is default for new API tasks)
        task.GetProperty("columnId").GetInt32().Should().Be(TestData.BacklogColumnId);
    }

    [Test]
    public async Task Full_Task_Lifecycle_From_Research_To_Completed()
    {
        // Create a task
        var task = await _api.CreateTask("Full Lifecycle Test", assignToSelf: false);
        _api.StatusCode.Should().Be(HttpStatusCode.Created);
        var taskId = task.GetProperty("id").GetInt32();

        // Add research reference
        await _api.AddResearch(taskId, "https://example.com", "Reference Doc", "Useful info");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Advance: Research → Brainstorm
        var result = await _api.AdvancePhase(taskId, "Brainstorm");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);
        result.GetProperty("to").GetString().Should().Be("Brainstorm");

        // Add proposal
        await _api.AddProposal(taskId, "Let's use approach A");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Advance: Brainstorm → Triage
        result = await _api.AdvancePhase(taskId, "Triage");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Advance: Triage → AuthorReview
        result = await _api.AdvancePhase(taskId, "AuthorReview");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Set ReadyToWork flag via task update
        await _api.UpdateTask(taskId, new { ReadyToWorkChecked = true });

        // Advance: AuthorReview → ReadyToWork (Rico is the author since he created the task)
        result = await _api.AdvancePhase(taskId, "ReadyToWork");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assign task to self
        await _api.AssignTask(taskId);
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Advance: ReadyToWork → InProgress
        result = await _api.AdvancePhase(taskId, "InProgress");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Advance: InProgress → Acceptance
        result = await _api.AdvancePhase(taskId, "Acceptance");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);

        // Approve (complete) the task
        result = await _api.ApproveTask(taskId, "Looks good!");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);
        result.GetProperty("phase").GetString().Should().Be("Completed");
        result.GetProperty("columnId").GetInt32().Should().Be(TestData.DoneColumnId);
    }

    [Test]
    public async Task Invalid_Phase_Transition_Returns_Error_400()
    {
        // Create a task (starts in Research phase)
        var task = await _api.CreateTask("Invalid Transition Test");
        var taskId = task.GetProperty("id").GetInt32();

        // Try to jump directly to InProgress (invalid: must go Research → Brainstorm → ...)
        await _api.AdvancePhase(taskId, "InProgress");

        // Should get 400
        _api.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Auto_Accept_Fails_When_Criteria_Not_Met()
    {
        // Create a task
        var task = await _api.CreateTask("Criteria Test");
        var taskId = task.GetProperty("id").GetInt32();

        // Add acceptance criteria (not yet met)
        var criterion = await _api.AddCriterion(taskId, "All tests pass");
        _api.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try auto-accept
        await _api.AutoAccept(taskId);

        // Should fail because criteria aren't met
        _api.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Task_Can_Be_Killed_From_Any_Phase()
    {
        // Create a task
        var task = await _api.CreateTask("Kill Test");
        var taskId = task.GetProperty("id").GetInt32();

        // Kill from Research phase (Rico is author)
        var result = await _api.AdvancePhase(taskId, "Killed");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);
        result.GetProperty("to").GetString().Should().Be("Killed");
        result.GetProperty("columnId").GetInt32().Should().Be(TestData.RejectedColumnId);
    }

    [Test]
    public async Task Acceptance_Can_Send_Task_Back_To_InProgress()
    {
        // Create and advance task to Acceptance
        var task = await _api.CreateTask("Bounce Test");
        var taskId = task.GetProperty("id").GetInt32();

        await _api.AdvancePhase(taskId, "Brainstorm");
        await _api.AdvancePhase(taskId, "Triage");
        await _api.AdvancePhase(taskId, "AuthorReview");
        await _api.UpdateTask(taskId, new { ReadyToWorkChecked = true });
        await _api.AdvancePhase(taskId, "ReadyToWork");
        await _api.AssignTask(taskId);
        await _api.AdvancePhase(taskId, "InProgress");
        await _api.AdvancePhase(taskId, "Acceptance");

        // Send back to InProgress (request changes)
        var result = await _api.AdvancePhase(taskId, "InProgress");
        _api.StatusCode.Should().Be(HttpStatusCode.OK);
        result.GetProperty("to").GetString().Should().Be("InProgress");
    }
}
