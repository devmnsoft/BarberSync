using BarberSync.Application.Abstractions.Strategy;
using BarberSync.Application.DTOs.Strategy;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Strategy;

[ApiController]
public class StrategicGrowthController(IStrategicGrowthService service) : ControllerBase
{
    [HttpGet("/api/competitive/competitors")]
    public IActionResult GetCompetitors() => Ok(service.GetCompetitors());

    [HttpGet("/api/competitive/services")]
    public IActionResult GetCompetitiveServices() => Ok(service.GetPricingSuggestions());

    [HttpGet("/api/competitive/prices")]
    public IActionResult GetCompetitivePrices() => Ok(service.GetPricingSuggestions());

    [HttpGet("/api/competitive/reviews")]
    public IActionResult GetCompetitiveReviews() => Ok(service.GetCompetitors());

    [HttpGet("/api/competitive/analysis")]
    public IActionResult GetCompetitiveAnalysis() => Ok(service.GetDashboardSummary());

    [HttpGet("/api/competitive/opportunities")]
    public IActionResult GetCompetitiveOpportunities() => Ok(service.GetExpansionProjects());

    [HttpGet("/api/pricing/suggestions")]
    public IActionResult GetPricingSuggestions() => Ok(service.GetPricingSuggestions());

    [HttpGet("/api/suppliers/profiles")]
    public IActionResult GetSuppliers() => Ok(service.GetSuppliers());

    [HttpGet("/api/purchases/requests")]
    public IActionResult GetPurchaseRequests() => Ok(service.GetPurchaseRequests());

    [HttpGet("/api/expansion/projects")]
    public IActionResult GetExpansionProjects() => Ok(service.GetExpansionProjects());

    [HttpGet("/api/franchise/health-score")]
    public IActionResult GetFranchiseHealth() => Ok(service.GetFranchiseHealth());

    [HttpGet("/api/benchmark/rankings")]
    public IActionResult GetBenchmarkRankings() => Ok(service.GetBenchmarkRankings());

    [HttpGet("/api/strategic-dashboard/summary")]
    public IActionResult GetSummary() => Ok(service.GetDashboardSummary());

    [HttpPost("/api/copilot/strategy/ask")]
    public IActionResult Ask([FromBody] StrategyQuestionDto dto) => Ok(service.AskStrategy(dto));
}
