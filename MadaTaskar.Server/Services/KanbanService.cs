using System.Text;
using System.Text.Json;
using MadaTaskar.Shared.DTOs;
using MadaTaskar.Shared.Models;

namespace MadaTaskar.Server.Services;

public class KanbanService : IKanbanService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public KanbanService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Boards
    public async Task<IEnumerable<BoardDto>> GetBoardsAsync()
    {
        var response = await _httpClient.GetAsync("boards");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<BoardDto>>(json, _jsonOptions) ?? new List<BoardDto>();
    }

    public async Task<BoardDto?> GetBoardAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"boards/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BoardDto>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<BoardDto?> CreateBoardAsync(CreateBoardDto createBoardDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(createBoardDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("boards", content);
            if (!response.IsSuccessStatusCode) return null;
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BoardDto>(responseJson, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateBoardAsync(int id, UpdateBoardDto updateBoardDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(updateBoardDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"boards/{id}", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteBoardAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"boards/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Columns
    public async Task<IEnumerable<ColumnDto>> GetColumnsAsync()
    {
        var response = await _httpClient.GetAsync("columns");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ColumnDto>>(json, _jsonOptions) ?? new List<ColumnDto>();
    }

    public async Task<ColumnDto?> GetColumnAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"columns/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ColumnDto>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ColumnDto?> CreateColumnAsync(CreateColumnDto createColumnDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(createColumnDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("columns", content);
            if (!response.IsSuccessStatusCode) return null;
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ColumnDto>(responseJson, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateColumnAsync(int id, UpdateColumnDto updateColumnDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(updateColumnDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"columns/{id}", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteColumnAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"columns/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReorderColumnAsync(int id, int newOrder)
    {
        try
        {
            var json = JsonSerializer.Serialize(newOrder, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"columns/{id}/reorder", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Tasks
    public async Task<IEnumerable<TaskItemDto>> GetTasksAsync(string? assignee = null, Priority? priority = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(assignee))
            queryParams.Add($"assignee={Uri.EscapeDataString(assignee)}");
        if (priority.HasValue)
            queryParams.Add($"priority={priority.Value}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        
        var response = await _httpClient.GetAsync($"tasks{queryString}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<TaskItemDto>>(json, _jsonOptions) ?? new List<TaskItemDto>();
    }

    public async Task<TaskItemDto?> GetTaskAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"tasks/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskItemDto>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<TaskItemDto?> CreateTaskAsync(CreateTaskItemDto createTaskDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(createTaskDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("tasks", content);
            if (!response.IsSuccessStatusCode) return null;
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskItemDto>(responseJson, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateTaskAsync(int id, UpdateTaskItemDto updateTaskDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(updateTaskDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"tasks/{id}", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"tasks/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> MoveTaskAsync(int id, MoveTaskItemDto moveTaskDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(moveTaskDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"tasks/{id}/move", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}