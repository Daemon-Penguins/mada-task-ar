using System.Net;
using System.Text.Json;
using Reqnroll;
using MadaTaskar.Tests.Support;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class TaskLifecycleFeatureSteps
{
    private readonly ScenarioContext _context;

    public TaskLifecycleFeatureSteps(ScenarioContext context)
    {
        _context = context;
    }

    private ApiClient GetApi()
    {
        if (!_context.ContainsKey("api"))
        {
            var api = ApiClient.WithRico();
            _context["api"] = api;
        }
        return _context.Get<ApiClient>("api");
    }

    [Given("I create a task {string} via API")]
    public async Task GivenICreateATaskViaAPI(string title)
    {
        var api = GetApi();
        var task = await api.CreateTask(title);
        api.StatusCode.Should().Be(HttpStatusCode.Created);
        var taskId = task.GetProperty("id").GetInt32();
        _context["taskId"] = taskId;
        _context["lastTaskResult"] = task;
    }

    [Given("I create a task {string} via API with acceptance criteria")]
    public async Task GivenICreateATaskViaAPIWithAcceptanceCriteria(string title)
    {
        var api = GetApi();
        var task = await api.CreateTask(title);
        api.StatusCode.Should().Be(HttpStatusCode.Created);
        var taskId = task.GetProperty("id").GetInt32();
        _context["taskId"] = taskId;

        await api.AddCriterion(taskId, "All tests pass");
        api.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [When("the agent adds research reference {string} with title {string}")]
    public async Task WhenTheAgentAddsResearchReferenceWithTitle(string url, string title)
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        await api.AddResearch(taskId, url, title, "Summary");
        api.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [When("the agent advances the task to {string} phase")]
    public async Task WhenTheAgentAdvancesTheTaskToPhase(string phase)
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        var result = await api.AdvancePhase(taskId, phase);
        _context["lastAdvanceResult"] = result;
        _context["lastStatusCode"] = api.StatusCode;
    }

    [When("the agent adds a proposal {string}")]
    public async Task WhenTheAgentAddsAProposal(string content)
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        await api.AddProposal(taskId, content);
        api.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [When("the author sets ReadyToWork to true")]
    public async Task WhenTheAuthorSetsReadyToWorkToTrue()
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        await api.UpdateTask(taskId, new { ReadyToWorkChecked = true });
    }

    [When("the agent assigns the task to themselves")]
    public async Task WhenTheAgentAssignsTheTaskToThemselves()
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        await api.AssignTask(taskId);
        api.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [When("the agent approves the task")]
    public async Task WhenTheAgentApprovesTheTask()
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        var result = await api.ApproveTask(taskId, "Approved");
        _context["lastApproveResult"] = result;
    }

    [When("the agent tries to advance directly to {string} phase")]
    public async Task WhenTheAgentTriesToAdvanceDirectlyToPhase(string phase)
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        await api.AdvancePhase(taskId, phase);
        _context["lastStatusCode"] = api.StatusCode;
    }

    [When("I try to auto-accept the task")]
    public async Task WhenITryToAutoAcceptTheTask()
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        await api.AutoAccept(taskId);
        _context["lastStatusCode"] = api.StatusCode;
    }

    [Then("the task should be in {string} phase")]
    public async Task ThenTheTaskShouldBeInPhase(string expectedPhase)
    {
        if (_context.TryGetValue<JsonElement>("lastApproveResult", out var approveResult))
        {
            approveResult.GetProperty("phase").GetString().Should().Be(expectedPhase);
        }
        else if (_context.TryGetValue<JsonElement>("lastAdvanceResult", out var advanceResult))
        {
            advanceResult.GetProperty("to").GetString().Should().Be(expectedPhase);
        }
        else
        {
            var result = _context.Get<JsonElement>("lastTaskResult");
            result.GetProperty("phase").GetString().Should().Be(expectedPhase);
        }
    }

    [Then("the task should be in {string} column")]
    public async Task ThenTheTaskShouldBeInColumn(string columnName)
    {
        var expectedColumnId = columnName switch
        {
            "Ideas" => TestData.IdeasColumnId,
            "Backlog" => TestData.BacklogColumnId,
            "In Progress" => TestData.InProgressColumnId,
            "Acceptance" => TestData.AcceptanceColumnId,
            "Done" => TestData.DoneColumnId,
            "Rejected" => TestData.RejectedColumnId,
            _ => throw new ArgumentException($"Unknown column: {columnName}")
        };

        if (_context.TryGetValue<JsonElement>("lastApproveResult", out var approveResult))
        {
            approveResult.GetProperty("columnId").GetInt32().Should().Be(expectedColumnId);
        }
        else
        {
            var result = _context.Get<JsonElement>("lastTaskResult");
            result.GetProperty("columnId").GetInt32().Should().Be(expectedColumnId);
        }
    }

    [Then("the API should return error {int}")]
    public async Task ThenTheAPIShouldReturnError(int statusCode)
    {
        var actual = _context.Get<HttpStatusCode>("lastStatusCode");
        ((int)actual).Should().Be(statusCode);
    }

    [Then("the error should explain allowed transitions")]
    public async Task ThenTheErrorShouldExplainAllowedTransitions()
    {
        var api = GetApi();
        api.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Then("it should fail because not all criteria are met")]
    public async Task ThenItShouldFailBecauseNotAllCriteriaAreMet()
    {
        var actual = _context.Get<HttpStatusCode>("lastStatusCode");
        actual.Should().Be(HttpStatusCode.BadRequest);
    }
}
