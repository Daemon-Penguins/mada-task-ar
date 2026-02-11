using Microsoft.EntityFrameworkCore;
using MadaTaskar.Shared.Models;

namespace MadaTaskar.Data;

public class KanbanDbContext : DbContext
{
    public KanbanDbContext(DbContextOptions<KanbanDbContext> options) : base(options)
    {
    }

    public DbSet<Board> Boards { get; set; }
    public DbSet<Column> Columns { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Board>()
            .HasMany(b => b.Columns)
            .WithOne(c => c.Board)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Column>()
            .HasMany(c => c.Tasks)
            .WithOne(t => t.Column)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure entity properties
        modelBuilder.Entity<Board>()
            .Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<Column>()
            .Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Title)
            .HasMaxLength(300)
            .IsRequired();

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Assignee)
            .HasMaxLength(100);

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default board
        var board = new Board
        {
            Id = 1,
            Name = "Mada-TASK-ar Board 🐧",
            Description = "Main Kanban board for penguins and humans",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Board>().HasData(board);

        // Seed default columns
        var columns = new[]
        {
            new Column { Id = 1, Name = "Ideas", Description = "New ideas and concepts", Order = 1, BoardId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Column { Id = 2, Name = "Backlog", Description = "Tasks ready to be picked up", Order = 2, BoardId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Column { Id = 3, Name = "In Progress", Description = "Work in progress", Order = 3, BoardId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Column { Id = 4, Name = "Acceptance", Description = "Ready for review and testing", Order = 4, BoardId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Column { Id = 5, Name = "Done", Description = "Completed tasks", Order = 5, BoardId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Column { Id = 6, Name = "Rejected", Description = "Tasks that were rejected or cancelled", Order = 6, BoardId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        modelBuilder.Entity<Column>().HasData(columns);

        // Seed sample tasks
        var tasks = new[]
        {
            new TaskItem { Id = 1, Title = "Welcome to Mada-TASK-ar! 🐧", Description = "This is your first task. Feel free to move it around or edit it.", Assignee = "Penguin Overlord", Priority = Priority.Medium, Order = 1, ColumnId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskItem { Id = 2, Title = "Create user documentation", Description = "Write comprehensive documentation for users", Assignee = "Claude", Priority = Priority.High, Order = 1, ColumnId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskItem { Id = 3, Title = "Implement drag & drop feature", Description = "Add drag and drop functionality between columns", Assignee = "Developer", Priority = Priority.Critical, Order = 1, ColumnId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        modelBuilder.Entity<TaskItem>().HasData(tasks);
    }
}