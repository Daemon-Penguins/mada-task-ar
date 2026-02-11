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

```bash
cd MadaTaskar
dotnet run
```

Open http://localhost:5000

## Docker

```bash
docker compose up --build
```

Open http://localhost:8080

## Screenshot

The board starts with seed data: an "Operations Board" with sample tasks to get you started.

## License

MIT
