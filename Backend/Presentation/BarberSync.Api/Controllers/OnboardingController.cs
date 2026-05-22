using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/onboarding")]
public class OnboardingController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost("start")] public ActionResult<OnboardingStateDto> Start() => Ok(onboardingService.Start());
    [HttpPost("company/{tenantId:guid}")] public ActionResult<OnboardingStateDto> Company(Guid tenantId, [FromBody] CompanyDto company) => Ok(onboardingService.SetCompany(tenantId, company));
    [HttpPost("admin/{tenantId:guid}")] public ActionResult<OnboardingStateDto> Admin(Guid tenantId, [FromBody] UserDto admin) => Ok(onboardingService.SetAdmin(tenantId, admin.Name, admin.Email));
    [HttpPost("plan/{tenantId:guid}")] public ActionResult<OnboardingStateDto> Plan(Guid tenantId, [FromQuery] Guid planId) => Ok(onboardingService.SetPlan(tenantId, planId));
    [HttpPost("branding/{tenantId:guid}")] public ActionResult<OnboardingStateDto> Branding(Guid tenantId, [FromBody] CompanyBrandingDto branding) => Ok(onboardingService.SetBranding(tenantId, branding));
    [HttpPost("finish/{tenantId:guid}")] public ActionResult<OnboardingStateDto> Finish(Guid tenantId) => Ok(onboardingService.Finish(tenantId));
}
