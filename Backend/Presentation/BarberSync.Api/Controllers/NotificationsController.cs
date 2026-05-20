using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private static readonly List<NotificationAlertDto> Queue = new();

    [HttpPost("enqueue")]
    public ActionResult<NotificationAlertDto> Enqueue([FromBody] NotificationAlertDto dto)
    {
        dto.Status = "queued";
        Queue.Add(dto);
        return Accepted(dto);
    }

    [HttpGet("queue")]
    public ActionResult<IEnumerable<NotificationAlertDto>> GetQueue() => Ok(Queue.OrderBy(x => x.TriggerAtUtc));
}
