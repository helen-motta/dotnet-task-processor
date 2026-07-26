using TaskProcessor.Enums;

namespace TaskProcessor.Models;

public class TaskModel
{
    public string Id { get; set; } = string.Empty;
    public TaskType? Type { get; set; } = null;
    public string Data { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}