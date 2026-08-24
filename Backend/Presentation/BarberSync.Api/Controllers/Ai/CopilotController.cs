using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Ai;
using BarberSync.Application.DTOs.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Ai;

[ApiController, Authorize]
[Route("api/copilot")]
public class CopilotController(ICopilotService copilotService, ICurrentUserContext currentUser) : ControllerBase
{
    [HttpGet("conversations")]
    public ActionResult<IReadOnlyCollection<CopilotConversationDto>> Conversations([FromQuery] Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return ScopeError("tenantId é obrigatório e deve ser um UUID válido e não vazio.");
        if (tenantId != currentUser.TenantId)
            return ScopeError("tenantId não corresponde ao tenant da sessão.", StatusCodes.Status403Forbidden);

        return Ok(copilotService.GetConversations(currentUser.TenantId));
    }

    [HttpGet("messages")]
    public ActionResult<IReadOnlyCollection<CopilotMessageDto>> Messages([FromQuery] Guid conversationId)
    {
        if (conversationId == Guid.Empty)
            return ScopeError("conversationId é obrigatório e deve ser um UUID válido e não vazio.");
        if (!OwnsConversation(conversationId))
            return ScopeError("Conversa não encontrada para o tenant da sessão.", StatusCodes.Status404NotFound);

        return Ok(copilotService.GetMessages(conversationId));
    }

    [HttpPost("ask")]
    public ActionResult<CopilotAskResponseDto> Ask([FromBody] CopilotAskRequestDto request)
    {
        if (request.TenantId == Guid.Empty)
            return ScopeError("tenantId é obrigatório e deve ser um UUID válido e não vazio.");
        if (request.TenantId != currentUser.TenantId)
            return ScopeError("tenantId não corresponde ao tenant da sessão.", StatusCodes.Status403Forbidden);
        if (request.ConversationId is { } conversationId && !OwnsConversation(conversationId))
            return ScopeError("Conversa não encontrada para o tenant da sessão.", StatusCodes.Status404NotFound);

        return Ok(copilotService.Ask(request));
    }

    [HttpGet("suggestions")]
    public ActionResult<IReadOnlyCollection<CopilotSuggestionDto>> Suggestions([FromQuery] string? tenantId)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId) || parsedTenantId == Guid.Empty)
            return ScopeError("tenantId é obrigatório e deve ser um UUID válido e não vazio.");
        if (parsedTenantId != currentUser.TenantId)
            return ScopeError("tenantId não corresponde ao tenant da sessão.", StatusCodes.Status403Forbidden);

        return Ok(copilotService.GetSuggestions(currentUser.TenantId));
    }

    [HttpPost("actions")]
    public ActionResult<CopilotActionDto> Actions([FromBody] CopilotActionDto request)
    {
        if (request.TenantId == Guid.Empty)
            return ScopeError("tenantId é obrigatório e deve ser um UUID válido e não vazio.");
        if (request.TenantId != currentUser.TenantId)
            return ScopeError("tenantId não corresponde ao tenant da sessão.", StatusCodes.Status403Forbidden);

        return Ok(copilotService.CreateAction(request));
    }

    [HttpPost("feedback")]
    public ActionResult<CopilotFeedbackDto> Feedback([FromBody] CopilotFeedbackDto request)
    {
        if (request.TenantId == Guid.Empty)
            return ScopeError("tenantId é obrigatório e deve ser um UUID válido e não vazio.");
        if (request.TenantId != currentUser.TenantId)
            return ScopeError("tenantId não corresponde ao tenant da sessão.", StatusCodes.Status403Forbidden);

        return Ok(copilotService.SubmitFeedback(request));
    }

    private bool OwnsConversation(Guid conversationId) =>
        copilotService.GetConversations(currentUser.TenantId).Any(conversation => conversation.Id == conversationId);

    private ObjectResult ScopeError(string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        Response.Headers["X-Trace-Id"] = HttpContext.TraceIdentifier;
        return StatusCode(statusCode, new { message, traceId = HttpContext.TraceIdentifier });
    }
}
