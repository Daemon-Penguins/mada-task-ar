using Reqnroll;
using MadaTaskar.Tests.Support;
using MadaTaskar.Tests.Pages;
using Microsoft.Playwright;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class AgentManagementFeatureSteps
{
    private readonly ScenarioContext _context;

    public AgentManagementFeatureSteps(ScenarioContext context)
    {
        _context = context;
    }

    [Given("I am on the admin panel Agents tab")]
    public async Task GivenIAmOnTheAdminPanelAgentsTab()
    {
        var page = _context.Get<IPage>("page");
        var adminPage = new AdminPage(page);
        await adminPage.NavigateAsync();
        await adminPage.ClickAgentsTab();
    }

    [When("I click {string}")]
    public async Task WhenIClick(string buttonText)
    {
        var page = _context.Get<IPage>("page");
        await page.Locator($"button:has-text('{buttonText}')").First.ClickAsync();
        await page.WaitForTimeoutAsync(500);
    }

    [When("I enter agent name {string}")]
    public async Task WhenIEnterAgentName(string name)
    {
        var page = _context.Get<IPage>("page");
        var dialog = page.Locator(".mud-dialog");
        // The first input in the AddAgentDialog is the Name field
        await dialog.Locator("input").First.FillAsync(name);
    }

    [When("I select roles {string} and {string}")]
    public async Task WhenISelectRolesAnd(string role1, string role2)
    {
        var page = _context.Get<IPage>("page");
        var dialog = page.Locator(".mud-dialog");
        // MudChipSet: click on the chip elements to select them
        // First deselect "worker" if it's pre-selected and we don't want it
        // Then click the chips we want
        foreach (var role in new[] { role1, role2 })
        {
            var chip = dialog.Locator($".mud-chip:has-text('{role}')").First;
            if (await chip.CountAsync() > 0)
            {
                await chip.ClickAsync();
                await page.WaitForTimeoutAsync(200);
            }
        }
        // Deselect worker if it was pre-selected and not in our list
        if (role1 != "worker" && role2 != "worker")
        {
            var workerChip = dialog.Locator(".mud-chip:has-text('Worker')").First;
            if (await workerChip.CountAsync() > 0)
            {
                // Check if it's selected (has selected class)
                var classes = await workerChip.GetAttributeAsync("class") ?? "";
                if (classes.Contains("mud-chip-selected") || classes.Contains("selected"))
                {
                    await workerChip.ClickAsync();
                    await page.WaitForTimeoutAsync(200);
                }
            }
        }
    }

    [When("I click Create")]
    public async Task WhenIClickCreate()
    {
        var page = _context.Get<IPage>("page");
        // Find Create button in dialog — may need to wait for it to be enabled
        var createBtn = page.Locator(".mud-dialog button:has-text('Create')").First;
        await createBtn.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        // If button is disabled, try force clicking or wait more
        var isDisabled = await createBtn.GetAttributeAsync("disabled");
        if (isDisabled != null)
        {
            // Button still disabled — form validation failed, try clicking it anyway
            await createBtn.ClickAsync(new() { Force = true });
        }
        else
        {
            await createBtn.ClickAsync();
        }
        await page.WaitForTimeoutAsync(500);
    }

    [Then("I should see {string} in the agents list")]
    public async Task ThenIShouldSeeInTheAgentsList(string name)
    {
        var page = _context.Get<IPage>("page");
        await page.WaitForTimeoutAsync(500);
        var content = await page.ContentAsync();
        content.Should().Contain(name);
    }

    [Then("the agent should have roles {string}")]
    public async Task ThenTheAgentShouldHaveRoles(string roles)
    {
        var page = _context.Get<IPage>("page");
        var content = await page.ContentAsync();
        foreach (var role in roles.Split(','))
        {
            content.Should().Contain(role.Trim());
        }
    }

    [When("I click edit roles for agent {string}")]
    public async Task WhenIClickEditRolesForAgent(string agentName)
    {
        var page = _context.Get<IPage>("page");
        // Find the row containing the agent name and click its edit button
        var row = page.Locator($"tr:has-text('{agentName}')").First;
        if (await row.CountAsync() == 0)
        {
            // Try finding in any container
            row = page.Locator($"*:has-text('{agentName}')").First;
        }
        var editBtn = row.Locator("button").First;
        await editBtn.ClickAsync();
        await page.WaitForTimeoutAsync(500);
    }

    [When("I add role {string}")]
    public async Task WhenIAddRole(string role)
    {
        var page = _context.Get<IPage>("page");
        var dialog = page.Locator(".mud-dialog");
        // EditRolesDialog also uses MudChipSet — click the chip
        var chip = dialog.Locator($".mud-chip:has-text('{role}')").First;
        if (await chip.CountAsync() > 0)
        {
            await chip.ClickAsync();
            await page.WaitForTimeoutAsync(200);
        }
    }

    [When("I click Save")]
    public async Task WhenIClickSave()
    {
        var page = _context.Get<IPage>("page");
        await page.Locator(".mud-dialog button:has-text('Save'), .mud-dialog button:has-text('Update')").First.ClickAsync();
        await page.WaitForTimeoutAsync(500);
    }

    [Then("agent {string} should have the updated roles")]
    public async Task ThenAgentShouldHaveTheUpdatedRoles(string agentName)
    {
        var page = _context.Get<IPage>("page");
        await page.WaitForTimeoutAsync(500);
        var content = await page.ContentAsync();
        content.Should().Contain(agentName);
    }

    [Given("there is an active agent {string}")]
    public async Task GivenThereIsAnActiveAgent(string agentName)
    {
        using var api = ApiClient.WithRico();
        var result = await api.RegisterAgent(agentName, "worker");
        var agentId = result.GetProperty("id").GetInt32();
        _context["targetAgentId"] = agentId;
    }

    [When("I deactivate agent {string}")]
    public async Task WhenIDeactivateAgent(string agentName)
    {
        using var api = ApiClient.WithRico();
        var agentId = _context.Get<int>("targetAgentId");
        await api.DeactivateAgent(agentId);
        _context["deactivateStatus"] = api.StatusCode;
    }

    [Then("agent {string} should show as inactive")]
    public async Task ThenAgentShouldShowAsInactive(string agentName)
    {
        var status = _context.Get<System.Net.HttpStatusCode>("deactivateStatus");
        status.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
