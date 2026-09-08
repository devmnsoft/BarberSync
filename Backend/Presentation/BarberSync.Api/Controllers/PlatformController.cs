using BarberSync.Api.Security;
using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using BarberSync.Domain.Saas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController,Authorize,Route("api/platform")]
public sealed class PlatformController(
    IPlatformTenantRepository tenants,
    ISaasModuleRepository modules,
    IPlatformScopeService scopes,
    ICurrentUserContext currentUser) : ControllerBase
{
    [HttpGet("dashboard"),RequirePlatformPermission("Platform.Dashboard.Read")]
    public async Task<IActionResult> Dashboard([FromQuery]DateOnly? from,[FromQuery]DateOnly? until,CancellationToken ct)
    {
        var end=until??DateOnly.FromDateTime(DateTime.UtcNow);var start=from??end.AddDays(-29);
        if(start>end)return Validation("O período inicial deve ser anterior ao período final.");
        return Ok(await tenants.DashboardAsync(start,end,ct));
    }

    [HttpGet("tenants"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> TenantList([FromQuery]PlatformTenantQuery query,CancellationToken ct)=>Ok(await tenants.ListAsync(query,ct));

    [HttpGet("tenants/{tenantId:guid}"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> Tenant(Guid tenantId,CancellationToken ct){var result=await tenants.DetailAsync(tenantId,ct);return result is null?NotFound():Ok(result);}

    [HttpGet("tenants/{tenantId:guid}/users"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> TenantUsers(Guid tenantId,CancellationToken ct,[FromQuery]int page=1,[FromQuery]int pageSize=25)=>Ok(await tenants.UsersAsync(tenantId,page,pageSize,ct));

    [HttpGet("tenants/{tenantId:guid}/roles"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> TenantRoles(Guid tenantId,CancellationToken ct)=>Ok(await tenants.RolesAsync(tenantId,ct));

    [HttpGet("tenants/{tenantId:guid}/branches"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> TenantBranches(Guid tenantId,CancellationToken ct)=>Ok(await tenants.BranchesAsync(tenantId,ct));

    [HttpGet("tenants/{tenantId:guid}/usage"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> TenantUsage(Guid tenantId,[FromQuery]DateOnly? from,[FromQuery]DateOnly? until,CancellationToken ct){var end=until??DateOnly.FromDateTime(DateTime.UtcNow);return Ok(await tenants.UsageAsync(tenantId,from??end.AddDays(-29),end,ct));}

    [HttpGet("tenants/{tenantId:guid}/modules"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> TenantModules(Guid tenantId,CancellationToken ct)=>Ok(await tenants.ModulesAsync(tenantId,ct));

    [HttpGet("modules"),RequirePlatformPermission("Platform.Modules.Read")]
    public async Task<IActionResult> ModuleList(CancellationToken ct)=>Ok(await modules.ListAsync(ct));

    [HttpPost("modules"),RequirePlatformPermission("Platform.Modules.Manage")]
    public async Task<IActionResult> CreateModule(CreateSaasModuleRequest request,CancellationToken ct)
    {
        if(!ValidModuleKey(request.ModuleKey)||string.IsNullOrWhiteSpace(request.Name)||string.IsNullOrWhiteSpace(request.Category))return Validation("Informe código estável, nome e categoria do módulo.");
        var id=await modules.CreateAsync(request,currentUser.UserId,HttpContext.TraceIdentifier,ct);return Created($"/api/platform/modules/{id}",new{id});
    }

    [HttpPut("modules/{id:guid}"),RequirePlatformPermission("Platform.Modules.Manage")]
    public async Task<IActionResult> UpdateModule(Guid id,UpdateSaasModuleRequest request,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(request.Name)||string.IsNullOrWhiteSpace(request.Category))return Validation("Nome e categoria são obrigatórios.");
        return await modules.UpdateAsync(id,request,currentUser.UserId,HttpContext.TraceIdentifier,ct)?NoContent():NotFound();
    }

    [HttpGet("module-prices"),RequirePlatformPermission("Platform.Modules.Read")]
    public async Task<IActionResult> Prices([FromQuery]Guid? moduleId,CancellationToken ct)=>Ok(await modules.ListPricesAsync(moduleId,ct));

    [HttpPost("module-prices"),RequirePlatformPermission("Platform.Prices.Manage")]
    public async Task<IActionResult> CreatePrice(UpsertSaasModulePriceRequest request,CancellationToken ct)
    {
        if(request.Price<0||request.ValidUntil<=request.ValidFrom)return Validation("Preço e período de vigência são inválidos.");
        var id=await modules.UpsertPriceAsync(null,request,currentUser.UserId,HttpContext.TraceIdentifier,ct);return Created($"/api/platform/module-prices/{id}",new{id});
    }

    [HttpPut("module-prices/{id:guid}"),RequirePlatformPermission("Platform.Prices.Manage")]
    public async Task<IActionResult> UpdatePrice(Guid id,UpsertSaasModulePriceRequest request,CancellationToken ct)
    {
        if(request.Price<0||request.ValidUntil<=request.ValidFrom)return Validation("Preço e período de vigência são inválidos.");
        await modules.UpsertPriceAsync(id,request,currentUser.UserId,HttpContext.TraceIdentifier,ct);return NoContent();
    }

    [HttpGet("contracts"),RequirePlatformPermission("Platform.Tenants.Read")]
    public async Task<IActionResult> Contracts([FromQuery]Guid? tenantId,[FromQuery]Guid? moduleId,[FromQuery]string? status,CancellationToken ct)=>Ok(await modules.ListContractsAsync(tenantId,moduleId,status,ct));

    [HttpPost("contracts"),RequirePlatformPermission("Platform.Contracts.Manage")]
    public async Task<IActionResult> CreateContract(CreateTenantModuleContractRequest request,CancellationToken ct)
    {
        if(request.TenantId==Guid.Empty||request.ModuleId==Guid.Empty||string.IsNullOrWhiteSpace(request.Reason))return Validation("Cliente, módulo e motivo são obrigatórios.");
        if(!ContractStatuses.Contains(request.Status))return Validation("Situação de contrato inválida.");
        try{var id=await modules.CreateContractAsync(request,currentUser.UserId,HttpContext.TraceIdentifier,ct);return Created($"/api/platform/contracts/{id}",new{id});}
        catch(InvalidOperationException exception){return Conflict(new{code="MODULE_DEPENDENCY_REQUIRED",message=exception.Message,traceId=HttpContext.TraceIdentifier});}
    }

    [HttpPost("contracts/{id:guid}/status"),RequirePlatformPermission("Platform.Contracts.Manage")]
    public async Task<IActionResult> ContractStatus(Guid id,ChangeModuleContractStatusRequest request,CancellationToken ct)
    {
        if(!ContractStatuses.Contains(request.Status)||string.IsNullOrWhiteSpace(request.Reason))return Validation("Situação e motivo são obrigatórios.");
        return await modules.ChangeContractStatusAsync(id,request,currentUser.UserId,HttpContext.TraceIdentifier,ct)?NoContent():NotFound();
    }

    [HttpGet("audit"),RequirePlatformPermission("Platform.Audit.Read")]
    public async Task<IActionResult> Audit(CancellationToken ct,[FromQuery]Guid? tenantId,[FromQuery]int pageSize=100)=>Ok(await tenants.AuditAsync(tenantId,pageSize,ct));

    [HttpPost("scope"),RequirePlatformPermission("Platform.Scope.Enter")]
    public async Task<IActionResult> StartScope(StartPlatformScopeRequest request,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(request.Reason)||request.Reason.Trim().Length<5)return Validation("Informe um motivo claro para entrar no contexto do cliente.");
        var result=await scopes.StartAsync(currentUser.UserId,request,HttpContext.Connection.RemoteIpAddress?.ToString(),HttpContext.TraceIdentifier,ct);
        return result is null?Forbid():Ok(result);
    }

    [HttpDelete("scope/{scopeSessionId:guid}"),RequirePlatformPermission("Platform.Scope.Enter")]
    public async Task<IActionResult> EndScope(Guid scopeSessionId,CancellationToken ct){await scopes.EndAsync(currentUser.UserId,scopeSessionId,HttpContext.TraceIdentifier,ct);return NoContent();}

    private BadRequestObjectResult Validation(string message)=>BadRequest(new{code="VALIDATION_ERROR",message,traceId=HttpContext.TraceIdentifier});
    private static bool ValidModuleKey(string value)=>!string.IsNullOrWhiteSpace(value)&&value.Length<=80&&value.All(character=>character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');
    private static readonly IReadOnlySet<string> ContractStatuses=new HashSet<string>(StringComparer.OrdinalIgnoreCase){ModuleContractStatuses.Pending,ModuleContractStatuses.PendingActivation,ModuleContractStatuses.Trial,ModuleContractStatuses.Active,ModuleContractStatuses.GracePeriod,ModuleContractStatuses.PastDue,ModuleContractStatuses.Suspended,ModuleContractStatuses.Cancelled,ModuleContractStatuses.Expired};
}

[ApiController,Authorize,Route("api/governance/modules")]
public sealed class TenantModulesController(ISaasModuleRepository modules,IModuleEntitlementService entitlements,ICurrentUserContext currentUser):ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Effective(CancellationToken ct)=>Ok(await entitlements.GetEffectiveAccessAsync(currentUser.UserId,currentUser.TenantId,currentUser.BranchId,ct));

    [HttpGet("catalog"),RequirePermission("Subscription.Manage")]
    public async Task<IActionResult> Catalog(CancellationToken ct)
    {
        var catalog=await modules.ListAsync(ct);var contracts=await modules.ListContractsAsync(currentUser.TenantId,null,null,ct);
        return Ok(catalog.Select(module=>new{module.Id,module.ModuleKey,module.Name,module.Description,module.Category,module.IsCore,module.IsSellable,price=module.CurrentMonthlyPrice,priceLabel=module.CurrentMonthlyPrice.HasValue?null:"Preço não configurado",contract=contracts.FirstOrDefault(contract=>contract.ModuleId==module.Id)}));
    }

    [HttpPost("{moduleId:guid}/request"),RequirePermission("Subscription.Manage")]
    public async Task<IActionResult> RequestModule(Guid moduleId,[FromBody]ModuleRequest request,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(request.Reason))return BadRequest(new{code="VALIDATION_ERROR",message="Informe o motivo da solicitação.",traceId=HttpContext.TraceIdentifier});
        var contract=new CreateTenantModuleContractRequest(currentUser.TenantId,moduleId,ModuleContractStatuses.PendingActivation,request.BillingCycle,null,"BRL",DateTimeOffset.UtcNow,null,null,request.Reason);
        var id=await modules.CreateContractAsync(contract,currentUser.UserId,HttpContext.TraceIdentifier,ct);return Accepted(new{id,status=ModuleContractStatuses.PendingActivation,message="Solicitação registrada. Nenhuma cobrança foi simulada."});
    }
    public sealed record ModuleRequest(string BillingCycle,string Reason);
}
