using Microsoft.AspNetCore.Mvc;
using TaskProcessor.Dtos;
using TaskProcessor.Services;

namespace TaskProcessor.Controllers;

public class TaskProcessorController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskProcessorController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost("api/tasks")]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskRequest request)
    {
        var response = await _taskService.CreateAsync(request);

        return Ok(response);
    }

    [HttpGet("api/tasks")]
    public async Task<List<TaskResponse>> GetTasks()
    {
        return await _taskService.GetAllAsync();
    }

    [HttpGet("api/tasks/{id}")]
    public async Task<TaskResponse> GetTaskById(string id)
    {
        var task = await _taskService.GetByIdAsync(id);

        return task ?? throw new Exception($"Task with ID {id} not found.");
    }
}