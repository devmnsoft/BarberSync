using BarberSync.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(ILogger<ClientsController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        try
        {
            var data = new PagedResult<object>([], page, pageSize, 0);
            return Ok(ApiResponse<PagedResult<object>>.Ok(data, "Consulta realizada com sucesso.", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao listar clientes.");
            return StatusCode(500, ApiResponse<object>.Fail("Ocorreu um erro inesperado. Tente novamente ou contate o suporte.", traceId: HttpContext.TraceIdentifier));
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] object request)
    {
        try
        {
            return Ok(ApiResponse<object>.Ok(request, "Registro salvo com sucesso.", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar cliente.");
            return StatusCode(500, ApiResponse<object>.Fail("Ocorreu um erro inesperado. Tente novamente ou contate o suporte.", traceId: HttpContext.TraceIdentifier));
        }
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] object request) => Ok(ApiResponse<object>.Ok(request, "Registro atualizado com sucesso.", HttpContext.TraceIdentifier));

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id) => Ok(ApiResponse<object>.Ok(new { id }, "Registro removido com sucesso.", HttpContext.TraceIdentifier));
}
