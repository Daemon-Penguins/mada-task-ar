using Reqnroll;
using MadaTaskar.Tests.Support;
using MadaTaskar.Tests.Pages;
using Microsoft.Playwright;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class MonkeyTestFeatureSteps
{
    private readonly ScenarioContext _context;

    public MonkeyTestFeatureSteps(ScenarioContext context)
    {
        _context = context;
    }

    private void SetupConsoleListener(IPage page)
    {
        var errors = new List<string>();
        _context["consoleErrors"] = errors;
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                errors.Add(msg.Text);
        };
    }

    [When("I click every non-disabled button on the page")]
    public async Task WhenIClickEveryNonDisabledButtonOnThePage()
    {
        var page = _context.Get<IPage>("page");
        SetupConsoleListener(page);

        var buttons = await page.QuerySelectorAllAsync("button:not([disabled])");
        foreach (var button in buttons)
        {
            try
            {
                if (await button.IsVisibleAsync())
                {
                    await button.ClickAsync(new() { Timeout = 2000 });
                    await page.WaitForTimeoutAsync(200);
                }
            }
            catch { /* continue on click failures */ }
        }
    }

    [When("I click every non-disabled button including in dialogs")]
    public async Task WhenIClickEveryNonDisabledButtonIncludingInDialogs()
    {
        var page = _context.Get<IPage>("page");
        SetupConsoleListener(page);

        for (int pass = 0; pass < 2; pass++)
        {
            var buttons = await page.QuerySelectorAllAsync("button:not([disabled])");
            foreach (var button in buttons)
            {
                try
                {
                    if (await button.IsVisibleAsync())
                    {
                        await button.ClickAsync(new() { Timeout = 2000 });
                        await page.WaitForTimeoutAsync(200);
                    }
                }
                catch { /* continue */ }
            }
        }
    }

    [Given("I open a task dialog")]
    public async Task GivenIOpenATaskDialog()
    {
        var page = _context.Get<IPage>("page");
        var homePage = new HomePage(page);
        await homePage.NavigateAsync();
        await homePage.ClickAddTaskOnColumn("Ideas");
    }

    [When("I click every non-disabled interactive element")]
    public async Task WhenIClickEveryNonDisabledInteractiveElement()
    {
        var page = _context.Get<IPage>("page");
        SetupConsoleListener(page);

        var dialog = page.Locator(".mud-dialog");
        if (await dialog.CountAsync() > 0)
        {
            var taskDialog = new TaskDialogPage(page);
            var elements = await taskDialog.GetAllInteractiveElements();
            foreach (var element in elements)
            {
                try
                {
                    if (await element.IsVisibleAsync())
                    {
                        await element.ClickAsync(new() { Timeout = 2000 });
                        await page.WaitForTimeoutAsync(200);
                    }
                }
                catch { /* continue */ }
            }
        }
    }

    [When("I check all expandable panels")]
    public async Task WhenICheckAllExpandablePanels()
    {
        var page = _context.Get<IPage>("page");
        var panels = page.Locator(".mud-expand-panel-header");
        var count = await panels.CountAsync();
        for (int i = 0; i < count; i++)
        {
            try
            {
                await panels.Nth(i).ClickAsync();
                await page.WaitForTimeoutAsync(200);
            }
            catch { /* continue */ }
        }
    }

    [When("I rapidly create {int} tasks")]
    public async Task WhenIRapidlyCreateTasks(int count)
    {
        var api = ApiClient.WithRico();
        _context["api"] = api;
        var taskIds = new List<int>();

        for (int i = 0; i < count; i++)
        {
            var task = await api.CreateTask($"Rapid Task {i + 1}");
            if (api.StatusCode == System.Net.HttpStatusCode.Created)
            {
                taskIds.Add(task.GetProperty("id").GetInt32());
            }
        }
        _context["rapidTaskIds"] = taskIds;
    }

    [When("I rapidly move them between columns")]
    public async Task WhenIRapidlyMoveThemBetweenColumns()
    {
        var api = _context.Get<ApiClient>("api");
        var taskIds = _context.Get<List<int>>("rapidTaskIds");

        foreach (var taskId in taskIds)
        {
            try
            {
                await api.UpdateTask(taskId, new { ColumnId = 1 });
                await api.UpdateTask(taskId, new { ColumnId = 2 });
            }
            catch { /* continue */ }
        }
    }

    [Then("the board should remain stable")]
    public async Task ThenTheBoardShouldRemainStable()
    {
        var api = _context.Get<ApiClient>("api");
        var board = await api.GetBoard();
        api.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Then("no console errors should appear")]
    public async Task ThenNoConsoleErrorsShouldAppear()
    {
        if (_context.TryGetValue<List<string>>("consoleErrors", out var errors))
        {
            errors.Should().BeEmpty();
        }
    }
}
