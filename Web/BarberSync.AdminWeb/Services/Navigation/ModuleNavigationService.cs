using System.Security.Claims;

namespace BarberSync.AdminWeb.Services.Navigation;

public interface IModuleNavigationService
{
    IReadOnlyList<NavigationGroup> Build(ClaimsPrincipal user,string currentPath);
}

public sealed record NavigationItem(string Label,string Href,string Icon,string? Permission=null);
public sealed record NavigationGroup(string Label,string Key,IReadOnlyList<NavigationItem> Items,bool IsPlatform=false);

public sealed class ModuleNavigationService : IModuleNavigationService
{
    private static readonly NavigationDefinition[] Definitions=
    [
        new("INÍCIO","CORE",[new("Visão Geral","/Admin/Dashboard","grid"),new("Operação do Dia","/Operation/Today","user-check")]),
        new("OPERAÇÃO","SCHEDULING",[new("Agenda","/Scheduling","calendar","Scheduling.Read")]),
        new("OPERAÇÃO","SERVICE_EXECUTION",[new("Atendimento 360","/ServiceExecution","user-check","ServiceExecution.Read"),new("Comandas","/ServiceExecution/Orders","receipt","ServiceExecution.Read")]),
        new("OPERAÇÃO","POS_CASH",[new("Caixa / PDV","/Operation/Cash","wallet","Cash.View")]),
        new("CLIENTES & RELACIONAMENTO","CLIENTS_360",[new("Clientes 360","/Clients360","users","Client.Read")]),
        new("CLIENTES & RELACIONAMENTO","RELATIONSHIP_CRM",[new("CRM / Relacionamento","/Relationship","users","Client.Read"),new("Fidelidade","/Relationship/Loyalty","gift")]),
        new("CLIENTES & RELACIONAMENTO","QUALITY_RETENTION",[new("Qualidade & Retenção","/Quality","chart","Quality.Read")]),
        new("CLIENTES & RELACIONAMENTO","CLUB_SALES",[new("Clube & Vendas","/Club","gift","Club.Read")]),
        new("COMERCIAL","CATALOG_PRICING",[new("Catálogo & Precificação","/Catalog","tag","Catalog.Read"),new("Serviços","/Catalog/Services","tag"),new("Pacotes","/Catalog/Packages","gift"),new("Cupons","/Relationship/Coupons","gift")]),
        new("EQUIPE & RH","TEAM_HR_360",[new("Equipe & RH 360","/Team360","scissors","Team360.Read"),new("Profissionais","/Team360/Professionals","user-check"),new("Escalas","/Team360/Schedules","calendar"),new("Comissões e metas","/Team360/Commissions","coins")]),
        new("ESTOQUE & COMPRAS","INVENTORY_PURCHASING_360",[new("Dashboard","/Inventory360","package","Inventory360.Read"),new("Produtos e categorias","/Inventory360/Products","box"),new("Compras e recebimentos","/Inventory360/Purchases","receipt"),new("Inventário e reposição","/Inventory360/InventoryCounts","package"),new("Fornecedores","/Inventory360/Suppliers","building")]),
        new("FINANCEIRO","FINANCE_360",[new("Dashboard Financeiro","/Finance360","coins","Finance360.Read"),new("Contas a pagar","/Finance360/Payables","receipt"),new("Contas a receber","/Finance360/Receivables","receipt"),new("Conciliação","/Finance360/Reconciliation","wallet"),new("Fluxo de caixa","/Finance360/CashFlow","chart"),new("DRE e relatórios","/Finance360/Dre","chart")]),
        new("IA & AUTOMAÇÃO","AI_OPERATIONS",[new("IA Operacional","/AiOperations","sparkles"),new("Fila de revisão","/AiOperations/ReviewQueue","user-check"),new("Câmeras e zonas","/AiOperations/Cameras","monitor"),new("Regras e evidências","/AiOperations/Rules","settings")]),
        new("MARKETING & COMUNICAÇÃO","MARKETING_STUDIO",[new("Marketing Studio","/Marketing","megaphone","Marketing.Read"),new("Campanhas e segmentos","/Marketing/Campaigns","users")]),
        new("MARKETING & COMUNICAÇÃO","COMMUNICATION",[new("Comunicação","/Communication","megaphone","Communication.Read")]),
        new("RELATÓRIOS & BI","REPORTS_BI",[new("Relatórios e indicadores","/Admin/Reports","chart"),new("Auditoria","/Admin/Audit","history")]),
        new("CANAIS DIGITAIS","CLIENT_PORTAL",[new("Portal do Cliente","/ClientPortalAdmin","monitor")]),
        new("CANAIS DIGITAIS","PUBLIC_WEB",[new("Public Web","/Admin/PublicSite","monitor")]),
        new("CANAIS DIGITAIS","MOBILE",[new("App","/Admin/Mobile","monitor")]),
        new("CANAIS DIGITAIS","TOTEM",[new("Totem","/Admin/Kiosk","monitor")]),
        new("ADMINISTRAÇÃO","CORE",[new("Unidades","/Admin/Branches","building","Branch.Manage"),new("Usuários","/Admin/Users","shield","Users.Manage"),new("Minha assinatura e módulos","/Subscription/Modules","star","Subscription.Manage"),new("Configurações","/Admin/Settings","settings")])
    ];
    private static readonly NavigationGroup Platform=new("PLATAFORMA SaaS","platform",[
        new("Dashboard Global","/Platform","grid","Platform.Dashboard.Read"),new("Clientes","/Platform/Tenants","building","Platform.Tenants.Read"),
        new("Módulos","/Platform/Modules","package","Platform.Modules.Read"),new("Planos","/Platform/Plans","star","Platform.Modules.Read"),
        new("Preços","/Platform/Prices","coins","Platform.Prices.Manage"),new("Contratos","/Platform/Contracts","receipt","Platform.Contracts.Manage"),
        new("Cobranças","/Platform/Billing","wallet","Platform.Contracts.Manage"),new("Usuários da Plataforma","/Platform/Users","shield","Platform.Users.Manage"),
        new("Auditoria Global","/Platform/Audit","history","Platform.Audit.Read"),new("Uso da Plataforma","/Platform/Usage","chart","Platform.Dashboard.Read"),
        new("Saúde do Sistema","/Platform/Health","monitor","Platform.Dashboard.Read"),new("Configurações da Plataforma","/Platform/Settings","settings","Platform.Modules.Manage")],true);

    public IReadOnlyList<NavigationGroup> Build(ClaimsPrincipal user,string currentPath)
    {
        var platform=user.HasClaim("platform_admin","true");var scoped=platform&&user.HasClaim(claim=>claim.Type=="tenant_id");
        if(platform&&!scoped)return FilterItems([Platform],user,true);
        var modules=user.FindAll("modules").Select(x=>x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var groups=Definitions.Where(definition=>modules.Contains(definition.Module)).GroupBy(definition=>definition.Group)
            .Select((group,index)=>new NavigationGroup(group.Key,$"group-{index}",group.SelectMany(x=>x.Items).DistinctBy(x=>x.Href).ToArray())).ToList();
        if(platform)groups.Add(Platform);
        return FilterItems(groups,user,platform);
    }

    private static IReadOnlyList<NavigationGroup> FilterItems(IEnumerable<NavigationGroup> groups,ClaimsPrincipal user,bool platform)
    {
        var permissions=user.FindAll("permissions").Select(x=>x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var superAdmin=platform&&user.IsInRole("SuperAdmin");
        return groups.Select(group=>group with{Items=group.Items.Where(item=>item.Permission is null||permissions.Contains(item.Permission)||superAdmin).ToArray()}).Where(group=>group.Items.Count>0).ToArray();
    }

    private sealed record NavigationDefinition(string Group,string Module,IReadOnlyList<NavigationItem> Items);
}
