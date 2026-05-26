using BarberSync.Application.Abstractions.Innovation;
using BarberSync.Application.DTOs;

namespace BarberSync.Infrastructure.Innovation;

public class InMemoryInnovationOrchestrator : IInnovationOrchestrator
{
    public InnovationProfessionalPerformanceDto GetProfessionalPerformance(Guid professionalId)
        => new()
        {
            ProfessionalId = professionalId,
            AverageServiceMinutes = 42,
            EfficiencyScore = 0.91m,
            PrecisionScore = 0.88m,
            CompletedServices = 126
        };

    public IEnumerable<UpsellRecommendationDto> GetUpsellRecommendations(Guid clientId)
        =>
        [
            new UpsellRecommendationDto
            {
                ClientId = clientId,
                RecommendedService = "Hidratação Premium",
                ConversionProbability = 0.74m,
                Reason = "cliente costuma comprar tratamentos após corte"
            },
            new UpsellRecommendationDto
            {
                ClientId = clientId,
                RecommendedService = "Pigmentação de Barba",
                ConversionProbability = 0.61m,
                Reason = "perfil com alta recorrência de serviços de barba"
            }
        ];

    public SmartScheduleSuggestionDto SuggestAppointmentSlot(Guid clientId, Guid serviceId)
    {
        var start = DateTime.UtcNow.Date.AddHours(14);
        return new SmartScheduleSuggestionDto
        {
            ClientId = clientId,
            ServiceId = serviceId,
            SuggestedStartUtc = start,
            SuggestedEndUtc = start.AddMinutes(50),
            Explanation = "janela com menor fila e maior aderência ao histórico do cliente"
        };
    }

    public IEnumerable<SmartAlertDto> GetSmartAlerts()
        =>
        [
            new SmartAlertDto { Type = "stock", Severity = "high", Message = "Estoque crítico de navalhas descartáveis" },
            new SmartAlertDto { Type = "schedule", Severity = "medium", Message = "Profissional João com sobrecarga prevista às 18:00" },
            new SmartAlertDto { Type = "promotion", Severity = "info", Message = "Grupo VIP apto para campanha de cashback hoje" }
        ];
}
