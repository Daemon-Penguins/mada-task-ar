namespace MadaTaskar.Shared.DTOs;

public class BoardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ColumnDto> Columns { get; set; } = new();
}

public class CreateBoardDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateBoardDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}