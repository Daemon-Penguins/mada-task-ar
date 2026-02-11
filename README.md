# 🐧 Mada-TASK-ar

Kanban board for mixed teams — humans and AI agents working together.

## Tech Stack

- **Blazor Server** (.NET 9) with interactive server-side rendering
- **MudBlazor** — Material Design component library (dark theme)
- **EF Core** — InMemory database (perfect for dev/demo)
- **Drag-and-drop** — Move tasks between columns with MudDropContainer

## Features

- 📋 Kanban board with 6 columns: Ideas → Backlog → In Progress → Acceptance → Done → Rejected
- 🖱️ Drag-and-drop tasks between columns
- ✏️ Create, edit, and delete tasks
- 🏷️ Priority levels: Low, Medium, High, Critical
- 👤 Assignee tracking
- 🌙 Dark theme by default

## Quick Start

### Docker Hub (recommended)

Pull and run the latest image:

```bash
docker run -d -p 8080:8080 nieprzecietnykowalski/mada-task-ar:latest
```

Or with docker compose:

```bash
docker compose up -d
```

Open http://localhost:8080

### From source

```bash
cd MadaTaskar
dotnet run
```

Open http://localhost:5000

### Build locally

```bash
docker build -t mada-task-ar .
docker run -d -p 8080:8080 mada-task-ar
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Runtime environment |
| `ASPNETCORE_URLS` | `http://+:8080` | Listen URL |

## CI/CD

Every push to `main` automatically builds and pushes a new Docker image to [Docker Hub](https://hub.docker.com/r/nieprzecietnykowalski/mada-task-ar).

Tags:
- `latest` — always the newest build
- `<commit-sha>` — pinned to a specific commit

## Screenshot

The board starts with seed data: an "Operations Board" with sample tasks to get you started.

## License

MIT
