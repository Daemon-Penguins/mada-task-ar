using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using MadaTaskar.Tests.Support;
using Microsoft.Playwright;

namespace MadaTaskar.Tests;

/// <summary>
/// Starts the real app as a subprocess and launches Playwright.
/// </summary>
[SetUpFixture]
public class TestFixture
{
    public static HttpClient HttpClient { get; private set; } = null!;
    public static string BaseUrl { get; private set; } = null!;
    public static IPlaywright PlaywrightInstance { get; private set; } = null!;
    public static IBrowser Browser { get; private set; } = null!;
    private static Process? _appProcess;
    private static readonly StringBuilder _appStdout = new();
    private static readonly StringBuilder _appStderr = new();

    private static readonly int Port = Random.Shared.Next(15000, 16000);

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        BaseUrl = $"http://127.0.0.1:{Port}";

        // Find the built DLL
        var testDir = AppContext.BaseDirectory; // e.g. .../bin/Release/net10.0/
        var projectRoot = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", ".."));
        var appDll = Path.Combine(projectRoot, "MadaTaskar", "bin", "Release", "net10.0", "MadaTaskar.dll");

        if (!File.Exists(appDll))
        {
            // Try Debug
            appDll = Path.Combine(projectRoot, "MadaTaskar", "bin", "Debug", "net10.0", "MadaTaskar.dll");
        }

        if (!File.Exists(appDll))
            throw new FileNotFoundException($"Cannot find MadaTaskar.dll. Looked in: {appDll}");

        // Start app process
        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{appDll}\" --urls {BaseUrl}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = projectRoot,
                Environment =
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Development"
                }
            }
        };
        _appProcess.OutputDataReceived += (_, e) => { if (e.Data != null) _appStdout.AppendLine(e.Data); };
        _appProcess.ErrorDataReceived += (_, e) => { if (e.Data != null) _appStderr.AppendLine(e.Data); };
        _appProcess.Start();

        // Drain stdout/stderr asynchronously to prevent pipe buffer deadlock
        _appProcess.BeginOutputReadLine();
        _appProcess.BeginErrorReadLine();

        // Wait for server to be ready
        HttpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        var ready = false;
        for (int i = 0; i < 50; i++)
        {
            try
            {
                var resp = await HttpClient.GetAsync("/api/me");
                ready = true;
                break;
            }
            catch { await Task.Delay(200); }
        }

        if (!ready)
        {
            throw new Exception($"App failed to start.\nSTDOUT: {_appStdout}\nSTDERR: {_appStderr}");
        }

        // Seed test agent
        await SeedTestData();

        // Start Playwright (skip if browsers not installed — allows NUnit-only CI runs)
        try
        {
            PlaywrightInstance = await Playwright.CreateAsync();
            Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            Console.WriteLine($"✅ Test server at {BaseUrl}, Playwright ready");
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist"))
        {
            Console.WriteLine($"⚠️ Playwright browsers not installed — UI tests will be skipped. {ex.Message}");
            PlaywrightInstance = null;
            Browser = null!;
        }
    }

    private async Task SeedTestData()
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
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
                    Console.WriteLine($"✅ TestBot seeded: id={result.Id}, key={result.ApiKey}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Failed to seed TestBot: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Seed error: {ex.Message}");
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (Browser != null) await Browser.CloseAsync();
        if (PlaywrightInstance != null) PlaywrightInstance.Dispose();
        HttpClient?.Dispose();
        if (_appProcess is { HasExited: false })
        {
            _appProcess.Kill(entireProcessTree: true);
            _appProcess.Dispose();
        }
    }

    public static string TestResultsDir { get; } = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "TestResults", "playwright");

    public static async Task<IBrowserContext> NewContextAsync()
    {
        if (Browser == null)
            Assert.Ignore("Playwright browsers not installed — skipping UI test.");

        Directory.CreateDirectory(TestResultsDir);

        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            BaseURL = BaseUrl
        });

        // Start tracing for each context (captures screenshots, DOM snapshots, network)
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = false
        });

        return context;
    }

    /// <summary>
    /// Call in TearDown to save trace + screenshot on failure.
    /// </summary>
    public static async Task SaveTestArtifactsAsync(IBrowserContext? context, IPage? page, string testName)
    {
        if (context == null) return;

        var safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));

        try
        {
            // Save screenshot on failure
            if (page != null && TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                var screenshotPath = Path.Combine(TestResultsDir, $"{safeName}.png");
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });
                TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");
            }

            // Save trace (always — useful for debugging)
            var tracePath = Path.Combine(TestResultsDir, $"{safeName}.zip");
            await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to save test artifacts: {ex.Message}");
        }
    }

    private record TestAgentResponse(int Id, string Name, string ApiKey, string Roles);
}
