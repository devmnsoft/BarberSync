using BarberSync.Api.Models;
using BarberSync.Application.DTOs.Commercial;
using BarberSync.Application.Services.Commercial;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Commercial;

[ApiController]
[Route("api/crm/leads")]
public class CrmLeadsController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<CrmLeadDto>>> GetAll([FromQuery] Guid tenantId, [FromServices] CrmLeadService service)
        => Ok(ApiResponse<IReadOnlyList<CrmLeadDto>>.Ok(service.GetLeads(tenantId), "Leads carregados com sucesso."));

    [HttpGet("{id:guid}")]
    public ActionResult<ApiResponse<CrmLeadDto>> GetById(Guid id, [FromServices] CrmLeadService service)
        => service.GetById(id) is { } lead ? Ok(ApiResponse<CrmLeadDto>.Ok(lead, "Lead carregado com sucesso.")) : NotFound(ApiResponse<CrmLeadDto>.Fail("Lead não encontrado."));

    [HttpPost]
    public ActionResult<ApiResponse<CrmLeadDto>> Create([FromBody] CreateCrmLeadRequest request, [FromServices] CrmLeadService service) =>
        Ok(ApiResponse<CrmLeadDto>.Ok(service.CreateLead(request), "Lead salvo com sucesso."));

    [HttpPost("{id:guid}/assign")] public ActionResult<ApiResponse<CrmLeadDto>> Assign(Guid id, [FromBody] AssignLeadRequest req, [FromServices] CrmLeadService service)=>Ok(ApiResponse<CrmLeadDto>.Ok(service.Assign(id, req.AssignedToUserId), "Lead atualizado com sucesso."));
    [HttpPost("{id:guid}/status")] public ActionResult<ApiResponse<CrmLeadDto>> Status(Guid id, [FromBody] UpdateLeadStatusRequest req, [FromServices] CrmLeadService service)=>Ok(ApiResponse<CrmLeadDto>.Ok(service.UpdateStatus(id, req), "Lead atualizado com sucesso."));
    [HttpPost("{id:guid}/interactions")] public IActionResult Interaction(Guid id, [FromBody] LeadInteractionRequest req, [FromServices] CrmLeadService service){service.AddInteraction(id, req); return Ok(ApiResponse.Ok("Interação registrada com sucesso."));}
    [HttpPost("{id:guid}/notes")] public IActionResult Notes(Guid id, [FromBody] LeadNoteRequest req, [FromServices] CrmLeadService service){service.AddNote(id, req); return Ok(ApiResponse.Ok("Observação registrada com sucesso."));}
    [HttpPost("{id:guid}/convert-to-client")] public ActionResult<ApiResponse<CrmLeadDto>> Convert(Guid id, [FromBody] ConvertLeadRequest req, [FromServices] CrmLeadService service)=>Ok(ApiResponse<CrmLeadDto>.Ok(service.Convert(id, req.ClientId), "Lead convertido em cliente com sucesso."));
    [HttpPost("{id:guid}/mark-lost")] public ActionResult<ApiResponse<CrmLeadDto>> Lost(Guid id, [FromBody] MarkLostRequest req, [FromServices] CrmLeadService service)=>Ok(ApiResponse<CrmLeadDto>.Ok(service.UpdateStatus(id, new("LOST", req.Reason)), "Lead marcado como perdido."));
    [HttpPost("{id:guid}/reopen")] public ActionResult<ApiResponse<CrmLeadDto>> Reopen(Guid id, [FromBody] ReopenLeadRequest req, [FromServices] CrmLeadService service)=>Ok(ApiResponse<CrmLeadDto>.Ok(service.Reopen(id, req.Justification), "Lead reaberto com sucesso."));
    [HttpGet("duplicates")] public ActionResult<ApiResponse<IReadOnlyList<CrmLeadDto>>> Duplicates([FromQuery] Guid tenantId, [FromServices] CrmLeadService service)=>Ok(ApiResponse<IReadOnlyList<CrmLeadDto>>.Ok(service.Duplicates(tenantId), "Duplicidades consultadas com sucesso."));
    [HttpGet("hot")] public ActionResult<ApiResponse<IReadOnlyList<CrmLeadDto>>> Hot([FromQuery] Guid tenantId, [FromServices] CrmLeadService service)=>Ok(ApiResponse<IReadOnlyList<CrmLeadDto>>.Ok(service.HotLeads(tenantId), "Leads quentes carregados com sucesso."));
}
