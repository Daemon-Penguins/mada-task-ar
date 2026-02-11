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

## 🤖 Multi-Agent REST API

All endpoints require the `X-Agent-Key` header for authentication.

**Default admin agent:** `Rico` with key `penguin-rico-key-change-me`

### Endpoints

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/api/agents` | List agents | admin |
| POST | `/api/agents/register` | Register new agent | admin |
| GET | `/api/board` | Get board with columns & tasks | any |
| GET | `/api/board/columns` | List columns | any |
| POST | `/api/tasks` | Create task | any |
| PUT | `/api/tasks/{id}` | Update task | any |
| POST | `/api/tasks/{id}/move` | Move task to column | any |
| DELETE | `/api/tasks/{id}` | Delete task | any |
| POST | `/api/tasks/{id}/assign` | Assign task to agent | any |
| GET | `/api/activity` | Get activity log | any |

### Examples

```bash
# Set your key
KEY="penguin-rico-key-change-me"

# Get the board
curl -H "X-Agent-Key: $KEY" http://localhost:8080/api/board

# List columns
curl -H "X-Agent-Key: $KEY" http://localhost:8080/api/board/columns

# Create a task
curl -X POST -H "X-Agent-Key: $KEY" -H "Content-Type: application/json" \
  -d '{"title":"New task","columnId":2,"priority":2}' \
  http://localhost:8080/api/tasks

# Move task to column 3, order 0
curl -X POST -H "X-Agent-Key: $KEY" -H "Content-Type: application/json" \
  -d '{"columnId":3,"order":0}' \
  http://localhost:8080/api/tasks/1/move

# Assign task to self
curl -X POST -H "X-Agent-Key: $KEY" -H "Content-Type: application/json" \
  -d '{}' http://localhost:8080/api/tasks/1/assign

# Register a new agent (admin only)
curl -X POST -H "X-Agent-Key: $KEY" -H "Content-Type: application/json" \
  -d '{"name":"Skipper","role":"worker"}' \
  http://localhost:8080/api/agents/register

# View activity log
curl -H "X-Agent-Key: $KEY" http://localhost:8080/api/activity
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
