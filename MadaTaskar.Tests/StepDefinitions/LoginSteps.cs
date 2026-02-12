using System.Net;
using FluentAssertions;
using MadaTaskar.Tests.Pages;
using MadaTaskar.Tests.Support;
using Microsoft.Playwright;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Tests for user authentication — login, logout, and error handling.
/// </summary>
[TestFixture, Category("Login"), Category("UI")]
public class LoginTests
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private LoginPage _loginPage = null!;
    private HomePage _homePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _context = await TestFixture.NewContextAsync();
        _page = await _context.NewPageAsync();
        _loginPage = new LoginPage(_page);
        _homePage = new HomePage(_page);
    }

    [TearDown]
    public async Task TearDown()
    {
        await TestFixture.SaveTestArtifactsAsync(_context, _page, TestContext.CurrentContext.Test.Name);
        if (_context != null) await _context.CloseAsync();
    }

    [Test]
    public async Task User_Can_Login_With_Valid_Credentials()
    {
        // Given I am on the login page
        await _loginPage.NavigateAsync();

        // When I enter valid credentials and click login
        await _loginPage.EnterCredentialsAsync(TestData.DefaultUsername, TestData.DefaultPassword);
        await _loginPage.ClickLoginAsync();

        // Then I should be redirected to the board
        var isOnBoard = await _homePage.IsOnBoardPageAsync();
        isOnBoard.Should().BeTrue("user should be redirected to the board after login");

        // And I should see the app header
        var header = await _homePage.GetHeaderTextAsync();
        header.Should().Match("*Mada*", "the board header should be visible");
    }

    [Test]
    public async Task User_Cannot_Login_With_Wrong_Password()
    {
        // Given I am on the login page
        await _loginPage.NavigateAsync();

        // When I enter wrong password and click login
        await _loginPage.EnterCredentialsAsync(TestData.DefaultUsername, "wrongpassword");
        await _loginPage.ClickLoginAsync();

        // Then I should see an error and remain on login page
        var isOnLogin = await _loginPage.IsOnLoginPageAsync();
        isOnLogin.Should().BeTrue("user should remain on login page after failed login");
    }

    [Test]
    public async Task User_Can_Logout()
    {
        // Given I am logged in
        await _loginPage.LoginAsync(TestData.DefaultUsername, TestData.DefaultPassword);

        // When I click the logout button
        await _homePage.ClickLogoutAsync();

        // Then I should be redirected to the login page
        var isOnLogin = await _loginPage.IsOnLoginPageAsync();
        isOnLogin.Should().BeTrue("user should be on login page after logout");
    }
}
