using Microsoft.AspNetCore.Mvc;

namespace case_f360.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromBody] HelloRequest request)
    {
        var resposta = new
        {
            mensagem = $"Hello, {request.Nome}!"
        };

        return Ok(resposta);
    }
}

public record HelloRequest(string Nome);