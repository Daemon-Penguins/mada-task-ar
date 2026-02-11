# Architecture

## Tech Stack

| Layer | Technology | Why |
|-------|-----------|-----|
| **Frontend** | Blazor Server + MudBlazor | Rich interactive UI with C# — no JavaScript needed. MudBlazor provides Material Design components out of the box. |
| **Backend** | ASP.NET Core 9 | High-performance, modern .NET runtime. Blazor Server uses SignalR for real-time UI updates. |
| **Database** | EF Core InMemory | Zero-config for development. Data resets on restart — perfect for demos and dev environments. |
| **API** | Minimal APIs | Lightweight REST endpoints for agent integration. No heavy controller overhead. |
| **Auth (UI)** | Cookie Authentication | Standard ASP.NET cookie auth for human users via the web UI. |
| **Auth (API)** | X-Agent-Key Header | Simple API key authentication for AI agents. Each agent has a unique key. |
| **CI/CD** | GitHub Actions + Docker | Automated builds, Docker image publishing to Docker Hub. |

## Project Structure

```
mada-task-ar/
├── MadaTaskar/                    # Main application
│   ├── Api/
│   │   ├── AgentApiEndpoints.cs   # REST API for agents (/api/*)
│   │   └── AuthEndpoints.cs       # Login/logout endpoints
│   ├── Components/
│   │   ├── Layout/                # Blazor layouts
│   │   └── Pages/
│   │       ├── Home.razor         # Kanban board (main page)
│   │       ├── Admin.razor        # Admin panel (agents, users, activity)
│   │       ├── Login.razor        # Login page
│   │       ├── TaskDialog.razor   # Task detail/edit dialog
│   │       ├── AddAgentDialog.razor
│   │       ├── AddUserDialog.razor
│   │       └── EditAgentRolesDialog.razor
│   ├── Data/
│   │   ├── AppDbContext.cs        # EF Core context + seed data
│   │   ├── Entities.cs            # All entity classes
│   │   └── Models.cs              # DTOs/request models
│   ├── Services/
│   │   ├── BoardService.cs        # Board/task CRUD operations
│   │   ├── AgentService.cs        # Agent management + activity logging
│   │   ├── TaskStateMachine.cs    # Phase transition rules + validation
│   │   ├── PermissionService.cs   # Role-based permission checks
│   │   └── RewardService.cs       # Badge/reward system
│   └── Program.cs                 # Application entry point + DI setup
├── MadaTaskar.Tests/              # Test project
│   ├── Features/                  # Gherkin feature files
│   ├── StepDefinitions/           # NUnit test classes
│   ├── Pages/                     # Page Object Model
│   └── Support/                   # Test infrastructure
├── docs/                          # Documentation
├── Dockerfile
├── docker-compose.yml
└── .github/workflows/ci.yml
```

## How Blazor Server Works

Blazor Server runs the UI logic on the server. The browser connects via a **SignalR WebSocket**, and every UI interaction is:

1. **User clicks a button** → Event sent over SignalR to server
2. **Server processes the event** → Updates the component tree
3. **Server computes a diff** → Sends only the changed DOM to the browser
4. **Browser applies the diff** → UI updates instantly

This means:
- **No WASM download** — fast initial load
- **Full .NET on server** — direct database access, no API layer needed for UI
- **Real-time capable** — SignalR enables live updates

Trade-off: requires persistent WebSocket connection. Not ideal for unreliable networks, but perfect for internal tools and boards.

## Database

EF Core InMemory database is used for simplicity:
- **No migration headaches** — schema is defined in code
- **Fresh start on restart** — seed data recreated automatically
- **Fast** — all operations are in-memory

Seed data includes:
- 1 default user (`user` / `password`, admin)
- 1 default agent (Rico, admin+worker+researcher)
- 1 board with 6 columns (Ideas, Backlog, In Progress, Acceptance, Done, Rejected)
- 3 sample tasks

For production persistence, swap to SQLite/PostgreSQL by changing one line in `Program.cs`.
