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
    public ActionResult<IReadOnlyCollection<CopilotSuggestionDto>> Suggestions([FromQuery] string? tenantId)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId) || parsedTenantId == Guid.Empty)
        {
            Response.Headers["X-Trace-Id"] = HttpContext.TraceIdentifier;
            return BadRequest(new
            {
                message = "tenantId é obrigatório e deve ser um UUID válido e não vazio.",
                traceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(copilotService.GetSuggestions(parsedTenantId));
    }

    [HttpPost("actions")]
    public ActionResult<CopilotActionDto> Actions([FromBody] CopilotActionDto request) => Ok(copilotService.CreateAction(request));

    [HttpPost("feedback")]
    public ActionResult<CopilotFeedbackDto> Feedback([FromBody] CopilotFeedbackDto request) => Ok(copilotService.SubmitFeedback(request));
}
