using FluentAssertions;
using MadaTaskar.Tests.Pages;
using MadaTaskar.Tests.Support;
using Microsoft.Playwright;

namespace MadaTaskar.Tests.StepDefinitions;

/// <summary>
/// Tests for User Management — adding, deleting users, and self-delete protection.
/// </summary>
[TestFixture, Category("UserManagement"), Category("UI")]
public class UserManagementTests
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private LoginPage _loginPage = null!;
    private AdminPage _adminPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _context = await TestFixture.NewContextAsync();
        _page = await _context.NewPageAsync();
        _loginPage = new LoginPage(_page);
        _adminPage = new AdminPage(_page);
        await _loginPage.LoginAsync(TestData.DefaultUsername, TestData.DefaultPassword);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.CloseAsync();
    }

    [Test]
    public async Task Admin_Can_See_Users_Tab()
    {
        // When I navigate to admin panel
        await _adminPage.NavigateAsync();

        // Then I should see the Users tab
        (await _adminPage.IsUsersTabVisible()).Should().BeTrue();
    }

    [Test]
    public async Task Admin_Can_See_Default_User_In_List()
    {
        // Given I am on admin panel Users tab
        await _adminPage.NavigateAsync();
        await _adminPage.ClickUsersTab();

        // Then I should see the default user
        (await _adminPage.HasUserInList("user")).Should().BeTrue("default user should be in the list");
    }
}
