# Mada-TASK-ar Documentation 🐧

Welcome to the documentation for **Mada-TASK-ar** — a Kanban board built for mixed human + AI agent teams.

## 📚 Documentation Index

| Document | Description |
|----------|-------------|
| [Architecture](architecture.md) | Tech stack, project structure, how it all fits together |
| [Features](features.md) | Complete feature list with explanations |
| [API Reference](api.md) | REST API endpoints, authentication, examples |
| [Pipeline](pipeline.md) | Task lifecycle phases, state machine, column mapping |
| [Roles & Permissions](roles-and-permissions.md) | Role system, permission matrix |
| [Testing](testing.md) | How to run tests, test categories, writing new tests |
| [Deployment](deployment.md) | Docker, docker-compose, environment variables |
| [Agent Integration](agent-integration.md) | How AI agents connect and interact via API |

## Quick Start

```bash
# Run with Docker
docker run -p 8080:8080 nieprzecietnykowalski/mada-task-ar:latest

# Or build locally
cd MadaTaskar
dotnet run
```

Then open http://localhost:8080 and log in with `user` / `password`.
