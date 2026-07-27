using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskProcessor.Messaging.Messages;
using TaskProcessor.Services;
using TaskProcessor.Settings;

namespace TaskProcessor.Messaging.Consumers;

public abstract class TaskConsumerBase : BackgroundService
{
    private readonly RabbitMqSettings _settings;

    protected abstract string QueueName { get; }
    protected abstract string RoutingKey { get; }
    protected abstract string ConsumerName { get; }
    private readonly IServiceScopeFactory _scopeFactory;

    protected TaskConsumerBase(RabbitMqSettings settings, IServiceScopeFactory scopeFactory)
    {
        _settings = settings;
        _scopeFactory = scopeFactory;
    }

    protected abstract Task ProcessAsync(ProcessTaskMessage message, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
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

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqSettings.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: RabbitMqSettings.Exchange,
            routingKey: RoutingKey,
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) => {
            var body = eventArgs.Body.ToArray();

        ProcessTaskMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<ProcessTaskMessage>(body);

            if (message is null)
            {
                throw new JsonException("O conteúdo da mensagem é nulo.");
            }

            if (string.IsNullOrWhiteSpace(message.TaskId) || message.Type is null || string.IsNullOrWhiteSpace(message.Data))
            {
                throw new JsonException("A mensagem não contém os campos obrigatórios.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[{ConsumerName}] Mensagem inválida: {ex.Message}");

            await channel.BasicNackAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: cancellationToken);

            return;
        }

        Console.WriteLine($"[{ConsumerName}] Task recebida: {message.TaskId}");

        using var scope = _scopeFactory.CreateScope();
        var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

        try
        {
            await taskService.UpdateTaskStatusAsync(message.TaskId, Enums.TaskStatus.InProgress, cancellationToken);
            await ProcessAsync(message, cancellationToken);
            await taskService.UpdateTaskStatusAsync(message.TaskId, Enums.TaskStatus.Completed, cancellationToken);
                
            await channel.BasicAckAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false,
            cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ConsumerName}] Erro ao processar a task {message.TaskId}: {ex.Message}");

            int? retryCount = await taskService.TryPrepareTaskForRetryAsync(message.TaskId, cancellationToken);

            if (retryCount.HasValue)
            {
                Console.WriteLine($"[{ConsumerName}] Retentativa {retryCount.Value}");

                await channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken);

                return;
            }

            await taskService.UpdateTaskStatusAsync(message.TaskId, Enums.TaskStatus.Failed, cancellationToken);

            await channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken);

            return;
        }};

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        Console.WriteLine($"[{ConsumerName}] Aguardando mensagens...");

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}