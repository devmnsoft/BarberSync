using System.Security.Cryptography;
using System.Text;
using BarberSync.Api.Finance.Models;

namespace BarberSync.Api.Finance.Services;

public class FinanceDataStore
{
    public Dictionary<Guid, FiscalSettingsDto> FiscalSettings { get; } = new();
    public List<ReceiptDto> Receipts { get; } = new();
    public List<SupplierDto> Suppliers { get; } = new();
    public List<AccountPayableDto> Payables { get; } = new();
    public List<AccountReceivableDto> Receivables { get; } = new();
    public List<CashflowEntryDto> CashflowEntries { get; } = new();
    public Dictionary<Guid, TaxConfigurationDto> TaxConfigurations { get; } = new();
    public List<TaxEstimateDto> TaxEstimates { get; } = new();
    public List<ContractDto> Contracts { get; } = new();
    public List<DocumentDto> Documents { get; } = new();

    public ReceiptDto GenerateReceipt(GenerateReceiptRequest request)
    {
        var settings = FiscalSettings.TryGetValue(request.TenantId, out var current)
            ? current
            : new FiscalSettingsDto(request.TenantId, "Empresa Demo", "00.000.000/0001-00", "ISENTO", "Simples Nacional", "9602-5/01", 5, "R1", 1);

        FiscalSettings[request.TenantId] = settings with { NextReceiptNumber = settings.NextReceiptNumber + 1 };
        var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var discountTotal = request.Items.Sum(i => i.Discount);
        var total = subtotal - discountTotal - request.CashbackUsed;
        var number = settings.NextReceiptNumber.ToString("D8");
        var seed = $"{request.TenantId}:{request.BranchId}:{number}:{total}:{DateTime.UtcNow:O}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(seed)))[..24];

        var receipt = new ReceiptDto(Guid.NewGuid(), request.TenantId, request.BranchId, number, settings.ReceiptSeries, DateTime.UtcNow, settings.CompanyName, request.CustomerName, subtotal, discountTotal, request.CashbackUsed, total, request.PaymentMethod, hash, false, null, request.Items);
        Receipts.Add(receipt);
        CashflowEntries.Add(new CashflowEntryDto(Guid.NewGuid(), request.TenantId, request.BranchId, DateTime.UtcNow, "IN", total, "RECEIPT"));
        return receipt;
    }
}
