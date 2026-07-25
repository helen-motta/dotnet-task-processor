using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskProcessor.Settings;

namespace TaskProcessor.Messaging.Consumers;

public sealed class ProcessTaskConsumer : BackgroundService
{
    private readonly RabbitMqSettings _settings;

    public ProcessTaskConsumer(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);

        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Mensagem recebida: {message}");
            Console.WriteLine("Processando...");

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

            Console.WriteLine("Processamento concluído.");

            await channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: stoppingToken);
        };

        await channel.BasicConsumeAsync(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        Console.WriteLine("Consumer aguardando mensagens...");

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}