using Microsoft.AspNetCore.Mvc;
using TaskProcessor.Messaging.Publishers;

namespace TaskProcessor.Controllers;

[ApiController]
[Route("api/test/rabbitmq")]
public class RabbitMqTestController : ControllerBase
{
    private readonly IRabbitMqPublisher _publisher;

    public RabbitMqTestController(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> Publish(
        [FromBody] PublishMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new
            {
                message = "A mensagem é obrigatória."
            });
        }

        await _publisher.PublishAsync(
            request.Message,
            cancellationToken);

        return Accepted(new
        {
            message = "Mensagem enviada para o RabbitMQ.",
            content = request.Message
        });
    }
}

public sealed record PublishMessageRequest(string Message);