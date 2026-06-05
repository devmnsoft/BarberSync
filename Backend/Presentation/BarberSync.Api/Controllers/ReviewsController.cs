using BarberSync.Application.DTOs;
using BarberSync.Application.Services.Saas;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
public class ReviewsController(ReviewService reviewService) : ControllerBase
{
    [HttpGet("api/reviews")]
    public ActionResult<object> GetReviews() => Ok(new { success = true, message = "Avaliações carregadas com sucesso.", data = new[] { new { id = "rev-001", client = "Lucas Almeida", rating = 5, nps = 10, comment = "Experiência premium e pontual.", status = "Publicado" }, new { id = "rev-002", client = "Marina Costa", rating = 5, nps = 10, comment = "Totem simples e atendimento excelente.", status = "Publicado" } }, isDemo = true });

    [HttpPost("api/reviews")]
    public ActionResult<ReviewDto> CreateReview([FromBody] ReviewDto review) => Ok(reviewService.AddReview(review));

    [HttpPost("api/nps")]
    public ActionResult<NpsResponseDto> CreateNps([FromBody] NpsResponseDto nps) => Ok(reviewService.AddNps(nps));

    [HttpGet("api/reports/satisfaction/{tenantId:guid}")]
    public ActionResult<SatisfactionReportDto> Satisfaction(Guid tenantId) => Ok(reviewService.Report(tenantId));
}
