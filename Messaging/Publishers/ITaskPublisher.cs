using TaskProcessor.Messaging.Messages;

namespace TaskProcessor.Messaging.Publishers;

public interface ITaskPublisher
{
    Task PublishAsync(ProcessTaskMessage message, CancellationToken cancellationToken = default);
}