using BarberSync.Api.Security;
using BarberSync.Api.Services.Inventory360;
using BarberSync.Api.Services.Team;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController,Authorize,Route("api/inventory360")]
public sealed class Inventory360Controller(TeamDataService data):ControllerBase
{
    [HttpGet("dashboard"),RequirePermission("Inventory360.Read")] public async Task<object> Dashboard(CancellationToken ct)=>new{success=true,data=(await data.QueryAsync(@"select coalesce(sum(quantity_available),0) available,coalesce(sum(quantity_on_hand*average_cost),0) inventory_value,count(*) filter(where quantity_available<=0) critical,(select count(*) from barber.inventory_stock_batches where tenant_id=@tenant and branch_id=@branch and status='Available' and expires_at between current_date and current_date+30) expiring,(select count(*) from barber.inventory_purchase_orders where tenant_id=@tenant and branch_id=@branch and status in ('Draft','Approved','PartiallyReceived')) pending_purchases from barber.inventory_stock_balances where tenant_id=@tenant and branch_id=@branch",null,ct)).Single()};
    [HttpGet("filter-options"),RequirePermission("Inventory360.Read")] public async Task<object> Filters(CancellationToken ct)=>new{success=true,data=new{products=await data.QueryAsync("select id,name from barber.inventory_products where tenant_id=@tenant and branch_id=@branch and status='Active' order by name",null,ct),supplies=await data.QueryAsync("select id,name from barber.inventory_supplies where tenant_id=@tenant and branch_id=@branch and status='Active' order by name",null,ct),suppliers=await data.QueryAsync("select id,name from barber.inventory_suppliers where tenant_id=@tenant and branch_id=@branch and status='Active' order by name",null,ct)}};
}

[ApiController,Authorize,Route("api/inventory360/products")]
public sealed class InventoryProductsController(InventoryProductService service):ControllerBase
{
    [HttpGet,RequirePermission("Inventory360.Read")]public Task<InventoryProductSearchResult> Search([FromQuery]string? query,[FromQuery]string? status,CancellationToken ct)=>service.SearchProductsAsync(new(query,status),ct);
    [HttpPost,RequirePermission("Inventory360.Products.Manage")]public Task<InventoryProductResult>Create(CreateInventoryProductRequest r,CancellationToken ct)=>service.CreateProductAsync(r,ct);
    [HttpPut("{id:guid}"),RequirePermission("Inventory360.Products.Manage")]public Task<InventoryProductResult>Update(Guid id,UpdateInventoryProductRequest r,CancellationToken ct)=>service.UpdateProductAsync(r with{Id=id},ct);
    [HttpPost("{id:guid}/activate"),RequirePermission("Inventory360.Products.Manage")]public Task<InventoryProductResult>Activate(Guid id,CancellationToken ct)=>service.ActivateProductAsync(new(id),ct);
    [HttpPost("{id:guid}/suspend"),RequirePermission("Inventory360.Products.Manage")]public Task<InventoryProductResult>Suspend(Guid id,[FromBody]ReasonRequest r,CancellationToken ct)=>service.SuspendProductAsync(new(id,r.Reason),ct);
    [HttpPost("{id:guid}/archive"),RequirePermission("Inventory360.Products.Manage")]public Task<InventoryProductResult>Archive(Guid id,[FromBody]ReasonRequest r,CancellationToken ct)=>service.ArchiveProductAsync(new(id,r.Reason),ct);
}
public sealed record ReasonRequest(string Reason);
public sealed record InventoryMasterRequest(string Name,string? Description,string UnitOfMeasure,decimal? DefaultCost,string Status="Draft");
public sealed record InventorySupplierRequest(string Name,string? Document,string? Email,string? Phone,Guid? PartnerId,string Status="Draft");

[ApiController,Authorize,Route("api/inventory360/supplies")]
public sealed class InventorySuppliesController(TeamDataService data):ControllerBase
{
 [HttpGet,RequirePermission("Inventory360.Read")]public async Task<object>List(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select * from barber.inventory_supplies where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name",null,ct)};
 [HttpPost,RequirePermission("Inventory360.Supplies.Manage")][HttpPut("{id:guid}"),RequirePermission("Inventory360.Supplies.Manage")]public async Task<object>Save(InventoryMasterRequest r,CancellationToken ct,Guid? id=null){if(string.IsNullOrWhiteSpace(r.Name)||string.IsNullOrWhiteSpace(r.UnitOfMeasure)||r.DefaultCost<0)throw new ArgumentException("Nome, unidade e custo válido são obrigatórios.");var key=await data.WriteAsync("insert into barber.inventory_supplies(id,tenant_id,branch_id,name,description,unit_of_measure,default_cost,status) values(@id,@tenant,@branch,@name,@description,@unit,@cost,@status) on conflict(id) do update set name=excluded.name,description=excluded.description,unit_of_measure=excluded.unit_of_measure,default_cost=excluded.default_cost,status=excluded.status,updated_at=now() where inventory_supplies.tenant_id=@tenant and inventory_supplies.branch_id=@branch","Inventory360.SupplySaved","inventory_supplies",id,null,c=>{TeamDataService.Add(c,"name",r.Name.Trim());TeamDataService.Add(c,"description",r.Description);TeamDataService.Add(c,"unit",r.UnitOfMeasure);TeamDataService.Add(c,"cost",r.DefaultCost);TeamDataService.Add(c,"status",r.Status);},ct);return new{success=true,data=new{id=key}};}
}

[ApiController,Authorize,Route("api/inventory360/suppliers")]
public sealed class InventorySuppliersController(TeamDataService data):ControllerBase
{
 [HttpGet,RequirePermission("Inventory360.Read")]public async Task<object>List(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select * from barber.inventory_suppliers where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name",null,ct)};
 [HttpPost,RequirePermission("Inventory360.Suppliers.Manage")][HttpPut("{id:guid}"),RequirePermission("Inventory360.Suppliers.Manage")]public async Task<object>Save(InventorySupplierRequest r,CancellationToken ct,Guid? id=null){if(string.IsNullOrWhiteSpace(r.Name))throw new ArgumentException("Nome obrigatório.");var key=await data.WriteAsync("insert into barber.inventory_suppliers(id,tenant_id,branch_id,name,document,email,phone,partner_id,status) values(@id,@tenant,@branch,@name,@document,@email,@phone,@partner,@status) on conflict(id) do update set name=excluded.name,document=excluded.document,email=excluded.email,phone=excluded.phone,partner_id=excluded.partner_id,status=excluded.status,updated_at=now() where inventory_suppliers.tenant_id=@tenant and inventory_suppliers.branch_id=@branch","Inventory360.SupplierSaved","inventory_suppliers",id,null,c=>{TeamDataService.Add(c,"name",r.Name.Trim());TeamDataService.Add(c,"document",r.Document);TeamDataService.Add(c,"email",r.Email);TeamDataService.Add(c,"phone",r.Phone);TeamDataService.Add(c,"partner",r.PartnerId);TeamDataService.Add(c,"status",r.Status);},ct);return new{success=true,data=new{id=key}};}
}

[ApiController,Authorize,Route("api/inventory360/service-inputs")]
public sealed class InventoryServiceInputsController(ServiceInputService service,TeamDataService data):ControllerBase
{
    [HttpGet,RequirePermission("Inventory360.Read")]public async Task<object>List(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select i.*,s.name service_name,p.name product_name,u.name supply_name from barber.inventory_service_inputs i join barber.services s on s.id=i.service_id left join barber.inventory_products p on p.id=i.product_id left join barber.inventory_supplies u on u.id=i.supply_id where i.tenant_id=@tenant and i.branch_id=@branch and i.deleted_at is null order by s.name",null,ct)};
    [HttpPost,RequirePermission("Inventory360.ServiceInputs.Manage")]public Task<ServiceInputResult>Configure(ConfigureServiceInputsRequest r,CancellationToken ct)=>service.ConfigureServiceInputsAsync(r,ct);
    [HttpPost("preview"),RequirePermission("Inventory360.Read")]public Task<ServiceInputPreviewResult>Preview(ServiceInputPreviewRequest r,CancellationToken ct)=>service.PreviewServiceConsumptionAsync(r,ct);
}

[ApiController,Authorize,Route("api/inventory360/stock")]
public sealed class InventoryStockController(StockMovementService service,TeamDataService data):ControllerBase
{
    [HttpGet,RequirePermission("Inventory360.Read")]public async Task<object>List(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select * from barber.inventory_stock_balances where tenant_id=@tenant and branch_id=@branch order by updated_at desc nulls last",null,ct)};
    [HttpPost("receive"),RequirePermission("Inventory360.Stock.Manage")]public Task<StockMovementResult>Receive(ReceiveStockRequest r,CancellationToken ct)=>service.ReceiveStockAsync(r,ct); [HttpPost("consume"),RequirePermission("Inventory360.Stock.Manage")]public Task<StockMovementResult>Consume(ConsumeStockRequest r,CancellationToken ct)=>service.ConsumeStockAsync(r,ct); [HttpPost("reserve"),RequirePermission("Inventory360.Stock.Manage")]public Task<StockMovementResult>Reserve(ReserveStockRequest r,CancellationToken ct)=>service.ReserveStockAsync(r,ct); [HttpPost("release-reservation"),RequirePermission("Inventory360.Stock.Manage")]public Task<StockMovementResult>Release(ReleaseStockReservationRequest r,CancellationToken ct)=>service.ReleaseReservationAsync(r,ct); [HttpPost("reverse"),RequirePermission("Inventory360.Stock.Manage")]public Task<StockMovementResult>Reverse(ReverseStockMovementRequest r,CancellationToken ct)=>service.ReverseMovementAsync(r,ct);
}

[ApiController,Authorize,Route("api/inventory360/purchases")]
public sealed class InventoryPurchasesController(PurchaseOrderService service,TeamDataService data):ControllerBase
{
    [HttpGet,RequirePermission("Inventory360.Read")]public async Task<object>List(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select o.*,s.name supplier_name from barber.inventory_purchase_orders o join barber.inventory_suppliers s on s.id=o.supplier_id where o.tenant_id=@tenant and o.branch_id=@branch order by o.created_at desc",null,ct)};
    [HttpPost,RequirePermission("Inventory360.Purchases.Manage")]public Task<PurchaseOrderResult>Create(CreatePurchaseOrderRequest r,CancellationToken ct)=>service.CreatePurchaseOrderAsync(r,ct); [HttpPost("{id:guid}/approve"),RequirePermission("Inventory360.Purchases.Approve")]public Task<PurchaseOrderResult>Approve(Guid id,CancellationToken ct)=>service.ApprovePurchaseOrderAsync(new(id),ct); [HttpPost("{id:guid}/receive"),RequirePermission("Inventory360.Purchases.Manage")]public Task<PurchaseReceivingResult>Receive(Guid id,ReceivePurchaseOrderRequest r,CancellationToken ct)=>service.ReceivePurchaseOrderAsync(r with{Id=id},ct); [HttpPost("{id:guid}/cancel"),RequirePermission("Inventory360.Purchases.Manage")]public Task<PurchaseOrderResult>Cancel(Guid id,ReasonRequest r,CancellationToken ct)=>service.CancelPurchaseOrderAsync(new(id,r.Reason),ct); [HttpPost("{id:guid}/return-to-supplier"),RequirePermission("Inventory360.Purchases.Manage")]public Task<SupplierReturnResult>Return(Guid id,ReturnToSupplierRequest r,CancellationToken ct)=>service.ReturnToSupplierAsync(r with{Id=id},ct);
}

[ApiController,Authorize,Route("api/inventory360/counts")]
public sealed class Inventory360CountsController(InventoryCountService service,TeamDataService data):ControllerBase
{
    [HttpGet,RequirePermission("Inventory360.Read")]public async Task<object>List(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select * from barber.inventory_counts where tenant_id=@tenant and branch_id=@branch order by created_at desc",null,ct)}; [HttpPost("open"),RequirePermission("Inventory360.Counts.Manage")]public Task<InventoryCountResult>Open(OpenInventoryCountRequest r,CancellationToken ct)=>service.OpenCountAsync(r,ct); [HttpPost("{id:guid}/items"),RequirePermission("Inventory360.Counts.Manage")]public Task<InventoryCountResult>Item(Guid id,RegisterInventoryCountItemRequest r,CancellationToken ct)=>service.RegisterCountItemAsync(r with{CountId=id},ct); [HttpPost("{id:guid}/close"),RequirePermission("Inventory360.Counts.Manage")]public Task<InventoryCountResult>Close(Guid id,CancellationToken ct)=>service.CloseCountAsync(new(id),ct); [HttpPost("{id:guid}/adjust"),RequirePermission("Inventory360.Counts.Manage")]public Task<InventoryAdjustmentResult>Adjust(Guid id,CancellationToken ct)=>service.ApplyAdjustmentAsync(new(id),ct);
}

[ApiController,Authorize,Route("api/inventory360/replenishment")]
public sealed class Inventory360ReplenishmentController(ReplenishmentService service):ControllerBase
{[HttpGet,RequirePermission("Inventory360.Read")]public Task<ReplenishmentDashboardResult>List(CancellationToken ct)=>service.GetDashboardAsync(new(),ct);[HttpPost("generate"),RequirePermission("Inventory360.Replenishment.Manage")]public Task<ReplenishmentSuggestionResult>Generate(CancellationToken ct)=>service.GenerateSuggestionsAsync(new(),ct);[HttpPost("{id:guid}/create-purchase"),RequirePermission("Inventory360.Replenishment.Manage")]public Task<PurchaseOrderResult>Create(Guid id,CancellationToken ct)=>service.CreatePurchaseOrderFromSuggestionAsync(new(id),ct);}

[ApiController,Authorize,Route("api/inventory360/costing")]
public sealed class InventoryCostingController(InventoryCostingService service):ControllerBase
{[HttpGet,RequirePermission("Inventory360.Costing.Read")]public Task<CogsResult>Get([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>service.CalculateCogsAsync(new(from,to),ct);}

[ApiController,Authorize,Route("api/inventory360")]
public sealed class Inventory360ReadController(TeamDataService data,InventoryCostingService costing):ControllerBase
{
 [HttpGet("batches"),RequirePermission("Inventory360.Read")]public async Task<object>Batches(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select * from barber.inventory_stock_batches where tenant_id=@tenant and branch_id=@branch order by expires_at nulls last",null,ct)};
 [HttpGet("audit"),RequirePermission("Inventory360.Read")]public async Task<object>Audit(CancellationToken ct)=>new{success=true,data=await data.QueryAsync("select * from barber.inventory_audit_events where tenant_id=@tenant and branch_id=@branch order by created_at desc limit 500",null,ct)};
 [HttpGet("reports/export"),RequirePermission("Inventory360.Reports.Export")]public async Task<IActionResult>Export([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct){var x=await costing.ExportCostingAsync(new(from,to),ct);return File(x.Content,"text/csv",x.FileName);}
}
