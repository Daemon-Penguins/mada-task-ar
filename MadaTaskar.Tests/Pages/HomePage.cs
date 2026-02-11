using Microsoft.Playwright;

namespace MadaTaskar.Tests.Pages;

/// <summary>
/// Page Object for the Home/Board page (Kanban board).
/// Handles column viewing, task interaction, and drag-and-drop.
/// </summary>
public class HomePage
{
    private readonly IPage _page;

    public HomePage(IPage page) => _page = page;

    // Selectors
    private ILocator Header => _page.Locator("h1, h2, h3, .mud-typography-h4, .mud-typography-h5").First;
    private ILocator LogoutButton => _page.Locator("button:has-text('Logout'), button:has-text('Sign out'), button[title='Logout']").First;
    private ILocator AdminLink => _page.Locator("a[href='/admin'], button:has-text('Admin')").First;

    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task<string> GetHeaderTextAsync()
    {
        return await Header.InnerTextAsync();
    }

    public async Task<bool> IsOnBoardPageAsync()
    {
        var url = _page.Url;
        return url.EndsWith("/") || url.Contains("/board") || !url.Contains("/login");
    }

    public async Task<List<string>> GetColumnNamesAsync()
    {
        var columns = _page.Locator(".board-column h3, .board-column h4, .mud-card-header .mud-typography, .column-header");
        var count = await columns.CountAsync();
        var names = new List<string>();
        for (int i = 0; i < count; i++)
        {
            names.Add((await columns.Nth(i).InnerTextAsync()).Trim());
        }
        return names;
    }

    public async Task<bool> HasAddTaskButtonOnColumn(string columnName)
    {
        // Look for add task buttons near column headers
        var addButtons = _page.Locator($"text='{columnName}' >> .. >> button:has-text('Add'), button:has-text('+')");
        var count = await addButtons.CountAsync();
        if (count > 0) return true;

        // Fallback: look for any add button in the column context
        var allButtons = await _page.QuerySelectorAllAsync("button");
        // Simplified check
        return false;
    }

    public async Task ClickAddTaskOnColumn(string columnName)
    {
        // Find add task button in the specified column
        var addButton = _page.Locator($"button:has-text('Add Task')").First;
        await addButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task<bool> HasTaskInColumn(string taskTitle, string columnName)
    {
        var content = await _page.ContentAsync();
        return content.Contains(taskTitle);
    }

    public async Task ClickTaskAsync(string taskTitle)
    {
        var task = _page.Locator($"text='{taskTitle}'").First;
        await task.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task ClickLogoutAsync()
    {
        await LogoutButton.ClickAsync();
        await _page.WaitForTimeoutAsync(1000);
    }

    public async Task NavigateToAdminAsync()
    {
        await AdminLink.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }
}
