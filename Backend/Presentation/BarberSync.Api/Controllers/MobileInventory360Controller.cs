using BarberSync.Api.Security;
using BarberSync.Api.Services.Inventory360;
using BarberSync.Api.Services.Team;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.Api.Controllers;
[ApiController,Authorize,Route("api/mobile/inventory360")]
public sealed class MobileInventory360Controller(TeamDataService data,InventoryCountService counts):ControllerBase
{
 [HttpGet("summary"),RequirePermission("Inventory360.Read")]public async Task<object>Summary(CancellationToken ct)=>new{success=true,data=(await data.QueryAsync("select coalesce(sum(quantity_available),0) available,count(*) filter(where quantity_available<=0) critical from barber.inventory_stock_balances where tenant_id=@tenant and branch_id=@branch",null,ct)).Single()};
 [HttpGet("products"),RequirePermission("Inventory360.Read")]public async Task<object>Products(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select id,sku,barcode,name,unit_of_measure,status from barber.inventory_products where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null order by name",null,ct)};
 [HttpGet("stock"),RequirePermission("Inventory360.Read")]public async Task<object>Stock(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select product_id,supply_id,quantity_on_hand,quantity_reserved,quantity_available,last_movement_at from barber.inventory_stock_balances where tenant_id=@tenant and branch_id=@branch",null,ct)};
 [HttpGet("replenishment"),RequirePermission("Inventory360.Read")]public async Task<object>Replenishment(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select id,product_id,supply_id,suggested_quantity,reason,source_status,status from barber.inventory_replenishment_suggestions where tenant_id=@tenant and branch_id=@branch and status='Open'",null,ct)};
 [HttpPost("counts/{id:guid}/items"),RequirePermission("Inventory360.Counts.Manage")]public Task<InventoryCountResult>Count(Guid id,RegisterInventoryCountItemRequest r,CancellationToken ct)=>counts.RegisterCountItemAsync(r with{CountId=id},ct);
}
