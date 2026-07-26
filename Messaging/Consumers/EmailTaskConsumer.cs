using TaskProcessor.Messaging.Messages;
using TaskProcessor.Settings;

namespace TaskProcessor.Messaging.Consumers;

public sealed class EmailTaskConsumer : TaskConsumerBase
{
    protected override string QueueName => RabbitMqSettings.EmailQueue;
    protected override string RoutingKey => RabbitMqSettings.EmailRoutingKey;
    protected override string ConsumerName => "E-mail";
    public EmailTaskConsumer(RabbitMqSettings settings, IServiceScopeFactory scopeFactory) : base(settings, scopeFactory)
    {
    }

    protected override async Task ProcessAsync(ProcessTaskMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[E-mail] Processando...");
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        var sortedNumber = Random.Shared.Next(1, 11);
        if (sortedNumber <= 1)
        {
            throw new Exception($"[E-mail] Ocorreu um erro ao processar a task {message.TaskId}.");
        }
        Console.WriteLine($"[E-mail] Task {message.TaskId} concluída.");
    }
}