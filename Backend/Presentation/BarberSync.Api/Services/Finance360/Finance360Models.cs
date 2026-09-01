namespace BarberSync.Api.Services.Finance360;

public sealed record CreateReceivableRequest(Guid? ClientId,string SourceType,Guid SourceId,string Description,decimal Amount,DateTimeOffset DueAt);
public sealed record MarkReceivablePaidRequest(Guid Id,Guid? PaymentId,bool ManualSettlement,string? Reason,decimal Amount);
public sealed record MarkReceivablePartiallyPaidRequest(Guid Id,Guid? PaymentId,bool ManualSettlement,string? Reason,decimal Amount);
public sealed record MarkReceivableOverdueRequest(Guid Id);
public sealed record CancelReceivableRequest(Guid Id,string Reason);
public sealed record ReceivableAgingRequest(DateOnly From,DateOnly To);
public sealed record ReceivableResult(Guid Id,string Status,decimal Amount,decimal PaidAmount);
public sealed record ReceivableAgingResult(IReadOnlyList<Dictionary<string,object?>> Buckets);

public sealed record CreatePayableRequest(Guid? SupplierId,Guid? PartnerId,Guid? ProfessionalId,string SourceType,Guid SourceId,string Description,decimal Amount,DateTimeOffset DueAt,bool Scheduled=false);
public sealed record SchedulePayableRequest(Guid Id,DateTimeOffset DueAt);
public sealed record MarkPayablePaidRequest(Guid Id,Guid? PaymentId,bool ManualSettlement,string? Reason,decimal Amount);
public sealed record MarkPayablePartiallyPaidRequest(Guid Id,Guid? PaymentId,bool ManualSettlement,string? Reason,decimal Amount);
public sealed record MarkPayableOverdueRequest(Guid Id);
public sealed record CancelPayableRequest(Guid Id,string Reason);
public sealed record PayableAgingRequest(DateOnly From,DateOnly To);
public sealed record PayableResult(Guid Id,string Status,decimal Amount,decimal PaidAmount);
public sealed record PayableAgingResult(IReadOnlyList<Dictionary<string,object?>> Buckets);

public record PostingRequest(string SourceType,Guid SourceId,Guid? AccountId,Guid? CategoryId,Guid? CostCenterId,decimal Amount,string IdempotencyKey,Guid? PaymentId=null);
public sealed record PostCheckoutRequest(string SourceType,Guid SourceId,Guid? AccountId,Guid? CategoryId,Guid? CostCenterId,decimal Amount,string IdempotencyKey,Guid? PaymentId=null):PostingRequest(SourceType,SourceId,AccountId,CategoryId,CostCenterId,Amount,IdempotencyKey,PaymentId);
public sealed record PostPaymentRequest(string SourceType,Guid SourceId,Guid? AccountId,Guid? CategoryId,Guid? CostCenterId,decimal Amount,string IdempotencyKey,Guid? PaymentId):PostingRequest(SourceType,SourceId,AccountId,CategoryId,CostCenterId,Amount,IdempotencyKey,PaymentId);
public sealed record PostRefundRequest(string SourceType,Guid SourceId,Guid? AccountId,Guid? CategoryId,Guid? CostCenterId,decimal Amount,string IdempotencyKey,Guid? PaymentId):PostingRequest(SourceType,SourceId,AccountId,CategoryId,CostCenterId,Amount,IdempotencyKey,PaymentId);
public sealed record PostCommissionRequest(string SourceType,Guid SourceId,Guid? AccountId,Guid? CategoryId,Guid? CostCenterId,decimal Amount,string IdempotencyKey):PostingRequest(SourceType,SourceId,AccountId,CategoryId,CostCenterId,Amount,IdempotencyKey);
public sealed record PostPayrollSettlementRequest(string SourceType,Guid SourceId,Guid? AccountId,Guid? CategoryId,Guid? CostCenterId,decimal Amount,string IdempotencyKey):PostingRequest(SourceType,SourceId,AccountId,CategoryId,CostCenterId,Amount,IdempotencyKey);
public sealed record PostPartnerPayoutRequest(string SourceType,Guid SourceId,Guid? AccountId,Guid? CategoryId,Guid? CostCenterId,decimal Amount,string IdempotencyKey):PostingRequest(SourceType,SourceId,AccountId,CategoryId,CostCenterId,Amount,IdempotencyKey);
public sealed record ReversePostingRequest(Guid PostingId,string Reason,string IdempotencyKey);
public sealed record FinancialPostingResult(Guid Id,string Status,bool Existing=false);

public sealed record ReconciliationPreviewRequest(Guid PaymentId,Guid PostingId,Guid AccountId,decimal ActualAmount,decimal Tolerance=0m);
public sealed record ReconcilePaymentRequest(Guid PaymentId,Guid PostingId,Guid AccountId,decimal ActualAmount,decimal Tolerance=0m);
public sealed record MarkReconciliationDivergentRequest(Guid PaymentId,Guid PostingId,Guid AccountId,decimal ActualAmount,string Reason);
public sealed record ReverseReconciliationRequest(Guid Id,string Reason);
public sealed record ReconciliationPreviewResult(decimal ExpectedAmount,decimal ActualAmount,decimal Difference,string SuggestedStatus,string PaymentStatus);
public sealed record ReconciliationResult(Guid Id,string Status,decimal Difference);

public sealed record CashFlowProjectionRequest(DateOnly From,DateOnly To);
public sealed record CashFlowRealizedRequest(DateOnly From,DateOnly To);
public sealed record CashFlowComparisonRequest(DateOnly From,DateOnly To);
public sealed record CreateCashFlowSnapshotRequest(DateOnly SnapshotDate,DateOnly From,DateOnly To);
public sealed record CashFlowProjectionResult(decimal ProjectedIn,decimal ProjectedOut,string SourceStatus);
public sealed record CashFlowRealizedResult(decimal RealizedIn,decimal RealizedOut,string SourceStatus);
public sealed record CashFlowComparisonResult(CashFlowProjectionResult Projected,CashFlowRealizedResult Realized);
public sealed record CashFlowSnapshotResult(Guid Id,CashFlowComparisonResult Values);

public sealed record DreRequest(DateOnly From,DateOnly To);
public sealed record CreateDreSnapshotRequest(DateOnly From,DateOnly To);
public sealed record DreExportRequest(DateOnly From,DateOnly To);
public sealed record DreResult(decimal GrossRevenue,decimal Discounts,decimal NetRevenue,decimal ServiceCosts,decimal ProductCosts,decimal Commissions,decimal PayrollCosts,decimal PartnerPayouts,decimal OperationalExpenses,decimal GrossProfit,decimal NetResult,string SourceStatus);
public sealed record DreSnapshotResult(Guid Id,DreResult Values);
public sealed record DreExportResult(byte[] Content,string FileName);

public sealed record RegisterFinanceAuditEventRequest(string EventType,string SourceType,Guid SourceId,string Description,string? OldStatus,string? NewStatus,decimal? Amount,string? MetadataJson=null);
public sealed record FinanceAuditSearchRequest(DateOnly From,DateOnly To,string? EventType=null);
public sealed record FinanceAuditExportRequest(DateOnly From,DateOnly To,string? EventType=null);
public sealed record FinanceAuditResult(Guid Id);
public sealed record FinanceAuditSearchResult(IReadOnlyList<Dictionary<string,object?>> Events);
public sealed record FinanceAuditExportResult(byte[] Content,string FileName);
