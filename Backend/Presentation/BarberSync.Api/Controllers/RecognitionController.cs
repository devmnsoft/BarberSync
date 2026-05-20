using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/recognition")]
public class RecognitionController : ControllerBase
{
    [HttpPost]
    public IActionResult Detect([FromForm] IFormFile image)
    {
        return Ok(new { service = "Corte Masculino", confidence = 0.89m, estimatedPrice = 45.00m });
    }
}
