using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MadaTaskar.Components;
using MadaTaskar.Data;
using MadaTaskar.Services;
using MadaTaskar.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseInMemoryDatabase("MadaTaskar"));

builder.Services.AddScoped<BoardService>();
builder.Services.AddScoped<AgentService>();

var app = builder.Build();

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Agent API
app.MapAgentApi();

app.Run();
