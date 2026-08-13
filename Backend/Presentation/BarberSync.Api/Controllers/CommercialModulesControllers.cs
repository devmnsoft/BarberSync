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
public sealed class ClientPackagesController(EnterpriseDataService data, ILogger<ClientPackagesController> log) : CommercialModuleController(data, log, "client-packages");
[ApiController, Route("api/memberships")]
public sealed class MembershipsController(EnterpriseDataService data, ILogger<MembershipsController> log) : CommercialModuleController(data, log, "memberships");
[ApiController, Route("api/client-memberships")]
public sealed class ClientMembershipsController(EnterpriseDataService data, ILogger<ClientMembershipsController> log) : CommercialModuleController(data, log, "client-memberships");
[ApiController, Route("api/suppliers")]
public sealed class SuppliersController(EnterpriseDataService data, ILogger<SuppliersController> log) : CommercialModuleController(data, log, "suppliers");
[ApiController, Route("api/purchases")]
public sealed class PurchasesController(EnterpriseDataService data, ILogger<PurchasesController> log) : CommercialModuleController(data, log, "purchases");
[ApiController, Route("api/finance")]
public sealed class FinanceController(EnterpriseDataService data, ILogger<FinanceController> log) : CommercialModuleController(data, log, "financial-entries");
