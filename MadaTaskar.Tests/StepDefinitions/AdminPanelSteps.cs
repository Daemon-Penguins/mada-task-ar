using FluentAssertions;
using MadaTaskar.Tests.Pages;
using MadaTaskar.Tests.Support;
using Microsoft.Playwright;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Tests for the Admin Panel — tab navigation and content verification.
/// </summary>
[TestFixture, Category("AdminPanel"), Category("UI")]
public class AdminPanelTests
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private LoginPage _loginPage = null!;
    private AdminPage _adminPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _context = await TestFixture.NewContextAsync();
        _page = await _context.NewPageAsync();
        _loginPage = new LoginPage(_page);
        _adminPage = new AdminPage(_page);

        // Login first
        await _loginPage.LoginAsync(TestData.DefaultUsername, TestData.DefaultPassword);
    }

    [TearDown]
    public async Task TearDown()
    {
        await TestFixture.SaveTestArtifactsAsync(_context, _page, TestContext.CurrentContext.Test.Name);
        if (_context != null) await _context.CloseAsync();
    }

    [Test]
    public async Task Admin_Can_Access_Admin_Panel_With_All_Tabs()
    {
        // When I navigate to the admin panel
        await _adminPage.NavigateAsync();

        // Then I should see all three tabs
        (await _adminPage.IsAgentsTabVisible()).Should().BeTrue("Agents tab should be visible");
        (await _adminPage.IsUsersTabVisible()).Should().BeTrue("Users tab should be visible");
        (await _adminPage.IsActivityTabVisible()).Should().BeTrue("Activity Log tab should be visible");
    }

    [Test]
    public async Task Admin_Can_View_Registered_Agents()
    {
        // Given I am on the admin panel
        await _adminPage.NavigateAsync();

        // When I click the Agents tab
        await _adminPage.ClickAgentsTab();

        // Then I should see agent "Rico" in the list
        (await _adminPage.HasAgentInList("Rico")).Should().BeTrue("Rico should be listed as a registered agent");
    }
}
