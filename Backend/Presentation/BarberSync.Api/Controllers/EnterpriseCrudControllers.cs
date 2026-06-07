using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

public abstract class EnterpriseCrudController(EnterpriseDataService data, ILogger logger, string resource) : ControllerBase
{
    protected Task<IActionResult> List(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.ListAsync(resource, cancellationToken), "Consulta realizada com sucesso.")));

    protected Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) => Safe(async () =>
    {
        var item = await data.GetAsync(resource, id, cancellationToken);
        return item is null ? NotFound(Envelope(null, "Registro não encontrado.", false)) : Ok(Envelope(item, "Registro carregado com sucesso."));
    });

    protected Task<IActionResult> Create(JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync(resource, payload, cancellationToken), "Operação realizada com sucesso.")));

    protected Task<IActionResult> Update(Guid id, JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.UpdateAsync(resource, id, payload, cancellationToken), "Operação realizada com sucesso.")));

    protected Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) => Safe(async () => { await data.SoftDeleteAsync(resource, id, cancellationToken); return Ok(Envelope(new { id }, "Registro inativado com sucesso.")); });

    protected async Task<IActionResult> Safe(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (EnterpriseValidationException ex)
        {
            return BadRequest(new { success = false, message = "Existem campos inválidos.", data = (object?)null, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar recurso {Resource}.", resource);
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }

    protected static object Envelope(object? data, string message, bool success = true) => new { success, message, data, errors = Array.Empty<object>() };
}
