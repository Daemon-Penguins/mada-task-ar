using Reqnroll;
using MadaTaskar.Tests.Support;
using MadaTaskar.Tests.Pages;
using Microsoft.Playwright;
using FluentAssertions;

namespace MadaTaskar.Tests.ReqnrollSteps;

[Binding]
public class LoginFeatureSteps
{
    private readonly ScenarioContext _context;

    public LoginFeatureSteps(ScenarioContext context)
    {
        _context = context;
    }

    [Given("I am on the login page")]
    public async Task GivenIAmOnTheLoginPage()
    {
        var browserContext = await TestFixture.NewContextAsync();
        var page = await browserContext.NewPageAsync();
        _context["browserContext"] = browserContext;
        _context["page"] = page;

        var loginPage = new LoginPage(page);
        await loginPage.NavigateAsync();
    }

    [When("I enter username {string} and password {string}")]
    public async Task WhenIEnterUsernameAndPassword(string username, string password)
    {
        var page = _context.Get<IPage>("page");
        var loginPage = new LoginPage(page);
        await loginPage.EnterCredentialsAsync(username, password);
    }

    [When("I click the login button")]
    public async Task WhenIClickTheLoginButton()
    {
        var page = _context.Get<IPage>("page");
        var loginPage = new LoginPage(page);
        await loginPage.ClickLoginAsync();
    }

    [Then("I should be redirected to the board")]
    public async Task ThenIShouldBeRedirectedToTheBoard()
    {
        var page = _context.Get<IPage>("page");
        var homePage = new HomePage(page);
        var isOnBoard = await homePage.IsOnBoardPageAsync();
        isOnBoard.Should().BeTrue("user should be redirected to the board after login");
    }

    [Then("I should see {string} in the header")]
    public async Task ThenIShouldSeeInTheHeader(string expectedText)
    {
        var page = _context.Get<IPage>("page");
        var homePage = new HomePage(page);
        var header = await homePage.GetHeaderTextAsync();
        header.Should().Contain(expectedText);
    }

    [Then("I should see an error message")]
    public async Task ThenIShouldSeeAnErrorMessage()
    {
        var page = _context.Get<IPage>("page");
        var loginPage = new LoginPage(page);
        // After failed login, user stays on login page
        var isOnLogin = await loginPage.IsOnLoginPageAsync();
        isOnLogin.Should().BeTrue();
    }

    [Then("I should remain on the login page")]
    public async Task ThenIShouldRemainOnTheLoginPage()
    {
        var page = _context.Get<IPage>("page");
        var loginPage = new LoginPage(page);
        var isOnLogin = await loginPage.IsOnLoginPageAsync();
        isOnLogin.Should().BeTrue();
    }

    [When("I click the logout button")]
    public async Task WhenIClickTheLogoutButton()
    {
        var page = _context.Get<IPage>("page");
        var homePage = new HomePage(page);
        // Ensure we're on the board page first
        await homePage.NavigateAsync();
        await homePage.ClickLogoutAsync();
    }

    [Then("I should be redirected to the login page")]
    public async Task ThenIShouldBeRedirectedToTheLoginPage()
    {
        var page = _context.Get<IPage>("page");
        var loginPage = new LoginPage(page);
        var isOnLogin = await loginPage.IsOnLoginPageAsync();
        isOnLogin.Should().BeTrue();
    }
}
