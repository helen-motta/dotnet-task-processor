using System.Text;
using RabbitMQ.Client;
using TaskProcessor.Settings;

namespace TaskProcessor.Messaging.Publishers;

public sealed class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqPublisher(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    public async Task PublishAsync(string message, CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        using var connection = await factory.CreateConnectionAsync(cancellationToken);

        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _settings.QueueName,
            body: body,
            cancellationToken: cancellationToken);

        Console.WriteLine($"Mensagem enviada: {message}");
    }
}