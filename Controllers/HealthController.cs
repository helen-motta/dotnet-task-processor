using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TaskProcessor.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IMongoClient _mongoClient;

    public HealthController(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    [HttpGet("mongodb")]
    public async Task<IActionResult> VerificarMongoDb(
        CancellationToken cancellationToken)
    {
        try
        {
            var database = _mongoClient.GetDatabase("admin");

            var resultado =
                await database.RunCommandAsync<BsonDocument>(
                    new BsonDocument("ping", 1),
                    cancellationToken: cancellationToken);

            return Ok(new
            {
                status = "conectado",
                mongoDb = resultado["ok"].ToDouble()
            });
        }
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    status = "desconectado",
                    erro = exception.Message
                });
        }
    }
}