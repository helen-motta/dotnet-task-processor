using TaskProcessor.Dtos;
using TaskProcessor.Messaging.Messages;
using TaskProcessor.Messaging.Publishers;
using TaskProcessor.Models;
using TaskProcessor.Repositories;

namespace TaskProcessor.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ITaskPublisher _publisher;


    public TaskService(ITaskRepository repository, ITaskPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
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

        await _repository.CreateAsync(taskModel);

        var message = new ProcessTaskMessage(
            TaskId: taskModel.Id,
            Type: taskModel.Type,
            Data: taskModel.Data);

        try
        {
            await _publisher.PublishAsync(message);
        }
        catch
        {
            await _repository.UpdateStatusAsync(taskModel.Id, Enums.TaskStatus.Failed);
            throw;
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
    public async Task UpdateTaskStatusAsync(string taskId, Enums.TaskStatus status, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateStatusAsync(taskId, status, cancellationToken);
    }

    public async Task<int?> TryPrepareTaskForRetryAsync(string taskId, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(taskId);

        if (task is null || task.RetryCount >= 3)
        {
            return null;
        }

        await _repository.UpdateStatusAsync(taskId, Enums.TaskStatus.Pending, cancellationToken);
        return await _repository.TryPrepareTaskForRetryAsync(taskId, cancellationToken);
    }
}