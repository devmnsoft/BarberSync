using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions.Saas;

public interface IOnboardingService
{
    OnboardingStateDto Start();
    OnboardingStateDto SetCompany(Guid tenantId, CompanyDto company);
    OnboardingStateDto SetAdmin(Guid tenantId, string name, string email);
    OnboardingStateDto SetPlan(Guid tenantId, Guid planId);
    OnboardingStateDto SetBranding(Guid tenantId, CompanyBrandingDto branding);
    OnboardingStateDto Finish(Guid tenantId);
}
