namespace BarberSync.Api.Finance.Models;

public record FiscalSettingsDto(Guid TenantId, string CompanyName, string Document, string MunicipalRegistration, string TaxRegime, string Cnae, decimal IssRate, string ReceiptSeries, int NextReceiptNumber);
public record ReceiptItemDto(string Description, decimal Quantity, decimal UnitPrice, decimal Discount);
public record GenerateReceiptRequest(Guid TenantId, Guid BranchId, string CustomerName, string CustomerDocument, string? ProfessionalName, string SourceType, string PaymentMethod, decimal CashbackUsed, List<ReceiptItemDto> Items);
public record ReceiptDto(Guid Id, Guid TenantId, Guid BranchId, string Number, string Series, DateTime IssuedAtUtc, string CompanyName, string CustomerName, decimal Subtotal, decimal DiscountTotal, decimal CashbackUsed, decimal Total, string PaymentMethod, string Hash, bool IsCanceled, string? CancellationReason, List<ReceiptItemDto> Items);

public record SupplierDto(Guid Id, Guid TenantId, string Name, string Document, string? Email);
public record AccountPayableDto(Guid Id, Guid TenantId, Guid BranchId, string Description, decimal Amount, DateTime DueDateUtc, string Status, Guid? SupplierId, string? CostCenter);
public record ReceivePayableRequest(decimal Amount, string PaymentMethod, DateTime PaidAtUtc);

public record AccountReceivableDto(Guid Id, Guid TenantId, Guid BranchId, string Description, decimal Amount, DateTime DueDateUtc, string Status, string CustomerName);
public record ReceiveReceivableRequest(decimal Amount, string PaymentMethod, DateTime ReceivedAtUtc);

public record CashflowEntryDto(Guid Id, Guid TenantId, Guid BranchId, DateTime DateUtc, string Type, decimal Amount, string Source);
public record DreSummaryDto(decimal GrossRevenue, decimal Discounts, decimal NetRevenue, decimal ProductCost, decimal Commissions, decimal FixedExpenses, decimal VariableExpenses, decimal EstimatedTaxes, decimal GrossProfit, decimal OperatingProfit, decimal MarginPercent);

public record TaxConfigurationDto(Guid TenantId, decimal IssRate, decimal SimplesRate, decimal ServiceTaxRate, decimal ProductTaxRate);
public record TaxEstimateDto(Guid TenantId, DateTime PeriodStartUtc, DateTime PeriodEndUtc, decimal RevenueBase, decimal EstimatedAmount, string TaxType);

public record ContractDto(Guid Id, Guid TenantId, string Type, string Title, string Status, DateTime CreatedAtUtc);
public record DocumentDto(Guid Id, Guid TenantId, string Category, string Name, DateTime? ExpirationDateUtc, string Status);

public record FinancialDashboardSummaryDto(decimal RevenueMonth, decimal ExpenseMonth, decimal EstimatedProfit, decimal MarginPercent, int OverduePayables, int OverdueReceivables, decimal EstimatedTaxes, int ReceiptsIssued, decimal ProjectedCash);
