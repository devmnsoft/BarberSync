using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions.Innovation;

public interface IInnovationOrchestrator
{
    InnovationProfessionalPerformanceDto GetProfessionalPerformance(Guid professionalId);
    IEnumerable<UpsellRecommendationDto> GetUpsellRecommendations(Guid clientId);
    SmartScheduleSuggestionDto SuggestAppointmentSlot(Guid clientId, Guid serviceId);
    IEnumerable<SmartAlertDto> GetSmartAlerts();
}
