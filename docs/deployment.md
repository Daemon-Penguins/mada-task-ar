# Deployment

## Docker Run

```bash
docker run -p 8080:8080 nieprzecietnykowalski/mada-task-ar:latest
```

Open http://localhost:8080 — login with `user` / `password`.

## Docker Compose

```yaml
services:
  madataskar:
    image: nieprzecietnykowalski/mada-task-ar:latest
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

```bash
docker compose up -d
```

## Build Locally

```bash
cd MadaTaskar
dotnet run
```

Runs on http://localhost:5000 by default.

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_URLS` | `http://+:8080` | Listen URL(s) |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment name |

## Production Considerations

### Data Persistence
The app uses **InMemory database** — data is lost on restart. For production:

```csharp
// In Program.cs, replace:
options.UseInMemoryDatabase("MadaTaskar");
// With:
options.UseSqlite("Data Source=madataskar.db");
// Or:
options.UseNpgsql(connectionString);
```

### HTTPS
The app does **not** handle TLS directly. Use a reverse proxy:

```nginx
server {
    listen 443 ssl;
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";  # Required for SignalR WebSocket
        proxy_set_header Host $host;
    }
}
```

**Important:** The `Upgrade` and `Connection` headers are required for Blazor Server's SignalR WebSocket connection.

### Resource Usage
- Memory: ~50-100 MB (InMemory DB grows with data)
- CPU: minimal — most time is idle, waiting for SignalR events
- Connections: one WebSocket per connected browser tab
