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

## Task Pipeline v2 — Phases, Roles & Permissions

### Agent Roles
Agents have comma-separated roles (e.g. `"admin,worker,researcher"`). Available roles: `admin`, `worker`, `researcher`, `architect`, `reviewer`.

### Task Phases
Tasks follow a pipeline: `Research → Brainstorm → Triage → AuthorReview → ReadyToWork → InProgress → Acceptance → Completed`. Tasks can also be `Killed` at any point.

Phase changes auto-map to board columns:
| Phase | Column |
|-------|--------|
| Research, Brainstorm, Triage | Backlog (2) |
| AuthorReview | Acceptance (4) |
| ReadyToWork | Backlog (2) |
| InProgress | In Progress (3) |
| Acceptance | Acceptance (4) |
| Completed | Done (5) |
| Killed | Rejected (6) |

### New API Endpoints

```bash
KEY="penguin-rico-key-change-me"
H="X-Agent-Key: $KEY"

# Register agent with roles
curl -X POST localhost:5000/api/agents/register -H "$H" -H "Content-Type: application/json" \
  -d '{"name":"Scout","roles":"researcher,worker"}'

# Add research reference
curl -X POST localhost:5000/api/tasks/1/research -H "$H" -H "Content-Type: application/json" \
  -d '{"url":"https://example.com","title":"Ref title","summary":"Key findings"}'

# Add proposal
curl -X POST localhost:5000/api/tasks/1/propose -H "$H" -H "Content-Type: application/json" \
  -d '{"content":"I propose we do X because Y"}'

# Comment on task
curl -X POST localhost:5000/api/tasks/1/comment -H "$H" -H "Content-Type: application/json" \
  -d '{"content":"Looks good","type":"Remark"}'

# Advance phase
curl -X POST localhost:5000/api/tasks/1/advance -H "$H" -H "Content-Type: application/json" \
  -d '{"targetPhase":"Brainstorm","reason":"Research complete"}'

# Approve task
curl -X POST localhost:5000/api/tasks/1/approve -H "$H" -H "Content-Type: application/json" \
  -d '{"comment":"LGTM"}'

# Reject task
curl -X POST localhost:5000/api/tasks/1/reject -H "$H" -H "Content-Type: application/json" \
  -d '{"comment":"Not viable"}'

# Request changes
curl -X POST localhost:5000/api/tasks/1/request-changes -H "$H" -H "Content-Type: application/json" \
  -d '{"comment":"Fix the edge case"}'

# Get timeline
curl localhost:5000/api/tasks/1/timeline -H "$H"

# Get references
curl localhost:5000/api/tasks/1/references -H "$H"

# Get proposals
curl localhost:5000/api/tasks/1/proposals -H "$H"

# Get available transitions for a task
curl localhost:5000/api/tasks/1/transitions -H "$H"

# Advance phase using state machine
curl -X POST localhost:5000/api/tasks/1/advance -H "$H" \
  -H "Content-Type: application/json" \
  -d '{"targetPhase":"Brainstorm","reason":"Research complete"}'
```

## State Machine

All task phase transitions are validated by `TaskStateMachine`. Invalid transitions return clear error messages.

### Valid Transitions

| From | To | Description | Required Roles |
|------|-----|-------------|----------------|
| Research | Brainstorm | Research completed, open for brainstorming | researcher, architect, admin |
| Brainstorm | Triage | Brainstorming done, ready for triage | architect, admin |
| Triage | AuthorReview | Triage complete, awaiting author review | architect, admin |
| AuthorReview | ReadyToWork | Author approved, ready to work | author*, admin |
| ReadyToWork | InProgress | Work started | worker, admin |
| InProgress | Acceptance | Work completed, awaiting acceptance | worker, admin |
| Acceptance | Completed | Accepted and done | author*, reviewer, admin |
| Acceptance | InProgress | Changes requested, back to work | author*, reviewer, admin |
| Acceptance | Research | Too many issues, restarting | author*, admin |
| Any (except Completed/Killed) | Killed | Task killed/rejected | author*, admin |

\* "author" = agent must be the task's `AuthorAgentId`

### Special Validations

- **ReadyToWork**: `ReadyToWorkChecked` flag must be `true`
- **InProgress** (from ReadyToWork): Task must have an `AssignedAgentId`

### Error Response Format

```json
{
  "error": "Invalid transition: 'Research' → 'InProgress'. Allowed transitions from 'Research': [Brainstorm, Killed].",
  "currentPhase": "Research"
}
```

### GET /api/tasks/{id}/transitions

Returns available transitions with permission checks:

```json
{
  "currentPhase": "Research",
  "availableTransitions": [
    {"targetPhase": "Brainstorm", "description": "Research completed, open for brainstorming", "allowed": true, "reason": null},
    {"targetPhase": "Killed", "description": "Idea rejected", "allowed": true, "reason": null}
  ]
}
```

Endpoints `/approve`, `/reject`, `/request-changes` also use the state machine and return 400 with descriptive errors on invalid transitions.
