using FluentAssertions;
using MadaTaskar.Tests.Pages;
using MadaTaskar.Tests.Support;
using Microsoft.Playwright;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Monkey Tests — click every button, stress test the UI, find crashes.
/// These tests simulate chaotic user behavior to uncover unexpected errors.
/// </summary>
[TestFixture, Category("MonkeyTest"), Category("UI")]
public class MonkeyTests
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private LoginPage _loginPage = null!;
    private readonly List<string> _consoleErrors = new();

    [SetUp]
    public async Task SetUp()
    {
        _context = await TestFixture.NewContextAsync();
        _page = await _context.NewPageAsync();
        _loginPage = new LoginPage(_page);
        _consoleErrors.Clear();

        // Capture all console errors
        _page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                _consoleErrors.Add(msg.Text);
        };

        // Login
        await _loginPage.LoginAsync(TestData.DefaultUsername, TestData.DefaultPassword);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_context != null)
            await _context.CloseAsync();
    }

    [Test]
    public async Task Click_All_Buttons_On_Board_Page_Without_Crashes()
    {
        // Given I am on the board page
        await _page.GotoAsync("/");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _page.WaitForTimeoutAsync(1000);

        // When I click every non-disabled button
        await ClickAllVisibleButtons();

        // Then no JavaScript errors should appear
        _consoleErrors.Should().BeEmpty(
            $"Console errors found during monkey test: {string.Join("\n", _consoleErrors)}");

        // And the page should not crash (still responsive)
        var content = await _page.ContentAsync();
        content.Should().NotBeNullOrEmpty("page should still have content after clicking all buttons");
    }

    [Test]
    public async Task Click_All_Buttons_In_Admin_Panel_Without_Crashes()
    {
        // Given I am on the admin panel
        await _page.GotoAsync("/admin");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _page.WaitForTimeoutAsync(1000);

        // When I click every non-disabled button including in dialogs
        await ClickAllVisibleButtons();

        // Also try closing any dialogs that appeared
        await CloseAllDialogs();

        // Then no errors
        _consoleErrors.Should().BeEmpty(
            $"Console errors found in admin panel monkey test: {string.Join("\n", _consoleErrors)}");
    }

    [Test]
    public async Task Rapid_Task_Creation_Stress_Test()
    {
        // Given I am logged in
        using var api = ApiClient.WithRico();

        // When I rapidly create 10 tasks
        var tasks = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            var task = await api.CreateTask($"Stress Test Task {i}");
            if (api.StatusCode == System.Net.HttpStatusCode.Created)
            {
                tasks.Add(task.GetProperty("id").GetInt32());
            }
        }

        // Then all tasks should have been created
        tasks.Count.Should().Be(10, "all 10 rapid tasks should be created successfully");

        // And the board should still work
        var board = await api.GetBoard();
        api.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Test]
    public async Task Click_All_Buttons_In_Task_Dialog_Without_Crashes()
    {
        // Navigate to board
        await _page.GotoAsync("/");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _page.WaitForTimeoutAsync(1000);

        // Try to open a task dialog by clicking on a task card
        var taskCards = await _page.QuerySelectorAllAsync(".mud-card, .task-card, [class*='task']");
        if (taskCards.Count > 0)
        {
            try
            {
                await taskCards[0].ClickAsync();
                await _page.WaitForTimeoutAsync(500);
            }
            catch { /* task card might not be clickable */ }
        }

        // Click all interactive elements in any open dialog
        var dialogButtons = await _page.QuerySelectorAllAsync(".mud-dialog button:not([disabled]), .mud-dialog [role='button']:not([aria-disabled='true'])");
        foreach (var button in dialogButtons)
        {
            try
            {
                if (await button.IsVisibleAsync())
                {
                    await button.ClickAsync();
                    await _page.WaitForTimeoutAsync(300);
                }
            }
            catch { /* element may have been removed */ }
        }

        // Check expandable panels
        var expanders = await _page.QuerySelectorAllAsync(".mud-expand-panel-header, [role='button'][aria-expanded]");
        foreach (var expander in expanders)
        {
            try
            {
                if (await expander.IsVisibleAsync())
                {
                    await expander.ClickAsync();
                    await _page.WaitForTimeoutAsync(300);
                }
            }
            catch { /* continue */ }
        }

        // Then no errors
        _consoleErrors.Should().BeEmpty(
            $"Console errors found in task dialog monkey test: {string.Join("\n", _consoleErrors)}");
    }

    // ── Helpers ───────────────────────────────────────────────

    private async Task ClickAllVisibleButtons()
    {
        var buttons = await _page.QuerySelectorAllAsync(
            "button:not([disabled]), .mud-button-root:not(.mud-disabled), [role='button']:not([aria-disabled='true'])");

        foreach (var button in buttons)
        {
            try
            {
                if (await button.IsVisibleAsync())
                {
                    await button.ClickAsync();
                    await _page.WaitForTimeoutAsync(300);

                    // Close any dialog that may have popped up
                    await CloseAllDialogs();
                }
            }
            catch
            {
                // Element may have been removed from DOM — continue
            }
        }
    }

    private async Task CloseAllDialogs()
    {
        var closeButtons = await _page.QuerySelectorAllAsync(
            ".mud-dialog button.mud-dialog-close, .mud-dialog button:has-text('Close'), .mud-dialog button:has-text('Cancel'), .mud-overlay");

        foreach (var btn in closeButtons)
        {
            try
            {
                if (await btn.IsVisibleAsync())
                {
                    await btn.ClickAsync();
                    await _page.WaitForTimeoutAsync(200);
                }
            }
            catch { /* continue */ }
        }
    }
}
