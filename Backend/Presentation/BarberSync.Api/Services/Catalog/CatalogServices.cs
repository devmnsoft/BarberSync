using System.Data.Common;
using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Team;

namespace BarberSync.Api.Services.Catalog;

public sealed record CatalogRule(Guid Id, string Name, string AdjustmentType, decimal? Amount, decimal? Percent, int Priority);
public sealed record CatalogPriceRequest(Guid ItemId, string ItemType, decimal BasePrice, decimal EstimatedCost, IReadOnlyList<CatalogRule>? Rules);
public sealed record CatalogPriceBreakdown(decimal BasePrice, IReadOnlyList<CatalogRule> AppliedRules, decimal DiscountAmount, decimal IncreaseAmount, decimal FinalPrice, decimal EstimatedCost, decimal MarginPercent, IReadOnlyList<string> Warnings, bool RequiresApproval, string SourceStatus);
public sealed record CatalogCommissionRequest(Guid OwnerId, Guid TargetId, string TargetType, string CommissionType, decimal BaseAmount, decimal? Amount, decimal? Percent, string SourceStatus);
public sealed record CatalogCommissionBreakdown(decimal BaseAmount, decimal CommissionAmount, string TriggerStatus, IReadOnlyList<string> Warnings, bool Eligible, string SourceStatus);

public sealed class CatalogMarginService
{
    public decimal Calculate(decimal price, decimal cost) => price <= 0 ? 0 : decimal.Round((price - cost) / price * 100m, 4, MidpointRounding.AwayFromZero);
    public (IReadOnlyList<string> Warnings, bool RequiresApproval) Validate(decimal margin, decimal minimum, string action) =>
        margin >= minimum ? ([], false) : ([ $"Margem de {margin:N2}% abaixo do mínimo de {minimum:N2}%." ], action is "RequireApproval" or "Block");
}

public sealed class CatalogPricingService(CatalogMarginService margins)
{
    public CatalogPriceBreakdown Calculate(CatalogPriceRequest request, decimal minimumMargin = 0, string marginAction = "Warn")
    {
        if (request.BasePrice < 0 || request.EstimatedCost < 0) throw new ArgumentException("Preço e custo não podem ser negativos.");
        var price = request.BasePrice; var discount = 0m; var increase = 0m;
        var rules = (request.Rules ?? []).OrderByDescending(x => x.Priority).ThenBy(x => x.Id).ToArray();
        foreach (var rule in rules)
        {
            var before = price;
            price = rule.AdjustmentType switch
            {
                "FixedPrice" => rule.Amount ?? price,
                "FixedDiscount" => price - (rule.Amount ?? 0m),
                "PercentDiscount" => price * (1m - (rule.Percent ?? 0m) / 100m),
                "PercentIncrease" => price * (1m + (rule.Percent ?? 0m) / 100m),
                "MinimumPrice" => decimal.Max(price, rule.Amount ?? 0m),
                _ => throw new ArgumentException($"Tipo de ajuste inválido: {rule.AdjustmentType}.")
            };
            price = decimal.Max(0m, decimal.Round(price, 2, MidpointRounding.AwayFromZero));
            if (price < before) discount += before - price; else increase += price - before;
        }
        var margin = margins.Calculate(price, request.EstimatedCost);
        var validation = margins.Validate(margin, minimumMargin, marginAction);
        return new(request.BasePrice, rules, decimal.Round(discount,2), decimal.Round(increase,2), price, request.EstimatedCost, margin, validation.Warnings, validation.RequiresApproval, "calculated");
    }
}

public sealed class CatalogCommissionService
{
    private static readonly HashSet<string> Eligible = ["ServiceCompleted", "ProductSold", "OrderPaid", "MembershipPaid"];
    public CatalogCommissionBreakdown Calculate(CatalogCommissionRequest request)
    {
        if (request.BaseAmount < 0) throw new ArgumentException("Base da comissão não pode ser negativa.");
        if (!Eligible.Contains(request.SourceStatus)) return new(request.BaseAmount, 0m, "Pending", ["A comissão só nasce após atendimento, venda ou pagamento real."], false, request.SourceStatus);
        var value = request.CommissionType switch { "FixedAmount" => request.Amount ?? 0m, "Percentage" => request.BaseAmount * (request.Percent ?? 0m) / 100m, _ => throw new ArgumentException("Tipo de comissão inválido.") };
        if (value < 0 || value > request.BaseAmount) throw new ArgumentException("Comissão fora dos limites da base.");
        return new(request.BaseAmount, decimal.Round(value, 2, MidpointRounding.AwayFromZero), "Payable", [], true, request.SourceStatus);
    }
}

public sealed class CatalogSimulationService(TeamDataService data, ICurrentUserContext current, CatalogPricingService pricing, CatalogCommissionService commissions)
{
    public async Task<CatalogPriceBreakdown> Price(CatalogPriceRequest request, CancellationToken ct)
    {
        var result = pricing.Calculate(request); await Persist("Price", request, result, ct); return result;
    }
    public async Task<CatalogCommissionBreakdown> Commission(CatalogCommissionRequest request, CancellationToken ct)
    {
        var result = commissions.Calculate(request); await Persist("Commission", request, result, ct); return result;
    }
    private Task<Guid> Persist(string type, object input, object result, CancellationToken ct)
    {
        var id=Guid.NewGuid();
        return data.WriteAsync("insert into barber.catalog_price_simulations(id,tenant_id,branch_id,simulation_type,input_json,result_json,created_by) values(@id,@tenant,@branch,@type,@input::jsonb,@result::jsonb,@user)","CatalogSimulationCreated","catalog_price_simulations",id,null,c=>{TeamDataService.Add(c,"type",type);TeamDataService.Add(c,"input",JsonSerializer.Serialize(input));TeamDataService.Add(c,"result",JsonSerializer.Serialize(result));TeamDataService.Add(c,"user",current.UserId);},ct);
    }
}
