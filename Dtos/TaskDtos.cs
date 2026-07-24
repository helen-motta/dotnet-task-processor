using TaskProcessor.Enums;

namespace TaskProcessor.Dtos;

public class CreateTaskRequest
{
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

public class TaskResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}