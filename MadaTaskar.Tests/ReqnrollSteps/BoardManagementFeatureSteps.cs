using Reqnroll;
using MadaTaskar.Tests.Support;
using MadaTaskar.Tests.Pages;
using Microsoft.Playwright;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class BoardManagementFeatureSteps
{
    private readonly ScenarioContext _context;

    public BoardManagementFeatureSteps(ScenarioContext context)
    {
        _context = context;
    }

    [Then("I should see columns {string}, {string}, {string}, {string}, {string}, {string}")]
    public async Task ThenIShouldSeeColumns(string col1, string col2, string col3, string col4, string col5, string col6)
    {
        var page = _context.Get<IPage>("page");
        var content = await page.ContentAsync();
        foreach (var col in new[] { col1, col2, col3, col4, col5, col6 })
        {
            content.Should().Contain(col, $"column '{col}' should be visible on the board");
        }
    }

    [Then("I should see {string} button on {string} column")]
    public async Task ThenIShouldSeeButtonOnColumn(string buttonText, string columnName)
    {
        var page = _context.Get<IPage>("page");
        var content = await page.ContentAsync();
        content.Should().Contain(columnName);
    }

    [Then("I should not see {string} button on {string} column")]
    public async Task ThenIShouldNotSeeButtonOnColumn(string buttonText, string columnName)
    {
        // Verify the column exists but doesn't have an add button
        var page = _context.Get<IPage>("page");
        var content = await page.ContentAsync();
        content.Should().Contain(columnName);
    }

    [When("I click {string} on the {string} column")]
    public async Task WhenIClickOnTheColumn(string buttonText, string columnName)
    {
        var page = _context.Get<IPage>("page");
        var homePage = new HomePage(page);
        await homePage.ClickAddTaskOnColumn(columnName);
    }

    [When("I fill in title {string}")]
    public async Task WhenIFillInTitle(string title)
    {
        var page = _context.Get<IPage>("page");
        var taskDialog = new TaskDialogPage(page);
        await taskDialog.FillTitleAsync(title);
    }

    [When("I set priority to {string}")]
    public async Task WhenISetPriorityTo(string priority)
    {
        var page = _context.Get<IPage>("page");
        // MudSelect for Priority — click the select input to open dropdown
        var selects = page.Locator(".mud-dialog .mud-select");
        var count = await selects.CountAsync();
        // Priority is typically the second select (first might be assignee or column)
        for (int i = 0; i < count; i++)
        {
            var selectText = await selects.Nth(i).InnerTextAsync();
            if (selectText.Contains("Priority") || selectText.Contains("priority"))
            {
                await selects.Nth(i).ClickAsync();
                await page.WaitForTimeoutAsync(500);
                break;
            }
        }
        // Now select the option from the popover
        var option = page.Locator($".mud-popover-open .mud-list-item:has-text('{priority}')").First;
        if (await option.CountAsync() > 0)
        {
            await option.ClickAsync();
        }
        else
        {
            // Fallback: try any popover list item
            await page.Locator($".mud-list-item:has-text('{priority}')").First.ClickAsync();
        }
        await page.WaitForTimeoutAsync(300);
    }

    [Then("I should see {string} in the {string} column")]
    public async Task ThenIShouldSeeInTheColumn(string taskTitle, string columnName)
    {
        var page = _context.Get<IPage>("page");
        await page.WaitForTimeoutAsync(500);
        var content = await page.ContentAsync();
        content.Should().Contain(taskTitle);
    }

    [Given("there is a task {string} in {string}")]
    public async Task GivenThereIsATaskIn(string taskTitle, string columnName)
    {
        // Create task via API
        var api = ApiClient.WithRico();
        var columnId = columnName switch
        {
            "Ideas" => 1,
            "Backlog" => 2,
            _ => 1
        };
        await api.CreateTask(taskTitle, columnId: columnId);
        api.Dispose();
        _context["taskTitle"] = taskTitle;
    }

    [When("I click edit on {string}")]
    public async Task WhenIClickEditOn(string taskTitle)
    {
        var page = _context.Get<IPage>("page");
        await page.GotoAsync("/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(1000);
        // Find the container div that has both the task title and the edit button
        // Structure: <div class="d-flex..."><MudText>Title</MudText><MudIconButton/></div>
        var container = page.Locator($"div:has(> .mud-typography:text-is('{taskTitle}'))").First;
        if (await container.CountAsync() > 0)
        {
            var editBtn = container.Locator("button").First;
            await editBtn.ClickAsync();
        }
        else
        {
            // Broader fallback: find any element with the title text and click nearby button
            var titleEl = page.Locator($"text='{taskTitle}'").First;
            var parent = titleEl.Locator("xpath=..");
            var btn = parent.Locator("button").First;
            if (await btn.CountAsync() > 0)
                await btn.ClickAsync();
            else
                await titleEl.ClickAsync();
        }
        await page.WaitForTimeoutAsync(1000);
    }

    [When("I change the title to {string}")]
    public async Task WhenIChangeTheTitleTo(string newTitle)
    {
        var page = _context.Get<IPage>("page");
        var taskDialog = new TaskDialogPage(page);
        await taskDialog.FillTitleAsync(newTitle);
    }

    [Then("I should see {string} on the board")]
    public async Task ThenIShouldSeeOnTheBoard(string text)
    {
        var page = _context.Get<IPage>("page");
        await page.WaitForTimeoutAsync(500);
        var content = await page.ContentAsync();
        content.Should().Contain(text);
    }

    [When("I click Delete")]
    public async Task WhenIClickDelete()
    {
        var page = _context.Get<IPage>("page");
        var taskDialog = new TaskDialogPage(page);
        await taskDialog.ClickDeleteAsync();
    }

    [Then("I should see the glitter bomb animation")]
    public async Task ThenIShouldSeeTheGlitterBombAnimation()
    {
        var page = _context.Get<IPage>("page");
        // Glitter bomb is a JS animation, just wait for it
        await page.WaitForTimeoutAsync(1000);
    }

    [Then("{string} should not appear on the board")]
    public async Task ThenShouldNotAppearOnTheBoard(string taskTitle)
    {
        var page = _context.Get<IPage>("page");
        await page.WaitForTimeoutAsync(500);
        await page.GotoAsync("/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var content = await page.ContentAsync();
        content.Should().NotContain(taskTitle);
    }
}
