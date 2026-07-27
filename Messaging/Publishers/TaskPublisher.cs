using System.Text;
using RabbitMQ.Client;
using TaskProcessor.Enums;
using TaskProcessor.Settings;
using TaskProcessor.Messaging.Connections;
using TaskProcessor.Messaging.Messages;
using System.Text.Json;

namespace TaskProcessor.Messaging.Publishers;

public sealed class TaskPublisher : ITaskPublisher
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;

    public TaskPublisher(IRabbitMqConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task PublishAsync(ProcessTaskMessage message, CancellationToken cancellationToken = default)
    {
        var routingKey = message.Type switch
        {
            TaskType.EnviarEmail => RabbitMqSettings.EmailRoutingKey,

            TaskType.GerarRelatorio => RabbitMqSettings.ReportRoutingKey,

            _ => throw new ArgumentException("Tipo de tarefa inválido.")
        };

        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

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
