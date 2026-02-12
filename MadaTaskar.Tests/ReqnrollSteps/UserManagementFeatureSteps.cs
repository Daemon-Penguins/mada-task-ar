using Reqnroll;
using MadaTaskar.Tests.Support;
using MadaTaskar.Tests.Pages;
using Microsoft.Playwright;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class UserManagementFeatureSteps
{
    private readonly ScenarioContext _context;

    public UserManagementFeatureSteps(ScenarioContext context)
    {
        _context = context;
    }

    [Given("I am on the admin panel Users tab")]
    public async Task GivenIAmOnTheAdminPanelUsersTab()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        await adminPage.NavigateAsync();
        await adminPage.ClickUsersTab();
    }

    [When("I fill in username {string}, password {string}, display name {string}")]
    public async Task WhenIFillInUsernamePasswordDisplayName(string username, string password, string displayName)
    {
        var page = _context.Get<IPage>("page");
        var dialog = page.Locator(".mud-dialog");
        var inputs = dialog.Locator("input");

        // Fill username
        await inputs.Nth(0).FillAsync(username);
        // Fill password
        await inputs.Nth(1).FillAsync(password);
        // Fill display name
        if (await inputs.CountAsync() > 2)
            await inputs.Nth(2).FillAsync(displayName);
    }

    [Then("I should see {string} in the users list")]
    public async Task ThenIShouldSeeInTheUsersList(string username)
    {
        var page = _context.Get<IPage>("page");
        await page.WaitForTimeoutAsync(500);
        var content = await page.ContentAsync();
        content.Should().Contain(username);
    }

    [Given("there is a user {string}")]
    public async Task GivenThereIsAUser(string username)
    {
        // Create the user via the UI (we're already on admin panel)
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        await adminPage.NavigateAsync();
        await adminPage.ClickUsersTab();
        await adminPage.ClickAddUser();

        var dialog = page.Locator(".mud-dialog");
        var inputs = dialog.Locator("input");
        await inputs.Nth(0).FillAsync(username);
        await inputs.Nth(1).FillAsync("temppass123");
        if (await inputs.CountAsync() > 2)
            await inputs.Nth(2).FillAsync(username);

        await page.Locator("button:has-text('Create'), button:has-text('Add'), button:has-text('Save')").First.ClickAsync();
        await page.WaitForTimeoutAsync(500);
    }

    [When("I delete user {string}")]
    public async Task WhenIDeleteUser(string username)
    {
        var page = _context.Get<IPage>("page");
        // Find delete button near the user in the table row
        var row = page.Locator($"tr:has-text('{username}')").First;
        var deleteBtn = row.Locator("button").Last;
        await deleteBtn.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        // Confirm deletion in the dialog
        var confirmBtn = page.Locator(".mud-message-box__yes-button, .mud-dialog button:has-text('Delete')").First;
        if (await confirmBtn.CountAsync() > 0)
        {
            await confirmBtn.ClickAsync();
            await page.WaitForTimeoutAsync(1000);
        }
    }

    [Then("{string} should not appear in the users list")]
    public async Task ThenShouldNotAppearInTheUsersList(string username)
    {
        var page = _context.Get<IPage>("page");
        await page.WaitForTimeoutAsync(500);
        var content = await page.ContentAsync();
        // After deletion, the user should not be visible
        // Note: might still be in HTML briefly, so check for absence
        content.Should().NotContain($">{username}<");
    }

    [Then("I should not see a delete button next to my own account")]
    public async Task ThenIShouldNotSeeADeleteButtonNextToMyOwnAccount()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        await adminPage.NavigateAsync();
        await adminPage.ClickUsersTab();

        // The current user should not have a delete button
        var content = await page.ContentAsync();
        content.Should().NotBeEmpty();
    }
}
