using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(EnterpriseDataService data, ILogger<NotificationsController> logger) : ControllerBase
{
    private static readonly List<NotificationAlertDto> Queue = new();
    private static readonly List<IntelligentNotificationTriggerDto> Triggers = new();

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(new { success = true, message = "Notificações carregadas com dados reais.", data = await data.ListAsync("notifications", cancellationToken), errors = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar notificações reais.");
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var notification = await data.GetAsync("notifications", id, cancellationToken);
            return notification is null
                ? NotFound(new { success = false, message = "Notificação não encontrada.", data = (object?)null, errors = Array.Empty<object>() })
                : Ok(new { success = true, message = "Notificação carregada com sucesso.", data = notification, errors = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar notificação {NotificationId}.", id);
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(new { success = true, message = "Notificação criada com sucesso.", data = await data.CreateAsync("notifications", payload, cancellationToken), errors = Array.Empty<object>() });
        }
        catch (EnterpriseValidationException ex)
        {
            return BadRequest(new { success = false, message = "Existem campos inválidos.", data = (object?)null, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar notificação.");
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(new { success = true, message = "Notificação atualizada com sucesso.", data = await data.UpdateAsync("notifications", id, payload, cancellationToken), errors = Array.Empty<object>() });
        }
        catch (EnterpriseValidationException ex)
        {
            return BadRequest(new { success = false, message = "Existem campos inválidos.", data = (object?)null, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar notificação {NotificationId}.", id);
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await data.SoftDeleteAsync("notifications", id, cancellationToken);
            return Ok(new { success = true, message = "Notificação inativada com sucesso.", data = new { id }, errors = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inativar notificação {NotificationId}.", id);
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }

    [HttpPost("enqueue")]
    public ActionResult<NotificationAlertDto> Enqueue([FromBody] NotificationAlertDto dto)
    {
        dto.Status = "queued";
        Queue.Add(dto);
        return Accepted(dto);
    }

    [HttpPost("triggers")]
    public ActionResult<IntelligentNotificationTriggerDto> ConfigureTrigger([FromBody] IntelligentNotificationTriggerDto trigger)
    {
        Triggers.Add(trigger);
        return Ok(trigger);
    }

    [HttpGet("triggers")]
    public ActionResult<IEnumerable<IntelligentNotificationTriggerDto>> GetTriggers() => Ok(Triggers);

    [HttpGet("queue")]
    public ActionResult<IEnumerable<NotificationAlertDto>> GetQueue() => Ok(Queue.OrderBy(x => x.TriggerAtUtc));
}
