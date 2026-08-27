using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Club;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BarberSync.Api.Controllers;

[ApiController,Authorize,Route("api/club")]
public sealed class ClubController(ClubStore store,ILogger<ClubController> log):ClubControllerBase(log)
{
 [HttpGet("dashboard"),RequirePermission("Club.Read")] public Task<IActionResult> Dashboard(CancellationToken ct)=>Safe(()=>store.Dashboard(ct));
 [HttpGet("filter-options"),RequirePermission("Club.Read")] public Task<IActionResult> Options(CancellationToken ct)=>Safe(()=>store.Options(ct));
}
[ApiController,Authorize,Route("api/club/plans")]
public sealed class ClubPlansController(ClubStore store,ILogger<ClubPlansController> log):ClubControllerBase(log)
{
 [HttpGet,RequirePermission("Club.Read")] public Task<IActionResult> List(CancellationToken ct)=>Safe(()=>store.List("club_plans",ct));
 [HttpPost,RequirePermission("Club.Plans.Manage")] public Task<IActionResult> Create(PlanRequest r,CancellationToken ct)=>Safe(()=>store.CreatePlan(r,ct),true);
 [HttpPut("{id:guid}"),RequirePermission("Club.Plans.Manage")] public IActionResult Update(Guid id)=>Problem(statusCode:409,title:"Edição versionada necessária",detail:"Arquive o plano e publique uma nova versão para preservar contratos.",extensions:Trace());
 [HttpPost("{id:guid}/activate"),RequirePermission("Club.Plans.Manage")] public Task<IActionResult> Activate(Guid id,CancellationToken ct)=>Safe(()=>store.Status("club_plans",id,"Active",ct));
 [HttpPost("{id:guid}/archive"),RequirePermission("Club.Plans.Manage")] public Task<IActionResult> Archive(Guid id,CancellationToken ct)=>Safe(()=>store.Status("club_plans",id,"Archived",ct));
}
[ApiController,Authorize,Route("api/club/memberships")]
public sealed class ClubMembershipsController(ClubStore store,ILogger<ClubMembershipsController> log):ClubControllerBase(log)
{
 [HttpGet,RequirePermission("Club.Read")] public Task<IActionResult> List(CancellationToken ct)=>Safe(()=>store.List("client_memberships",ct));
 [HttpPost,RequirePermission("Club.Memberships.Manage")] public Task<IActionResult> Create(MembershipRequest r,CancellationToken ct)=>Safe(()=>store.CreateMembership(r,ct),true);
 [HttpPost("{id:guid}/activate"),RequirePermission("Club.Memberships.Manage")] public Task<IActionResult> Activate(Guid id,CancellationToken ct)=>Safe(()=>store.Status("client_memberships",id,"Active",ct));
 [HttpPost("{id:guid}/suspend"),RequirePermission("Club.Memberships.Manage")] public Task<IActionResult> Suspend(Guid id,CancellationToken ct)=>Safe(()=>store.Status("client_memberships",id,"Suspended",ct));
 [HttpPost("{id:guid}/cancel"),RequirePermission("Club.Memberships.Manage")] public Task<IActionResult> Cancel(Guid id,CancellationToken ct)=>Safe(()=>store.Status("client_memberships",id,"Cancelled",ct));
 [HttpGet("{id:guid}/usage"),RequirePermission("Club.Read")] public Task<IActionResult> Usage(Guid id,CancellationToken ct)=>Safe(()=>store.Query("select coalesce(jsonb_agg(to_jsonb(x) order by created_at desc),'[]') from barber.membership_usage x where tenant_id=@tenant and branch_id=@branch and membership_id=@id",c=>c.Parameters.AddWithValue("id",id),ct));
}
[ApiController,Authorize,Route("api/club/wallets")]
public sealed class ClientWalletsController(ClubStore store,ILogger<ClientWalletsController> log):ClubControllerBase(log)
{
 [HttpGet,RequirePermission("Club.Read")] public Task<IActionResult> List(CancellationToken ct)=>Safe(()=>store.List("client_wallets",ct));
 [HttpGet("{clientId:guid}"),RequirePermission("Club.Read")] public Task<IActionResult> Get(Guid clientId,CancellationToken ct)=>Safe(()=>store.Query("select jsonb_build_object('wallet',to_jsonb(w),'transactions',coalesce((select jsonb_agg(to_jsonb(t) order by created_at desc) from barber.wallet_transactions t where t.wallet_id=w.id),'[]')) from barber.client_wallets w where tenant_id=@tenant and branch_id=@branch and client_id=@client",c=>c.Parameters.AddWithValue("client",clientId),ct));
 [HttpPost("{clientId:guid}/credit"),RequirePermission("Club.Wallets.Manage")] public Task<IActionResult> Credit(Guid clientId,WalletRequest r,CancellationToken ct)=>Safe(()=>store.Wallet(clientId,r.Amount,"Credit",r.Reason,ct));
 [HttpPost("{clientId:guid}/debit"),RequirePermission("Club.Wallets.Manage")] public Task<IActionResult> Debit(Guid clientId,WalletRequest r,CancellationToken ct)=>Safe(()=>store.Wallet(clientId,r.Amount,"Debit",r.Reason,ct));
 [HttpPost("{clientId:guid}/adjust"),RequirePermission("Club.Wallets.Manage")] public Task<IActionResult> Adjust(Guid clientId,WalletRequest r,CancellationToken ct)=>Safe(()=>store.Wallet(clientId,Math.Abs(r.Amount),r.Amount<0?"Debit":"Adjustment",r.Reason,ct));
}
[ApiController,Authorize,Route("api/club/gift-cards")]
public sealed class GiftCardsController(ClubStore store,ILogger<GiftCardsController> log):ClubControllerBase(log)
{
 [HttpGet,RequirePermission("Club.Read")] public Task<IActionResult> List(CancellationToken ct)=>Safe(()=>store.List("gift_cards",ct));
 [HttpPost,RequirePermission("Club.GiftCards.Manage")] public async Task<IActionResult> Create(GiftRequest r,CancellationToken ct){try{var (data,code)=await store.CreateGift(r,ct);return StatusCode(201,new{success=true,data,activationCode=code,showOnce=true,traceId=HttpContext.TraceIdentifier});}catch(Exception e){return Failure(e);}}
 [HttpPost("{id:guid}/activate"),RequirePermission("Club.GiftCards.Manage")] public Task<IActionResult> Activate(Guid id,CancellationToken ct)=>Safe(()=>store.Status("gift_cards",id,"Active",ct));
 [HttpPost("redeem"),RequirePermission("Club.GiftCards.Manage")] public Task<IActionResult> Redeem(CodeRedemption r,CancellationToken ct)=>Safe(()=>store.RedeemGift(r,ct));
 [HttpPost("{id:guid}/cancel"),RequirePermission("Club.GiftCards.Manage")] public Task<IActionResult> Cancel(Guid id,CancellationToken ct)=>Safe(()=>store.Status("gift_cards",id,"Cancelled",ct));
}
[ApiController,Authorize,Route("api/club/{resource:regex(vouchers|combos|online-sales)}")]
public sealed class ClubCatalogController(ClubStore store,ILogger<ClubCatalogController> log):ClubControllerBase(log)
{
 [HttpGet,RequirePermission("Club.Read")] public Task<IActionResult> List(string resource,CancellationToken ct)=>Safe(()=>store.List(resource switch{"vouchers"=>"vouchers","combos"=>"commercial_combos",_=>"online_sales_orders"},ct));
}
[ApiController,Authorize,Route("api/club/reports")]
public sealed class ClubReportsController(ClubStore store,ILogger<ClubReportsController> log):ClubControllerBase(log)
{[HttpGet("export"),RequirePermission("Club.Reports.Export")] public Task<IActionResult> Export(CancellationToken ct)=>Safe(()=>store.Dashboard(ct));}

public abstract class ClubControllerBase(ILogger logger):ControllerBase
{
 protected Dictionary<string,object?> Trace()=>new(){{"traceId",HttpContext.TraceIdentifier}};
 protected async Task<IActionResult> Safe(Func<Task<JsonElement>> operation,bool created=false){try{var data=await operation();return StatusCode(created?201:200,new{success=true,data,traceId=HttpContext.TraceIdentifier});}catch(Exception e){return Failure(e);}}
 protected IActionResult Failure(Exception e){var status=e switch{ClubValidationException=>400,KeyNotFoundException=>409,PostgresException p when p.SqlState==PostgresErrorCodes.UniqueViolation=>409,_=>500};logger.Log(status==500?LogLevel.Error:LogLevel.Warning,e,"Club operation failed {TraceId}",HttpContext.TraceIdentifier);return Problem(statusCode:status,title:status==500?"Falha no Clube & Vendas":"Regra comercial não atendida",detail:status==500?"Tente novamente e informe o traceId ao suporte.":e.Message,extensions:Trace());}
}
