using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace MadaTaskar.Tests.Support;

/// <summary>
/// Manages the test web application and Playwright browser lifecycle.
/// Starts the app on a random port and provides a headless Chromium browser.
/// </summary>
[SetUpFixture]
public class TestFixture
{
    public static WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public static HttpClient HttpClient { get; private set; } = null!;
    public static string BaseUrl { get; private set; } = null!;
    public static IPlaywright PlaywrightInstance { get; private set; } = null!;
    public static IBrowser Browser { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Start the application using WebApplicationFactory
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseUrls("http://127.0.0.1:0"); // random port
            });

        HttpClient = Factory.CreateClient();
        BaseUrl = Factory.Server.BaseAddress.ToString().TrimEnd('/');

        // Seed the test agent via direct DB access
        await SeedTestData();

        // Start Playwright
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    private async Task SeedTestData()
    {
        // Create test agent "TestBot" with known API key using the existing Rico agent
        // Rico is seeded by default with key "penguin-rico-key-change-me"
        // We also register a TestBot agent via API
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Agent-Key", TestData.RicoApiKey);

        var response = await client.PostAsJsonAsync("/api/agents/register", new
        {
            Name = "TestBot",
            Roles = "admin,worker,researcher,reviewer"
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TestAgentResponse>();
            if (result != null)
            {
                TestData.TestBotApiKey = result.ApiKey;
                TestData.TestBotId = result.Id;
            }
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (Browser != null) await Browser.CloseAsync();
        PlaywrightInstance?.Dispose();
        HttpClient?.Dispose();
        Factory?.Dispose();
    }

    /// <summary>
    /// Creates a new browser context (isolated cookies/storage) for a test.
    /// </summary>
    public static async Task<IBrowserContext> NewContextAsync()
    {
        return await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = BaseUrl
        });
    }

    private record TestAgentResponse(int Id, string Name, string ApiKey, string Roles);
}
