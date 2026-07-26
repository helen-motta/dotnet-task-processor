using TaskProcessor.Models;

namespace TaskProcessor.Repositories;

public interface ITaskRepository
{
    Task CreateAsync(TaskModel taskModel);
    Task<List<TaskModel>> GetAllAsync();
    Task<TaskModel> GetByIdAsync(string id);
    Task UpdateStatusAsync(string taskId, Enums.TaskStatus status, CancellationToken cancellationToken = default);
}