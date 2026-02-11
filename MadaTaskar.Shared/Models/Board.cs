namespace MadaTaskar.Shared.Models;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = "Mada-TASK-ar Board 🐧";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Column> Columns { get; set; } = new List<Column>();
}