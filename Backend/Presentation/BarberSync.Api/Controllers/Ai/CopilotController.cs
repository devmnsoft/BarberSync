using BarberSync.Application.Abstractions.Ai;
using BarberSync.Application.DTOs.Ai;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Ai;

[ApiController]
[Route("api/copilot")]
public class CopilotController(ICopilotService copilotService) : ControllerBase
{
    [HttpGet("conversations")]
    public ActionResult<IReadOnlyCollection<CopilotConversationDto>> Conversations([FromQuery] Guid tenantId) => Ok(copilotService.GetConversations(tenantId));

    [HttpGet("messages")]
    public ActionResult<IReadOnlyCollection<CopilotMessageDto>> Messages([FromQuery] Guid conversationId) => Ok(copilotService.GetMessages(conversationId));

    [HttpPost("ask")]
    public ActionResult<CopilotAskResponseDto> Ask([FromBody] CopilotAskRequestDto request) => Ok(copilotService.Ask(request));

    [HttpGet("suggestions")]
    public ActionResult<IReadOnlyCollection<CopilotSuggestionDto>> Suggestions([FromQuery] Guid? tenantId)
    {
        var resolvedTenantId = tenantId.GetValueOrDefault(Guid.Parse("11111111-2222-3333-4444-555555555555"));
        var suggestions = copilotService.GetSuggestions(resolvedTenantId);
        if (suggestions.Count > 0)
        {
            return Ok(suggestions);
        }

        return Ok(new[]
        {
            new CopilotSuggestionDto(Guid.Parse("22222222-2222-2222-2222-222222222221"), resolvedTenantId, "retention", "Recuperar clientes inativos", "Criar campanha WhatsApp para clientes VIP sem visita nos últimos 30 dias.", "Alta", DateTime.UtcNow),
            new CopilotSuggestionDto(Guid.Parse("22222222-2222-2222-2222-222222222222"), resolvedTenantId, "stock", "Repor Pomada Modeladora", "Gerar pedido ao fornecedor para evitar ruptura no fim de semana.", "Crítica", DateTime.UtcNow),
            new CopilotSuggestionDto(Guid.Parse("22222222-2222-2222-2222-222222222223"), resolvedTenantId, "capacity", "Abrir agenda extra sexta", "Convidar profissional parceiro para elevar capacidade no pico 18h-20h.", "Média", DateTime.UtcNow)
        });
    }

    [HttpPost("actions")]
    public ActionResult<CopilotActionDto> Actions([FromBody] CopilotActionDto request) => Ok(copilotService.CreateAction(request));

    [HttpPost("feedback")]
    public ActionResult<CopilotFeedbackDto> Feedback([FromBody] CopilotFeedbackDto request) => Ok(copilotService.SubmitFeedback(request));
}
