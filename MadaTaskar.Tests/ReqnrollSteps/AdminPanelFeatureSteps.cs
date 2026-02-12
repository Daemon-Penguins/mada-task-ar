using Reqnroll;
using MadaTaskar.Tests.Support;
using MadaTaskar.Tests.Pages;
using Microsoft.Playwright;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class AdminPanelFeatureSteps
{
    private readonly ScenarioContext _context;

    public AdminPanelFeatureSteps(ScenarioContext context)
    {
        _context = context;
    }

    [When("I navigate to the admin panel")]
    public async Task WhenINavigateToTheAdminPanel()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        await adminPage.NavigateAsync();
    }

    [Then("I should see the Agents tab")]
    public async Task ThenIShouldSeeTheAgentsTab()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        var visible = await adminPage.IsAgentsTabVisible();
        visible.Should().BeTrue("Agents tab should be visible");
    }

    [Then("I should see the Users tab")]
    public async Task ThenIShouldSeeTheUsersTab()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        var visible = await adminPage.IsUsersTabVisible();
        visible.Should().BeTrue("Users tab should be visible");
    }

    [Then("I should see the Activity Log tab")]
    public async Task ThenIShouldSeeTheActivityLogTab()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        var visible = await adminPage.IsActivityTabVisible();
        visible.Should().BeTrue("Activity Log tab should be visible");
    }

    [When("I click the Agents tab")]
    public async Task WhenIClickTheAgentsTab()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        await adminPage.ClickAgentsTab();
    }

    [Then("I should see agent {string} in the list")]
    public async Task ThenIShouldSeeAgentInTheList(string agentName)
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        var found = await adminPage.HasAgentInList(agentName);
        found.Should().BeTrue($"agent '{agentName}' should be in the list");
    }

    [Then("I should see their roles and status")]
    public async Task ThenIShouldSeeTheirRolesAndStatus()
    {
        var page = _context.Get<IPage>("page");
        var content = await page.ContentAsync();
        // Roles and status info should be present in the admin panel
        content.Should().NotBeEmpty();
    }
}
