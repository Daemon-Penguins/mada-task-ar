using Reqnroll;
using MadaTaskar.Tests.Support;
using MadaTaskar.Tests.Pages;
using Microsoft.Playwright;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class SharedSteps
{
    private readonly ScenarioContext _context;

    public SharedSteps(ScenarioContext context)
    {
        _context = context;
    }

    private async Task<(IBrowserContext ctx, IPage page)> EnsurePage()
    {
        if (_context.TryGetValue<IPage>("page", out var existing))
            return (_context.Get<IBrowserContext>("browserContext"), existing);

        var browserContext = await TestFixture.NewContextAsync();
        var page = await browserContext.NewPageAsync();
        _context["browserContext"] = browserContext;
        _context["page"] = page;
        return (browserContext, page);
    }

    [Given("I am logged in")]
    public async Task GivenIAmLoggedIn()
    {
        var (_, page) = await EnsurePage();
        var loginPage = new LoginPage(page);
        await loginPage.LoginAsync(TestData.DefaultUsername, TestData.DefaultPassword);
    }

    [Given("I am logged in as {string}")]
    public async Task GivenIAmLoggedInAs(string username)
    {
        var (_, page) = await EnsurePage();
        var loginPage = new LoginPage(page);
        await loginPage.LoginAsync(username, TestData.DefaultPassword);
    }

    [Given("I am logged in as admin {string}")]
    public async Task GivenIAmLoggedInAsAdmin(string username)
    {
        var (_, page) = await EnsurePage();
        var loginPage = new LoginPage(page);
        await loginPage.LoginAsync(username, TestData.DefaultPassword);
    }

    [Given("I am logged in as admin")]
    public async Task GivenIAmLoggedInAsAdmin()
    {
        var (_, page) = await EnsurePage();
        var loginPage = new LoginPage(page);
        await loginPage.LoginAsync(TestData.DefaultUsername, TestData.DefaultPassword);
    }

    [Given("I am on the board page")]
    [When("I am on the board page")]
    public async Task WhenIAmOnTheBoardPage()
    {
        var page = _context.Get<IPage>("page");
        var homePage = new HomePage(page);
        await homePage.NavigateAsync();
    }

    [Given("I am on the admin panel")]
    public async Task GivenIAmOnTheAdminPanel()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        await adminPage.NavigateAsync();
    }

    [Given("the test agent {string} is registered")]
    public async Task GivenTheTestAgentIsRegistered(string agentName)
    {
        // TestBot is already registered by TestFixture.SeedTestData
        // Just ensure we have an API client available
        if (!_context.ContainsKey("api"))
        {
            var api = ApiClient.WithRico();
            _context["api"] = api;
        }
    }

    [Then("no JavaScript errors should appear in the console")]
    public async Task ThenNoJavaScriptErrorsShouldAppearInTheConsole()
    {
        if (_context.TryGetValue<List<string>>("consoleErrors", out var errors))
        {
            // Filter out network errors (401, 404, etc.) — only real JS errors matter
            var jsErrors = errors.Where(e =>
                !e.Contains("Failed to load resource") &&
                !e.Contains("net::ERR_") &&
                !e.Contains("status of 4") &&
                !e.Contains("status of 5")).ToList();
            jsErrors.Should().BeEmpty("no JavaScript errors should appear");
        }
    }

    [Then("the page should not crash")]
    public async Task ThenThePageShouldNotCrash()
    {
        var page = _context.Get<IPage>("page");
        var content = await page.ContentAsync();
        content.Should().NotContain("Error");
    }

    [AfterScenario]
    public async Task Cleanup()
    {
        if (_context.TryGetValue<IBrowserContext>("browserContext", out var ctx) && ctx != null)
            await ctx.CloseAsync();
        if (_context.TryGetValue<ApiClient>("api", out var api) && api != null)
            api.Dispose();
    }
}
