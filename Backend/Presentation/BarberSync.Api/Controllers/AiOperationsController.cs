using System.Text;
using System.Text.Json;
using BarberSync.Api.Services.Recognition;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

/// <summary>Operational-signal recognition. It never identifies a person and never charges automatically.</summary>
[ApiController, Authorize(Roles = "Owner,SuperAdmin,Admin,Manager,Reception")]
[Route("api/ai-operations")]
public sealed class AiOperationsController(IServiceRecognitionService service, IAiProvider provider, ICurrentUserContext user) : ControllerBase
{
    public sealed record DetectionRequest(Guid CameraDeviceId, Guid? ZoneId, Guid? AppointmentId, Guid? ProfessionalId, string[] Signals, bool PrivacyNoticeAccepted);
    public sealed record ReviewRequest(Guid? ServiceId, Guid? ServiceOrderId, Guid? ClientId, Guid? ProfessionalId, bool CreatePreOrder, string? Reason);

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var cameras = await List("camera_devices", ct); var events = await List("service_recognition_events", ct); var suggestions = await List("service_recognition_suggestions", ct);
        static string Status(Dictionary<string, object?> x) => Convert.ToString(x.GetValueOrDefault("status")) ?? "";
        var pending = suggestions.Count(x => Status(x).Equals("Pending", StringComparison.OrdinalIgnoreCase));
        var approved = suggestions.Count(x => Status(x).Equals("Confirmed", StringComparison.OrdinalIgnoreCase));
        var rejected = suggestions.Count(x => Status(x).Equals("Rejected", StringComparison.OrdinalIgnoreCase));
        return Ok(new { activeCameras = cameras.Count(x => !Equals(x.GetValueOrDefault("is_active"), false)), inactiveCameras = cameras.Count(x => Equals(x.GetValueOrDefault("is_active"), false)), openSessions = events.Count(x => Status(x).Equals("Pending", StringComparison.OrdinalIgnoreCase)), pendingSuggestions = pending, approvedSuggestions = approved, rejectedSuggestions = rejected, correctedSuggestions = 0, approvalRate = suggestions.Count == 0 ? 0 : Math.Round(approved * 100m / suggestions.Count, 2), correctionRate = 0, provider = provider.Name, providerStatus = provider.IsConfigured ? "Configured" : "NotConfigured", requiresHumanReview = true, automaticCharge = false });
    }

    [HttpGet("cameras")] public Task<IReadOnlyList<Dictionary<string, object?>>> Cameras(CancellationToken ct) => List("camera_devices", ct);
    [HttpPost("cameras")] public Task<Dictionary<string, object?>> CreateCamera([FromBody] JsonElement body, CancellationToken ct) => Create("camera_devices", body, ct);
    [HttpPut("cameras/{id:guid}")] public IActionResult UpdateCamera(Guid id) => Conflict(Error("Alterações de câmera exigem novo cadastro auditável; inative o dispositivo anterior."));
    [HttpGet("zones")] public Task<IReadOnlyList<Dictionary<string, object?>>> Zones(CancellationToken ct) => List("service_stations", ct);
    [HttpPost("zones")] public Task<Dictionary<string, object?>> CreateZone([FromBody] JsonElement body, CancellationToken ct) => Create("service_stations", body, ct);
    [HttpPut("zones/{id:guid}")] public IActionResult UpdateZone(Guid id) => Conflict(Error("Alterações de zona exigem novo cadastro auditável."));
    [HttpGet("signal-rules")] public Task<IReadOnlyList<Dictionary<string, object?>>> Rules(CancellationToken ct) => List("recognition_rules", ct);
    [HttpPost("signal-rules")] public Task<Dictionary<string, object?>> CreateRule([FromBody] JsonElement body, CancellationToken ct) => Create("recognition_rules", body, ct);
    [HttpPut("signal-rules/{id:guid}")] public IActionResult UpdateRule(Guid id) => Conflict(Error("Alterações de regra exigem nova versão auditável."));
    [HttpGet("review-queue")] public async Task<IReadOnlyList<Dictionary<string, object?>>> Queue(CancellationToken ct) => (await List("service_recognition_suggestions", ct)).Where(x => Convert.ToString(x.GetValueOrDefault("status")) == "Pending").ToArray();
    [HttpGet("suggestions")] public Task<IReadOnlyList<Dictionary<string, object?>>> Suggestions(CancellationToken ct) => List("service_recognition_suggestions", ct);
    [HttpGet("evidence")] public Task<IReadOnlyList<Dictionary<string, object?>>> Evidence(CancellationToken ct) => List("service_recognition_events", ct);

    [HttpPost("detection-events")]
    public async Task<IActionResult> Detect([FromBody] DetectionRequest request, CancellationToken ct)
    {
        if (!request.PrivacyNoticeAccepted) return UnprocessableEntity(Error("Confirme o aviso de privacidade. Imagens, rostos e biometria não são processados."));
        if (request.Signals is not { Length: > 0 }) return BadRequest(Error("Selecione ao menos um sinal operacional."));
        if (!provider.IsConfigured) return StatusCode(StatusCodes.Status503ServiceUnavailable, Error("ProviderNotConfigured: nenhum evento foi processado e nenhuma sugestão foi criada."));
        var suggestion = await service.RecordEventAsync(new(Guid.NewGuid(), user.TenantId, user.BranchId, request.CameraDeviceId, request.ZoneId, request.AppointmentId, request.ProfessionalId, DateTimeOffset.UtcNow, request.Signals), ct);
        return Accepted(new { suggestion, status = "PendingReview", requiresHumanReview = true, automaticCharge = false });
    }

    [HttpPost("suggestions/{id:guid}/approve")] public Task<IActionResult> Approve(Guid id, [FromBody] ReviewRequest request, CancellationToken ct) => Review(id, request, true, false, ct);
    [HttpPost("suggestions/{id:guid}/correct")] public Task<IActionResult> Correct(Guid id, [FromBody] ReviewRequest request, CancellationToken ct) => Review(id, request, true, true, ct);
    [HttpPost("suggestions/{id:guid}/reject")] public Task<IActionResult> Reject(Guid id, [FromBody] ReviewRequest request, CancellationToken ct) => Review(id, request, false, false, ct);

    [HttpGet("settings")] public IActionResult Settings() => Ok(new { provider = provider.Name, status = provider.IsConfigured ? "Configured" : "NotConfigured", privacyMode = "MetadataOnly", humanReviewRequired = true, faceRecognition = false, biometricIdentification = false, automaticCharge = false });
    [HttpPut("settings")] public IActionResult UpdateSettings([FromBody] JsonElement body) => UnprocessableEntity(Error("Configuração de provider requer credenciais seguras no ambiente; nenhuma configuração foi simulada."));

    [HttpGet("reports/export")]
    public async Task<IActionResult> Export([FromQuery] string type, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(type) || from > to) return BadRequest(Error("Selecione o tipo e um período válido (início menor ou igual ao fim)."));
        var rows = await List("service_recognition_suggestions", ct); var csv = new StringBuilder("status,confidence,createdAt\n");
        foreach (var row in rows) csv.Append(Csv(row.GetValueOrDefault("status"))).Append(',').Append(Csv(row.GetValueOrDefault("confidence"))).Append(',').Append(Csv(row.GetValueOrDefault("created_at"))).AppendLine();
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", $"ai-operations-{type}-{from}-{to}.csv");
    }

    private async Task<IActionResult> Review(Guid id, ReviewRequest r, bool approve, bool correction, CancellationToken ct)
    {
        if ((!approve || correction) && string.IsNullOrWhiteSpace(r.Reason)) return BadRequest(Error("O motivo é obrigatório para rejeição ou correção."));
        if (correction && r.ServiceId is null) return BadRequest(Error("Selecione o serviço correto no catálogo."));
        var decision = new RecognitionDecision(id, r.ServiceId, r.ServiceOrderId, r.ClientId, r.ProfessionalId, r.CreatePreOrder, r.Reason);
        var order = await service.DecideAsync(user.TenantId, user.BranchId, user.UserId, decision, approve, HttpContext.TraceIdentifier, ct);
        return Ok(new { status = correction ? "Corrected" : approve ? "Approved" : "Rejected", serviceOrderId = order, automaticCharge = false, paymentStillRequiresConfirmation = true });
    }
    private Task<IReadOnlyList<Dictionary<string, object?>>> List(string resource, CancellationToken ct) => service.ListAsync(user.TenantId, user.BranchId, resource, ct);
    private Task<Dictionary<string, object?>> Create(string resource, JsonElement body, CancellationToken ct) => service.CreateAsync(user.TenantId, user.BranchId, resource, body, ct);
    private object Error(string message) => new { message, traceId = HttpContext.TraceIdentifier };
    private static string Csv(object? value) => $"\"{Convert.ToString(value)?.Replace("\"", "\"\"")}\"";
}
