using FluentAssertions;
using MadaTaskar.Tests.Pages;
using MadaTaskar.Tests.Support;
using Microsoft.Playwright;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Tests for Board Management — viewing columns, creating/editing/deleting tasks.
/// </summary>
[TestFixture, Category("BoardManagement"), Category("UI")]
public class BoardManagementTests
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private LoginPage _loginPage = null!;
    private HomePage _homePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _context = await TestFixture.NewContextAsync();
        _page = await _context.NewPageAsync();
        _loginPage = new LoginPage(_page);
        _homePage = new HomePage(_page);
        await _loginPage.LoginAsync(TestData.DefaultUsername, TestData.DefaultPassword);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.CloseAsync();
    }

    [Test]
    public async Task Board_Shows_All_Six_Columns()
    {
        // When I am on the board page
        await _homePage.NavigateAsync();

        // Then I should see all 6 columns
        var content = await _page.ContentAsync();
        content.Should().Contain("Ideas");
        content.Should().Contain("Backlog");
        content.Should().Contain("In Progress");
        content.Should().Contain("Acceptance");
        content.Should().Contain("Done");
        content.Should().Contain("Rejected");
    }

    [Test]
    public async Task Board_Shows_Seeded_Tasks()
    {
        // When I am on the board page
        await _homePage.NavigateAsync();

        // Then I should see the seeded tasks
        var content = await _page.ContentAsync();
        content.Should().Contain("Welcome to Mada-TASK-ar!");
    }

    [Test]
    public async Task Board_Has_Add_Task_Buttons()
    {
        // When I am on the board page
        await _homePage.NavigateAsync();

        // Then there should be Add Task buttons visible
        var addButtons = _page.Locator("button:has-text('Add'), button:has-text('+'), button[title*='add' i]");
        var count = await addButtons.CountAsync();
        count.Should().BeGreaterThan(0, "board should have at least one Add Task button");
    }
}
