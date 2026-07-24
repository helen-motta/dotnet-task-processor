using TaskProcessor.Dtos;
using TaskProcessor.Models;

namespace TaskProcessor.Services;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request);
    Task<List<TaskResponse>> GetAllAsync();
    Task<TaskResponse?> GetByIdAsync(string id);
}