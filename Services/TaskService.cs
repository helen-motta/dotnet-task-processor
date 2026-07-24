using TaskProcessor.Dtos;
using TaskProcessor.Enums;
using TaskProcessor.Models;
using TaskProcessor.Repositories;

namespace TaskProcessor.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaskResponse> CreateAsync(
        CreateTaskRequest request)
    {
        var taskModel = new TaskModel
        {
            Id = Guid.NewGuid().ToString(),
            Type = request.Type,
            Data = request.Data,
            Status = Enums.TaskStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(
            taskModel);

        return new TaskResponse
        {
            Id = taskModel.Id,
            Type = taskModel.Type,
            Data = taskModel.Data,
            Status = taskModel.Status,
            RetryCount = taskModel.RetryCount,
            CreatedAt = taskModel.CreatedAt
        };
    }

    public async Task<List<TaskResponse>> GetAllAsync()
    {
        var taskResponse = await _repository.GetAllAsync();

        List<TaskResponse> TaskResponses = taskResponse.Select(taskModel => new TaskResponse
        {
            Id = taskModel.Id,
            Type = taskModel.Type,
            Data = taskModel.Data,
            Status = taskModel.Status,
            RetryCount = taskModel.RetryCount,
            CreatedAt = taskModel.CreatedAt
        }).ToList();

        return TaskResponses;
    }

    public async Task<TaskResponse?> GetByIdAsync(string id)
    {
        var taskModel = await _repository.GetByIdAsync(id);

        if (taskModel == null)
        {
            return null;
        }

        return new TaskResponse
        {
            Id = taskModel.Id,
            Type = taskModel.Type,
            Data = taskModel.Data,
            Status = taskModel.Status,
            RetryCount = taskModel.RetryCount,
            CreatedAt = taskModel.CreatedAt
        };
    }
}