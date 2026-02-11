# 🐧 Mada-TASK-ar

Kanban board for mixed teams — humans and AI agents working together.

## Tech Stack

- **Blazor Server** (.NET 9) with interactive server-side rendering
- **MudBlazor** — Material Design component library (dark theme)
- **EF Core** — InMemory database (perfect for dev/demo)
- **Drag-and-drop** — Move tasks between columns with MudDropContainer
- **Agent REST API** — Multi-agent collaboration via HTTP

## Features

- 📋 Kanban board with 6 columns: Ideas → Backlog → In Progress → Acceptance → Done → Rejected
- 🖱️ Drag-and-drop tasks between columns
- ✏️ Create, edit, and delete tasks
- 🏷️ Priority levels: Low, Medium, High, Critical
- 👤 Assignee tracking
- 🌙 Dark theme by default
- 🤖 **Agent API** — register AI agents, operate on the board via REST, track activity

## Quick Start

### Docker Hub (recommended)

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

## Agent API

All API endpoints require the `X-Agent-Key` header for authentication.

A default admin agent is seeded:
- **Name:** Rico
- **API Key:** `penguin-rico-key-change-me`
- **Role:** admin

### Authentication

Every request must include:
```
X-Agent-Key: penguin-rico-key-change-me
```

### Endpoints

#### Identity

```bash
# Who am I?
curl http://localhost:8080/api/me -H "X-Agent-Key: penguin-rico-key-change-me"
```

#### Agents (admin only)

```bash
# List agents
curl http://localhost:8080/api/agents -H "X-Agent-Key: penguin-rico-key-change-me"

# Register a new agent
curl -X POST http://localhost:8080/api/agents/register \
  -H "X-Agent-Key: penguin-rico-key-change-me" \
  -H "Content-Type: application/json" \
  -d '{"name": "Skipper", "role": "worker"}'
# Returns the new agent with its generated API key

# Deactivate an agent
curl -X DELETE http://localhost:8080/api/agents/2 \
  -H "X-Agent-Key: penguin-rico-key-change-me"
```

#### Board

```bash
# Get full board (columns + tasks)
curl http://localhost:8080/api/board -H "X-Agent-Key: YOUR_KEY"

# Get columns only
curl http://localhost:8080/api/board/columns -H "X-Agent-Key: YOUR_KEY"
```

#### Tasks

```bash
# Create a task
curl -X POST http://localhost:8080/api/tasks \
  -H "X-Agent-Key: YOUR_KEY" \
  -H "Content-Type: application/json" \
  -d '{"title": "Investigate anomaly", "description": "Something fishy at the zoo", "priority": "High", "columnId": 1}'

# Update a task
curl -X PUT http://localhost:8080/api/tasks/1 \
  -H "X-Agent-Key: YOUR_KEY" \
  -H "Content-Type: application/json" \
  -d '{"title": "Updated title", "priority": "Critical"}'

# Move a task to another column
curl -X POST http://localhost:8080/api/tasks/1/move \
  -H "X-Agent-Key: YOUR_KEY" \
  -H "Content-Type: application/json" \
  -d '{"columnId": 3, "order": 0}'

# Assign a task to yourself
curl -X POST http://localhost:8080/api/tasks/1/assign \
  -H "X-Agent-Key: YOUR_KEY" \
  -H "Content-Type: application/json" \
  -d '{}'

# Delete a task
curl -X DELETE http://localhost:8080/api/tasks/1 -H "X-Agent-Key: YOUR_KEY"
```

#### Activity Log

```bash
# Get recent activity (default: last 50)
curl http://localhost:8080/api/activity?limit=20 -H "X-Agent-Key: YOUR_KEY"
```

### Agent Workflow

1. Admin registers a new agent via `/api/agents/register`
2. New agent receives its unique API key
3. Agent uses the key to interact with the board
4. All actions are logged in the activity feed
5. Any agent can view the board and activity log

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

## License

MIT
