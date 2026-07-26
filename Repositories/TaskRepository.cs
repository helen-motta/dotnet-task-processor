using MongoDB.Driver;
using TaskProcessor.Models;

namespace TaskProcessor.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly IMongoCollection<TaskModel> _collection;

    public TaskRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<TaskModel>("tasks");
    }

    public async Task CreateAsync(TaskModel taskModel)
    {
        await _collection.InsertOneAsync(taskModel);
    }

    public async Task UpdateStatusAsync(string taskId, Enums.TaskStatus status, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TaskModel>.Filter.Eq(t => t.Id, taskId);
        var update = Builders<TaskModel>.Update.Set(t => t.Status, status);
        await _collection.UpdateOneAsync(filter, update, new UpdateOptions(), cancellationToken);
    }

    public async Task<List<TaskModel>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<TaskModel> GetByIdAsync(string id)
    {
        return await _collection.Find(task => task.Id == id).FirstOrDefaultAsync();
    }
}