using System.Text;
using RabbitMQ.Client;
using TaskProcessor.Enums;
using TaskProcessor.Settings;
using TaskProcessor.Messaging.Messages;
using System.Text.Json;

namespace TaskProcessor.Messaging.Publishers;

public sealed class TaskPublisher : ITaskPublisher
{
    private readonly RabbitMqSettings _settings;

    public TaskPublisher(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    public async Task PublishAsync(ProcessTaskMessage message, CancellationToken cancellationToken = default)
    {
        var routingKey = message.Type switch
        {
            TaskType.EnviarEmail => RabbitMqSettings.EmailRoutingKey,

            TaskType.GerarRelatorio => RabbitMqSettings.ReportRoutingKey,

            _ => throw new ArgumentException("Tipo de tarefa inválido.")
        };

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        using var connection = await factory.CreateConnectionAsync(cancellationToken);

        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqSettings.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqSettings.Exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        Console.WriteLine(
            $"Mensagem enviada: {message.Type} - {message.Data}");
    }
}