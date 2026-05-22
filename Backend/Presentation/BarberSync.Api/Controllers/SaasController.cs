using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/saas")]
public class SaasController(ISaasService saasService) : ControllerBase
{
    [HttpGet("plans")] public ActionResult<IReadOnlyList<SubscriptionPlanDto>> Plans() => Ok(saasService.GetPlans());
    [HttpGet("subscriptions")] public ActionResult<IReadOnlyList<SubscriptionDto>> Subscriptions([FromQuery] Guid? tenantId) => Ok(saasService.GetSubscriptions(tenantId));
    [HttpGet("usage/{tenantId:guid}")] public ActionResult<TenantUsageDto> Usage(Guid tenantId) => Ok(saasService.GetUsage(tenantId));
    [HttpGet("invoices/{tenantId:guid}")] public ActionResult<IReadOnlyList<InvoiceDto>> Invoices(Guid tenantId) => Ok(saasService.GetInvoices(tenantId));
}
