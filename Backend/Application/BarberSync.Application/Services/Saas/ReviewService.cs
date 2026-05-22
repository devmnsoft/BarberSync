using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.Saas;

public class ReviewService(InMemorySaasStore store)
{
    public ReviewDto AddReview(ReviewDto review)
    {
        var created = review with { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        store.Reviews.Add(created);
        return created;
    }
    public NpsResponseDto AddNps(NpsResponseDto nps)
    {
        var created = nps with { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        store.NpsResponses.Add(created);
        return created;
    }
    public SatisfactionReportDto Report(Guid tenantId)
    {
        var reviews = store.Reviews.Where(x => x.TenantId == tenantId).ToList();
        var avg = reviews.Count == 0 ? 0 : reviews.Average(x => x.Rating);
        var npsList = store.NpsResponses.Where(x => x.TenantId == tenantId).ToList();
        var nps = npsList.Count == 0 ? 0 : npsList.Average(x => x.Score) * 10 - 50;
        return new SatisfactionReportDto((decimal)avg, (decimal)nps, ["Alex", "Bruno"], ["Corte", "Barba"], ["Atraso pontual"]);
    }
}
