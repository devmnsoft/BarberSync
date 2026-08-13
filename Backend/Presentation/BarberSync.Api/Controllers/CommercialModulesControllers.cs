using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

public abstract class CommercialModuleController(EnterpriseDataService data, ILogger logger, string resource)
    : EnterpriseCrudController(data, logger, resource)
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken ct) => List(ct);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetOne(Guid id, CancellationToken ct) => Get(id, ct);
    [HttpPost] public Task<IActionResult> Post([FromBody] JsonElement payload, CancellationToken ct) => Create(payload, ct);
    [HttpPut("{id:guid}")] public Task<IActionResult> Put(Guid id, [FromBody] JsonElement payload, CancellationToken ct) => Update(id, payload, ct);
    [HttpDelete("{id:guid}")] [Authorize(Roles = "SuperAdmin,Owner,Admin")] public Task<IActionResult> Remove(Guid id, CancellationToken ct) => Delete(id, ct);
}

[ApiController, Route("api/commissions"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager,Professional")]
public sealed class CommissionsController(EnterpriseDataService data, ILogger<CommissionsController> log) : CommercialModuleController(data, log, "commissions");
[ApiController, Route("api/packages")]
public sealed class PackagesController(EnterpriseDataService data, ILogger<PackagesController> log) : CommercialModuleController(data, log, "packages");
[ApiController, Route("api/client-packages")]
public sealed class ClientPackagesController(EnterpriseDataService data, ILogger<ClientPackagesController> log) : CommercialModuleController(data, log, "client-packages")
{
    [HttpPost("sell"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager,Cashier,Receptionist")]
    public async Task<IActionResult> Sell([FromBody] PackageSaleRequest request, CancellationToken ct) => Ok(await data.SellPackageAsync(request.PackageId, request.ClientId, request.Paid, ct));
    [HttpPost("{id:guid}/use"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager,Cashier,Receptionist,Professional")]
    public async Task<IActionResult> Use(Guid id, [FromBody] BenefitUseRequest request, CancellationToken ct) => Ok(await data.UsePackageSessionAsync(id, request.ServiceId ?? throw new EnterpriseValidationException([new("serviceId", "Serviço é obrigatório.")]), request.ServiceOrderId, ct));
    [HttpPost("{id:guid}/cancel"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ReasonRequest request, CancellationToken ct) => Ok(await data.CancelPackageAsync(id, request.Reason, ct));
}
[ApiController, Route("api/memberships")]
public sealed class MembershipsController(EnterpriseDataService data, ILogger<MembershipsController> log) : CommercialModuleController(data, log, "memberships");
[ApiController, Route("api/client-memberships")]
public sealed class ClientMembershipsController(EnterpriseDataService data, ILogger<ClientMembershipsController> log) : CommercialModuleController(data, log, "client-memberships")
{
    [HttpPost("activate"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager,Cashier,Receptionist")]
    public async Task<IActionResult> Activate([FromBody] MembershipActivationRequest request, CancellationToken ct) => Ok(await data.ActivateMembershipAsync(request.MembershipId, request.ClientId, request.Paid, ct));
    [HttpPost("{id:guid}/use"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager,Cashier,Receptionist,Professional")]
    public async Task<IActionResult> Use(Guid id, [FromBody] BenefitUseRequest request, CancellationToken ct) => Ok(await data.UseMembershipAsync(id, request.ServiceOrderId, ct));
    [HttpPost("{id:guid}/pause"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager")]
    public async Task<IActionResult> Pause(Guid id, [FromBody] ReasonRequest request, CancellationToken ct) => Ok(await data.PauseMembershipAsync(id, request.Reason, ct));
    [HttpPost("{id:guid}/cancel"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ReasonRequest request, CancellationToken ct) => Ok(await data.CancelMembershipAsync(id, request.Reason, ct));
}
[ApiController, Route("api/suppliers")]
public sealed class SuppliersController(EnterpriseDataService data, ILogger<SuppliersController> log) : CommercialModuleController(data, log, "suppliers");
[ApiController, Route("api/purchases")]
public sealed class PurchasesController(EnterpriseDataService data, ILogger<PurchasesController> log) : CommercialModuleController(data, log, "purchases")
{
    [HttpPost("{id:guid}/approve"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct) => Ok(await data.ChangePurchaseStatusAsync(id, "Approved", null, ct));
    [HttpPost("{id:guid}/cancel"), Authorize(Roles = "SuperAdmin,Owner,Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ReasonRequest request, CancellationToken ct) => Ok(await data.ChangePurchaseStatusAsync(id, "Cancelled", request.Reason, ct));
}
[ApiController, Route("api/finance")]
public sealed class FinanceController(EnterpriseDataService data, ILogger<FinanceController> log) : CommercialModuleController(data, log, "financial-entries");

public sealed record PackageSaleRequest(Guid PackageId, Guid ClientId, bool Paid);
public sealed record MembershipActivationRequest(Guid MembershipId, Guid ClientId, bool Paid);
public sealed record BenefitUseRequest(Guid ServiceOrderId, Guid? ServiceId);
public sealed record ReasonRequest(string Reason);
