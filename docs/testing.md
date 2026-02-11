# Testing

## Quick Start

```bash
# Install Playwright browsers (first time only)
cd MadaTaskar.Tests
dotnet build
pwsh bin/Debug/net9.0/playwright.ps1 install chromium

# Run all tests
dotnet test

# Run specific category
dotnet test --filter "Category=API"
dotnet test --filter "Category=UI"
dotnet test --filter "Category=Login"
dotnet test --filter "Category=MonkeyTest"
```

## Test Categories

| Category | Type | Description |
|----------|------|-------------|
| **Login** | UI | User authentication — login, logout, error handling |
| **AdminPanel** | UI | Admin panel navigation, tab switching |
| **UserManagement** | UI | User CRUD via admin panel |
| **BoardManagement** | UI | Kanban board columns, task cards |
| **AgentManagement** | API | Agent registration, deactivation, permissions |
| **TaskLifecycle** | API | Full pipeline flow, phase transitions, validation |
| **ApiIntegration** | API | REST API endpoints, auth, CRUD, retrospectives |
| **MonkeyTest** | UI | Random button clicking, stress testing, crash detection |

## Architecture

### Test Infrastructure
- **TestFixture.cs** — `[SetUpFixture]` that starts the app via `WebApplicationFactory` and Playwright browser
- **ApiClient.cs** — HTTP wrapper for all API endpoints with agent key auth
- **TestData.cs** — Shared constants (credentials, API keys, column IDs)

### Page Object Model
- **LoginPage.cs** — Login form interactions
- **HomePage.cs** — Kanban board interactions
- **AdminPage.cs** — Admin panel tab/table interactions
- **TaskDialogPage.cs** — Task create/edit dialog

### Feature Files (Gherkin)
Located in `Features/` — business-readable scenario descriptions. Written in plain English for non-technical stakeholders.

## Writing New Tests

1. **API tests** — Use `ApiClient` helper, categorize as `[Category("API")]`
2. **UI tests** — Use Page Objects + Playwright, categorize as `[Category("UI")]`
3. **Name tests descriptively** — `User_Can_Login_With_Valid_Credentials()`

Example:
```csharp
[Test, Category("MyFeature"), Category("API")]
public async Task Agent_Can_Do_Something_Cool()
{
    using var api = ApiClient.WithRico();
    var result = await api.CreateTask("Test");
    api.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

## Monkey Testing

Monkey tests simulate chaotic user behavior:
1. Find all visible, non-disabled buttons on the page
2. Click each one, wait briefly
3. Close any dialogs that pop up
4. Check browser console for JavaScript errors
5. Verify the page didn't crash

This catches unexpected errors from button interactions, dialog race conditions, and state corruption.
