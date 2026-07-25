namespace TaskProcessor.Messaging.Publishers;

public interface IRabbitMqPublisher
{
    Task PublishAsync(
        string message,
        CancellationToken cancellationToken = default);
}