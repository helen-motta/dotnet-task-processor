namespace TaskProcessor.Settings;

public sealed class RabbitMqSettings
{
    public required string HostName { get; init; }
    public required int Port { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string QueueName { get; init; }
    public const string Exchange = "tasks_exchange";
    public const string EmailQueue = "email_tasks_queue";
    public const string ReportQueue = "report_tasks_queue";
    public const string EmailRoutingKey = "task.email";
    public const string ReportRoutingKey = "task.report";
}