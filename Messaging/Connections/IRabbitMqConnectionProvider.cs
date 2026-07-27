using RabbitMQ.Client;

namespace TaskProcessor.Messaging.Connections;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
