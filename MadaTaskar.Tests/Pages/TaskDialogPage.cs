using Microsoft.Playwright;

namespace MadaTaskar.Tests.Pages;

/// <summary>
/// Page Object for the Task Dialog (create/edit task modal).
/// Handles task form fields, priority selection, and CRUD actions.
/// </summary>
public class TaskDialogPage
{
    private readonly IPage _page;

    public TaskDialogPage(IPage page) => _page = page;

    // Dialog container
    private ILocator Dialog => _page.Locator(".mud-dialog");

    // Form fields
    private ILocator TitleInput => Dialog.Locator("input").First;
    private ILocator DescriptionInput => Dialog.Locator("textarea").First;

    // Buttons
    private ILocator CreateButton => Dialog.Locator("button:has-text('Create'), button:has-text('Add')").First;
    private ILocator SaveButton => Dialog.Locator("button:has-text('Save'), button:has-text('Update')").First;
    private ILocator DeleteButton => Dialog.Locator("button:has-text('Delete')").First;
    private ILocator CloseButton => Dialog.Locator("button:has-text('Close'), button:has-text('Cancel'), button.mud-dialog-close").First;

    public async Task<bool> IsOpenAsync()
    {
        return await Dialog.CountAsync() > 0 && await Dialog.First.IsVisibleAsync();
    }

    public async Task FillTitleAsync(string title)
    {
        await TitleInput.FillAsync(title);
    }

    public async Task FillDescriptionAsync(string description)
    {
        await DescriptionInput.FillAsync(description);
    }

    public async Task ClickCreateAsync()
    {
        await CreateButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task ClickSaveAsync()
    {
        await SaveButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task ClickDeleteAsync()
    {
        await DeleteButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task CloseAsync()
    {
        await CloseButton.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<List<IElementHandle>> GetAllInteractiveElements()
    {
        return (await Dialog.Locator("button:not([disabled]), input, textarea, select, [role='button']:not([aria-disabled='true'])").ElementHandlesAsync()).ToList();
    }
}
