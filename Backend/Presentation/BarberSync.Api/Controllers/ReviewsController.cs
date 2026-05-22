using BarberSync.Application.DTOs;
using BarberSync.Application.Services.Saas;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
public class ReviewsController(ReviewService reviewService) : ControllerBase
{
    [HttpPost("api/reviews")]
    public ActionResult<ReviewDto> CreateReview([FromBody] ReviewDto review) => Ok(reviewService.AddReview(review));

    [HttpPost("api/nps")]
    public ActionResult<NpsResponseDto> CreateNps([FromBody] NpsResponseDto nps) => Ok(reviewService.AddNps(nps));

    [HttpGet("api/reports/satisfaction/{tenantId:guid}")]
    public ActionResult<SatisfactionReportDto> Satisfaction(Guid tenantId) => Ok(reviewService.Report(tenantId));
}
