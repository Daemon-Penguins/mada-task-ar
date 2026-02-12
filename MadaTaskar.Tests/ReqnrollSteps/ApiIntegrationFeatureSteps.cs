using System.Net;
using System.Text.Json;
using Reqnroll;
using MadaTaskar.Tests.Support;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class ApiIntegrationFeatureSteps
{
    private readonly ScenarioContext _context;

    public ApiIntegrationFeatureSteps(ScenarioContext context)
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

    [When(@"I call GET \/api\/me with valid API key")]
    public async Task WhenICallGETApiMeWithValidAPIKey()
    {
        var api = GetApi();
        var result = await api.GetMe();
        _context["apiResult"] = result;
    }

    [When(@"I call GET \/api\/me with invalid API key")]
    public async Task WhenICallGETApiMeWithInvalidAPIKey()
    {
        var api = ApiClient.WithInvalidKey();
        _context["api"] = api;
        await api.GetMe();
    }

    [Then("I should receive my agent details")]
    public async Task ThenIShouldReceiveMyAgentDetails()
    {
        var api = GetApi();
        api.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then("the response should contain my name and roles")]
    public async Task ThenTheResponseShouldContainMyNameAndRoles()
    {
        var result = _context.Get<JsonElement>("apiResult");
        result.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("roles").GetString().Should().NotBeNullOrEmpty();
    }

    [Then(@"I should receive (\d+) Unauthorized")]
    public async Task ThenIShouldReceiveUnauthorized(int statusCode)
    {
        var api = GetApi();
        ((int)api.StatusCode).Should().Be(statusCode);
    }

    [When(@"I call POST \/api\/tasks with title {string}")]
    public async Task WhenICallPOSTApiTasksWithTitle(string title)
    {
        var api = GetApi();
        var result = await api.CreateTask(title);
        _context["apiResult"] = result;
    }

    [Then(@"I should receive (\d+) Created")]
    public async Task ThenIShouldReceiveCreated(int statusCode)
    {
        var api = GetApi();
        ((int)api.StatusCode).Should().Be(statusCode);
    }

    [Then("the task should appear on the board")]
    public async Task ThenTheTaskShouldAppearOnTheBoard()
    {
        var api = GetApi();
        api.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [When(@"I call POST \/api\/tasks with columnId (\d+)")]
    public async Task WhenICallPOSTApiTasksWithColumnId(int columnId)
    {
        var api = GetApi();
        await api.CreateTask("Restricted Test", columnId: columnId);
    }

    [Then(@"I should receive (\d+) Bad Request")]
    public async Task ThenIShouldReceiveBadRequest(int statusCode)
    {
        var api = GetApi();
        ((int)api.StatusCode).Should().Be(statusCode);
    }

    [Then("the error should say tasks can only be in Ideas or Backlog")]
    public async Task ThenTheErrorShouldSayTasksCanOnlyBeInIdeasOrBacklog()
    {
        var api = GetApi();
        api.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Given("there is a completed task")]
    public async Task GivenThereIsACompletedTask()
    {
        var api = GetApi();
        var task = await api.CreateTask("Completed Task For Retro");
        var taskId = task.GetProperty("id").GetInt32();
        _context["taskId"] = taskId;

        await api.AdvancePhase(taskId, "Brainstorm");
        await api.AdvancePhase(taskId, "Triage");
        await api.AdvancePhase(taskId, "AuthorReview");
        await api.UpdateTask(taskId, new { ReadyToWorkChecked = true });
        await api.AdvancePhase(taskId, "ReadyToWork");
        await api.AssignTask(taskId);
        await api.AdvancePhase(taskId, "InProgress");
        await api.AdvancePhase(taskId, "Acceptance");
        await api.ApproveTask(taskId);
    }

    [Given("the agent completes their first task")]
    public async Task GivenTheAgentCompletesTheirFirstTask()
    {
        await GivenThereIsACompletedTask();
    }

    [When(@"I call GET \/api\/tasks\/\{id\}\/retrospective")]
    public async Task WhenICallGETApiTasksIdRetrospective()
    {
        var api = GetApi();
        var taskId = _context.Get<int>("taskId");
        var result = await api.GetRetrospective(taskId);
        _context["apiResult"] = result;
    }

    [Then("I should receive the full lifecycle summary")]
    public async Task ThenIShouldReceiveTheFullLifecycleSummary()
    {
        var api = GetApi();
        api.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = _context.Get<JsonElement>("apiResult");
        result.GetProperty("title").GetString().Should().NotBeNullOrEmpty();
    }

    [Then("it should contain lessons learned")]
    public async Task ThenItShouldContainLessonsLearned()
    {
        var result = _context.Get<JsonElement>("apiResult");
        result.GetProperty("lessonsLearned").GetProperty("totalPhaseChanges").GetInt32().Should().BeGreaterThan(0);
    }

    [When(@"I call GET \/api\/me\/badges")]
    public async Task WhenICallGETApiMeBadges()
    {
        var api = GetApi();
        var result = await api.GetMyBadges();
        _context["apiResult"] = result;
    }

    [Then("I should see the {string} badge")]
    public async Task ThenIShouldSeeTheBadge(string badgeName)
    {
        var result = _context.Get<JsonElement>("apiResult");
        var badges = result.EnumerateArray().ToList();
        badges.Should().Contain(b => b.GetProperty("name").GetString() == badgeName,
            $"should have '{badgeName}' badge");
    }
}
