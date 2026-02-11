using Microsoft.EntityFrameworkCore;

namespace MadaTaskar.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentActivity> AgentActivities => Set<AgentActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>().HasIndex(a => a.ApiKey).IsUnique();
        modelBuilder.Entity<AgentActivity>().HasIndex(a => a.CreatedAt);
        modelBuilder.Entity<AgentActivity>().HasIndex(a => a.AgentId);

        modelBuilder.Entity<Agent>().HasData(new Agent
        {
            Id = 1, Name = "Rico", ApiKey = "penguin-rico-key-change-me",
            Role = "admin", IsActive = true, CreatedAt = DateTime.UtcNow
        });

        modelBuilder.Entity<Board>().HasData(new Board { Id = 1, Name = "Operations Board" });

        modelBuilder.Entity<Column>().HasData(
            new Column { Id = 1, Name = "Ideas", Order = 0, BoardId = 1 },
            new Column { Id = 2, Name = "Backlog", Order = 1, BoardId = 1 },
            new Column { Id = 3, Name = "In Progress", Order = 2, BoardId = 1 },
            new Column { Id = 4, Name = "Acceptance", Order = 3, BoardId = 1 },
            new Column { Id = 5, Name = "Done", Order = 4, BoardId = 1 },
            new Column { Id = 6, Name = "Rejected", Order = 5, BoardId = 1 }
        );

        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem { Id = 1, Title = "Welcome to Mada-TASK-ar! 🐧", Description = "Drag me to another column", Assignee = "Penguin", Priority = Priority.Medium, Order = 0, ColumnId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskItem { Id = 2, Title = "Set up CI/CD pipeline", Priority = Priority.High, Order = 0, ColumnId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskItem { Id = 3, Title = "Deploy to production", Priority = Priority.Critical, Order = 1, ColumnId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
    }
}
