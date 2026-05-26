using BarberSync.Api.Models;
using BarberSync.Api.Models.Recognition;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/recognition")]
public class RecognitionController : ControllerBase
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    [HttpPost("detect")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Detect([FromForm] ServiceRecognitionUploadRequest request, CancellationToken cancellationToken)
    {
        if (request.Image is null || request.Image.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Envie uma imagem válida.", ["A imagem é obrigatória."], HttpContext.TraceIdentifier));

        if (!AllowedContentTypes.Contains(request.Image.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<object>.Fail("Formato de imagem inválido.", ["Use JPG, PNG ou WEBP."], HttpContext.TraceIdentifier));

        var result = new
        {
            serviceDetected = "Corte Masculino",
            confidence = 0.87,
            imageFileName = request.Image.FileName,
            imageSize = request.Image.Length,
            appointmentId = request.AppointmentId,
            clientId = request.ClientId,
            professionalId = request.ProfessionalId,
            tenantId = request.TenantId,
            branchId = request.BranchId,
        };

        await Task.CompletedTask;

        return Ok(ApiResponse<object>.Ok(result, "Serviço reconhecido com sucesso.", HttpContext.TraceIdentifier));
    }
}
