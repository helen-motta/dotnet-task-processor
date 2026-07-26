namespace TaskProcessor.Settings;

public sealed class RabbitMqSettings
{
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string QueueName { get; init; } = "task_queue_test";
    public const string Exchange = "tasks_exchange";
    public const string EmailQueue = "email_tasks_queue";
    public const string ReportQueue = "report_tasks_queue";
    public const string EmailRoutingKey = "task.email";
    public const string ReportRoutingKey = "task.report";
}