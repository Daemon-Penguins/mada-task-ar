using Microsoft.Playwright;

namespace MadaTaskar.Tests.Pages;

/// <summary>
/// Page Object for the Login page (/login).
/// Encapsulates all login-related UI interactions.
/// </summary>
public class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page) => _page = page;

    // Selectors
    private ILocator UsernameInput => _page.Locator("input[autocomplete='username'], input[type='text']").First;
    private ILocator PasswordInput => _page.Locator("input[type='password']").First;
    private ILocator LoginButton => _page.Locator("button[type='submit'], button:has-text('Login'), button:has-text('Sign in')").First;
    private ILocator ErrorMessage => _page.Locator(".mud-alert-text-error, .mud-snackbar-error, .error-message, .mud-alert");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task EnterCredentialsAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
    }

    public async Task ClickLoginAsync()
    {
        await LoginButton.ClickAsync();
        await _page.WaitForTimeoutAsync(1000); // wait for Blazor Server round-trip
    }

    public async Task LoginAsync(string username, string password)
    {
        await NavigateAsync();
        await EnterCredentialsAsync(username, password);
        await ClickLoginAsync();
    }

    public async Task<bool> IsOnLoginPageAsync()
    {
        return _page.Url.Contains("/login");
    }

    public async Task<bool> HasErrorMessageAsync()
    {
        return await ErrorMessage.CountAsync() > 0 && await ErrorMessage.First.IsVisibleAsync();
    }
}
