using Microsoft.Playwright;

namespace MadaTaskar.Tests.Pages;

/// <summary>
/// Page Object for the Admin Panel (/admin).
/// Handles agent management, user management, and activity log tabs.
/// </summary>
public class AdminPage
{
    private readonly IPage _page;

    public AdminPage(IPage page) => _page = page;

    // Tabs
    private ILocator AgentsTab => _page.Locator("div.mud-tab:has-text('Agents'), button:has-text('Agents')").First;
    private ILocator UsersTab => _page.Locator("div.mud-tab:has-text('Users'), button:has-text('Users')").First;
    private ILocator ActivityTab => _page.Locator("div.mud-tab:has-text('Activity'), button:has-text('Activity')").First;

    // Buttons
    private ILocator AddAgentButton => _page.Locator("button:has-text('Add Agent'), button:has-text('Register')").First;
    private ILocator AddUserButton => _page.Locator("button:has-text('Add User')").First;

    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/admin", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await _page.WaitForTimeoutAsync(1000);
    }

    public async Task<bool> IsAgentsTabVisible() => await AgentsTab.IsVisibleAsync();
    public async Task<bool> IsUsersTabVisible() => await UsersTab.IsVisibleAsync();
    public async Task<bool> IsActivityTabVisible() => await ActivityTab.IsVisibleAsync();

    public async Task ClickAgentsTab()
    {
        await AgentsTab.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task ClickUsersTab()
    {
        await UsersTab.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task ClickActivityTab()
    {
        await ActivityTab.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<bool> HasAgentInList(string agentName)
    {
        var content = await _page.ContentAsync();
        return content.Contains(agentName);
    }

    public async Task<bool> HasUserInList(string username)
    {
        var content = await _page.ContentAsync();
        return content.Contains(username);
    }

    public async Task ClickAddAgent()
    {
        await AddAgentButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task ClickAddUser()
    {
        await AddUserButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task<List<IElementHandle>> GetAllButtons()
    {
        return (await _page.QuerySelectorAllAsync("button:not([disabled])")).ToList();
    }
}
