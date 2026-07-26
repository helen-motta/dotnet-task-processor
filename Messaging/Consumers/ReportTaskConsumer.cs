using TaskProcessor.Messaging.Messages;
using TaskProcessor.Settings;

namespace TaskProcessor.Messaging.Consumers;

public sealed class ReportTaskConsumer : TaskConsumerBase
{
    protected override string QueueName => RabbitMqSettings.ReportQueue;
    protected override string RoutingKey => RabbitMqSettings.ReportRoutingKey;
    protected override string ConsumerName => "Relatório";

    public ReportTaskConsumer(RabbitMqSettings settings, IServiceScopeFactory scopeFactory) : base(settings, scopeFactory)
    {
    }

    protected override async Task ProcessAsync(ProcessTaskMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Relatório] Processando...");
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);

        var sortedNumber = Random.Shared.Next(1, 11);
        if (sortedNumber <= 3)
        {
            throw new Exception($"[Relatório] Ocorreu um erro ao processar a task {message.TaskId}.");
        }

        Console.WriteLine($"[Relatório] Task {message.TaskId} concluída.");
    }
}