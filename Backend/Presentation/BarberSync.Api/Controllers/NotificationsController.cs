using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private static readonly List<NotificationAlertDto> Queue = new();
    private static readonly List<IntelligentNotificationTriggerDto> Triggers = new();

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
