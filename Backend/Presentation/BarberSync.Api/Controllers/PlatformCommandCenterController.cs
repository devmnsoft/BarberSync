using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/platform")]
public class PlatformCommandCenterController : ControllerBase
{
    [HttpGet("command-center")]
    public ActionResult<PlatformCommandCenterDto> GetCommandCenter()
    {
        var now = DateTime.UtcNow;

        var response = new PlatformCommandCenterDto(
            now,
            new KpiBoardDto(12490.32m, 87.4m, 74.1m, 31.8m, 12.2m),
            new[]
            {
                new AutomationQueueDto("smart-scheduling", 14, 3, 1),
                new AutomationQueueDto("payment-reconciliation", 8, 2, 0),
                new AutomationQueueDto("notifications-dispatch", 24, 4, 2)
            },
            new[]
            {
                new SecurityAlertDto("medium", "auth", "MFA challenge spike detected for admins", now.AddMinutes(-18)),
                new SecurityAlertDto("low", "audit", "New policy rule applied to stock module", now.AddMinutes(-42))
            },
            new[]
            {
                new AiRuntimeAlertDto("efficiency-drop", "Paulo Mendes", 0.72m, "Suggest retraining on fade workflow", now.AddMinutes(-12)),
                new AiRuntimeAlertDto("upsell-opportunity", "Renata Alves", 0.93m, "Offer premium hydration combo", now.AddMinutes(-4))
            },
            new[]
            {
                new EngagementActionDto("vip-clients", "whatsapp", "cashback-friday", "No visit in 20 days"),
                new EngagementActionDto("new-clients", "push", "return-in-7-days", "First appointment completed")
            });

        return Ok(response);
    }
}
