using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskProcessor.Messaging.Messages;
using TaskProcessor.Services;
using TaskProcessor.Settings;

namespace TaskProcessor.Messaging.Consumers;

public sealed class ReportTaskConsumer : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportTaskConsumer(RabbitMqSettings settings, IServiceScopeFactory scopeFactory)
    {
        _settings = settings;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqSettings.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: RabbitMqSettings.ReportQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: RabbitMqSettings.ReportQueue,
            exchange: RabbitMqSettings.Exchange,
            routingKey: RabbitMqSettings.ReportRoutingKey,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var message =
                JsonSerializer.Deserialize<ProcessTaskMessage>(json)
                ?? throw new InvalidOperationException(
                    "Não foi possível interpretar a mensagem.");

            using var scope = _scopeFactory.CreateScope();

            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

            try
            {
                await taskService.UpdateTaskStatusAsync(message.TaskId, Enums.TaskStatus.InProgress, cancellationToken);

                Console.WriteLine($"Processando task de relatório {message.TaskId}...");

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                await taskService.UpdateTaskStatusAsync(message.TaskId, Enums.TaskStatus.Completed, cancellationToken);

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: CancellationToken.None);

                Console.WriteLine($"Task de relatório {message.TaskId} concluída.");
            }
            catch (Exception exception)
            {
                await taskService.UpdateTaskStatusAsync(message.TaskId, Enums.TaskStatus.Failed, CancellationToken.None);

                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: CancellationToken.None);

                Console.WriteLine($"Erro na task {message.TaskId}: {exception.Message}");
            }
        };

        await channel.BasicConsumeAsync(
            queue: RabbitMqSettings.ReportQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        Console.WriteLine(
            $"Report consumer aguardando mensagens em {RabbitMqSettings.ReportQueue}...");
        
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}
