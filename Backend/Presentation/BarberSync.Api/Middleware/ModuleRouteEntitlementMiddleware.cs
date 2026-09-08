using System.Security.Claims;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using BarberSync.Domain.Saas;
using Microsoft.AspNetCore.Authorization;

namespace BarberSync.Api.Middleware;

public sealed class ModuleRouteEntitlementMiddleware(RequestDelegate next)
{
    private static readonly (PathString Prefix,string Module,string? Permission)[] Routes=
    [
        ("/api/mobile/finance360",SaasModuleKeys.Mobile,null),
        ("/api/mobile/finance360",SaasModuleKeys.Finance360,"Finance360.Read"),
        ("/api/mobile/marketing",SaasModuleKeys.Mobile,null),
        ("/api/mobile/marketing",SaasModuleKeys.MarketingStudio,"Marketing.Read"),
        ("/api/mobile/service-execution",SaasModuleKeys.Mobile,null),
        ("/api/mobile/service-execution",SaasModuleKeys.ServiceExecution,"ServiceExecution.Read"),
        ("/api/scheduling",SaasModuleKeys.Scheduling,"Scheduling.Read"),
        ("/api/service-execution",SaasModuleKeys.ServiceExecution,"ServiceExecution.Read"),
        ("/api/finance360",SaasModuleKeys.Finance360,"Finance360.Read"),
        ("/api/finance",SaasModuleKeys.Finance360,"Finance.Read"),
        ("/api/inventory360",SaasModuleKeys.InventoryPurchasing360,"Inventory360.Read"),
        ("/api/inventory",SaasModuleKeys.InventoryPurchasing360,"Stock.View"),
        ("/api/team360",SaasModuleKeys.TeamHr360,"Team360.Read"),
        ("/api/marketing",SaasModuleKeys.MarketingStudio,"Marketing.Read"),
        ("/api/quality",SaasModuleKeys.QualityRetention,"Quality.Read"),
        ("/api/catalog",SaasModuleKeys.CatalogPricing,"Catalog.Read"),
        ("/api/club",SaasModuleKeys.ClubSales,"Club.Read"),
        ("/api/clients360",SaasModuleKeys.Clients360,"Client.Read"),
        ("/api/communication",SaasModuleKeys.Communication,"Communication.Read"),
        ("/api/ai-operations",SaasModuleKeys.AiOperations,null),
        ("/api/command-center",SaasModuleKeys.CommandCenter,"CommandCenter.Read"),
        ("/api/partners",SaasModuleKeys.PartnersMarketplace,null)
    ];

    public async Task InvokeAsync(HttpContext context,IModuleEntitlementService entitlements)
    {
        if(context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null){await next(context);return;}
        var routes=Routes.Where(item=>context.Request.Path.StartsWithSegments(item.Prefix)).ToArray();
        foreach(var route in routes)
        {
            if(!Guid.TryParse(context.User.FindFirstValue("sub"),out var userId)||!Guid.TryParse(context.User.FindFirstValue("tenant_id"),out var tenantId)||!Guid.TryParse(context.User.FindFirstValue("branch_id"),out var branchId))
            {
                await Deny(context,"UNAUTHENTICATED","Autenticação necessária.",route.Module);return;
            }
            var decision=await entitlements.DecideAsync(new ModuleAccessRequest(userId,tenantId,branchId,route.Module,route.Permission),context.RequestAborted);
            if(!decision.Allowed)
            {
                var owner=context.User.IsInRole("Owner")||context.User.IsInRole("Admin");
                var message=decision.ReasonCode==ModuleAccessReasonCodes.ModuleNotEntitled&&owner?"Este módulo não está incluído na assinatura atual.":"Você não possui acesso a esta funcionalidade.";
                await Deny(context,decision.ReasonCode,message,route.Module);return;
            }
        }
        await next(context);
    }

    private static async Task Deny(HttpContext context,string code,string message,string module)
    {
        context.Response.StatusCode=StatusCodes.Status403Forbidden;context.Response.ContentType="application/problem+json";
        await context.Response.WriteAsJsonAsync(new{type="https://barbersync.app/problems/module-access",title="Acesso ao módulo negado",status=403,code,message,moduleKey=module,traceId=context.TraceIdentifier},context.RequestAborted);
    }
}
