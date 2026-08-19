using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BarberSync.AdminWeb.Controllers;

[Route("Admin")]
[Authorize]
public class AdminController(IWebHostEnvironment environment) : Controller
{
    [HttpGet("")] public IActionResult Index() => Render("Dashboard");
    [HttpGet("Dashboard")] public IActionResult Dashboard() => Render("Dashboard");
    [HttpGet("ChannelManager")] public IActionResult ChannelManager() => Render("ChannelManager");
    [HttpGet("LeadToCash")] public IActionResult LeadToCash() => DevelopmentOnly("LeadToCash");
    [HttpGet("SaasControlCenter")] public IActionResult SaasControlCenter() => DevelopmentOnly("SaasControlCenter");
    [HttpGet("Operations")] public IActionResult Operations() => Render("Operations");
    [HttpGet("FullServiceFlow")] public IActionResult FullServiceFlow() => DevelopmentOnly("FullServiceFlow");
    [HttpGet("CommercialFlow")] public IActionResult CommercialFlow() => DevelopmentOnly("CommercialFlow");
    [HttpGet("DemoCenter")] public IActionResult DemoCenter() => DevelopmentOnly("DemoCenter");
    [HttpGet("DemoExperience")] public IActionResult DemoExperience() => DevelopmentOnly("DemoExperience");
    [HttpGet("DemoWizard")] public IActionResult DemoWizard() => DevelopmentOnly("DemoWizard");
    [HttpGet("CustomerJourney")] public IActionResult CustomerJourney() => Render("CustomerJourney");
    [HttpGet("Clients")] public IActionResult Clients() => Render("Clients");
    [HttpGet("Clients/{id:guid}")] public IActionResult Client360(Guid id)
    {
        ViewData["ClientId"] = id;
        return Render("Client360");
    }
    [HttpGet("Professionals")] public IActionResult Professionals() => Render("Professionals");
    [HttpGet("Team")] public IActionResult Team() => Render("Professionals");
    [HttpGet("Team/{id:guid}")] public IActionResult TeamProfile(Guid id) => Redirect($"/Admin/Team?profile={id}");
    [HttpGet("Commissions")] public IActionResult Commissions() => Commercial("Comissões", "commissions", "professionalId", "Profissional", "amount", "Valor", "saleStatus", "Situação da venda");
    [HttpGet("Packages")] public IActionResult Packages() => Commercial("Pacotes", "packages", "name", "Nome", "price", "Preço", "services", "Serviços (IDs separados por vírgula)");
    [HttpGet("ClientPackages")] public IActionResult ClientPackages() => Commercial("Pacotes de clientes", "client-packages", "clientId", "Cliente", "packageId", "Pacote", "remainingSessions", "Sessões restantes");
    [HttpGet("Memberships")] public IActionResult Memberships() => Commercial("Assinaturas", "memberships", "name", "Plano", "monthlyPrice", "Mensalidade", "usageLimit", "Limite mensal");
    [HttpGet("ClientMemberships")] public IActionResult ClientMemberships() => Commercial("Assinaturas de clientes", "client-memberships", "clientId", "Cliente", "membershipId", "Plano", "billingDay", "Dia de cobrança");
    [HttpGet("Suppliers")] public IActionResult Suppliers() => Commercial("Fornecedores", "suppliers", "name", "Nome / razão social", "document", "CPF / CNPJ", "phone", "Telefone");
    [HttpGet("Purchases")] public IActionResult Purchases() => Commercial("Compras", "purchases", "supplierId", "Fornecedor", "invoiceNumber", "Nota fiscal", "items", "Itens (produto:quantidade:custo)");
    [HttpGet("Finance")] public IActionResult Finance() => Commercial("Financeiro gerencial", "finance", "description", "Descrição", "amount", "Valor", "category", "Categoria");
    [HttpGet("ServiceRecognition")] public IActionResult ServiceRecognition() => Render("ServiceRecognition");
    [HttpGet("System/AiSettings"), Authorize(Roles = "Owner,SuperAdmin,Admin")] public IActionResult AiSettings() => Render("AiSettings");
    [HttpGet("Services")] public IActionResult Services() => Render("Services");
    [HttpGet("Appointments")] public IActionResult Appointments() => Render("Appointments");
    [HttpGet("ServiceOrders")] public IActionResult ServiceOrders() => Render("ServiceOrders");
    [HttpGet("Attendance")] public IActionResult Attendance() => Render("Operations");
    [HttpGet("Cash")] public IActionResult Cash() => Render("Cash");
    [HttpGet("Payments")] public IActionResult Payments() => Render("Payments");
    [HttpGet("Financial")] public IActionResult Financial() => Render("Financial");
    [HttpGet("Products")] public IActionResult Products() => Render("Products");
    [HttpGet("Stock")] public IActionResult Stock() => Render("Stock");
    [HttpGet("Loyalty")] public IActionResult Loyalty() => Render("Loyalty");
    [HttpGet("Campaigns")] public IActionResult Campaigns() => Render("Campaigns");
    [HttpGet("Coupons")] public IActionResult Coupons() => Render("Coupons");
    [HttpGet("Reviews")] public IActionResult Reviews() => Render("Reviews");
    [HttpGet("Reports")] public IActionResult Reports() => Render("Reports");
    [HttpGet("Copilot")] public IActionResult Copilot() => Render("Copilot");
    [HttpGet("Kiosk")] public IActionResult Kiosk() => Render("Kiosk");
    [HttpGet("PublicSite")] public IActionResult PublicSite() => Render("PublicSite");
    [HttpGet("Settings")] public IActionResult Settings() => Render("Settings");
    [HttpGet("PlatformSettings")] public IActionResult PlatformSettings() => DevelopmentOnly("PlatformSettings");
    [HttpGet("Users")] public IActionResult Users() => Render("Users");
    [HttpGet("Branches")] public IActionResult Branches() => Render("Branches");
    [HttpGet("Audit")] public IActionResult Audit() => Render("Audit");
    [HttpGet("Notifications")] public IActionResult Notifications() => Render("Notifications");
    [HttpGet("SystemHealth")]
    [Authorize(Roles = "SuperAdmin,Owner,Admin")]
    public IActionResult SystemHealth() => Render("SystemHealth");
    [HttpGet("Mobile")] public IActionResult Mobile() => RedirectToAction(nameof(PublicSite));
    [HttpGet("Manual")] public IActionResult Manual() => RedirectToAction(nameof(Help));
    [HttpGet("Help")] public IActionResult Help() => Render("Help");
    [HttpGet("Onboarding")] public IActionResult Onboarding() => Render("Onboarding");
    [HttpGet("Subscription")] public IActionResult Subscription() => Render("Subscription");
    [HttpGet("ClientOnboarding")] public IActionResult ClientOnboarding() => Render("ClientOnboarding");
    [HttpGet("AddOns")] public IActionResult AddOns() => DevelopmentOnly("AddOns");
    [HttpGet("Automations")] public IActionResult Automations() => DevelopmentOnly("Automations");
    [HttpGet("Assistant")] public IActionResult Assistant() => Render("Assistant");
    [HttpGet("Reputation")] public IActionResult Reputation() => RedirectToAction(nameof(Reviews));
    [HttpGet("Integrations")] public IActionResult Integrations() => DevelopmentOnly("Integrations");
    [HttpGet("KnowledgeBase")] public IActionResult KnowledgeBase() => DevelopmentOnly("KnowledgeBase");
    [HttpGet("Diagnostics")] public IActionResult Diagnostics() => DevelopmentOnly("Diagnostics");

    private IActionResult DevelopmentOnly(string module)
        => environment.IsDevelopment() && User.IsInRole("SuperAdmin") ? Render(module) : NotFound();

    private IActionResult Render(string module) => View(module, BuildViewModel(module));

    private IActionResult Commercial(string title, string endpoint, string field1, string label1, string field2, string label2, string field3, string label3)
    {
        ViewData["Title"] = title;
        return View("CommercialModule", new CommercialModuleViewModel(title, endpoint,
            [new(field1, label1), new(field2, label2), new(field3, label3)]));
    }

    private static AdminModuleViewModel BuildViewModel(string module)
        => new(module, $"BarberSync • {module}", "Agenda, caixa, estoque, totem e inteligência em um só lugar.");

    public sealed record AdminModuleViewModel(string Module, string Title, string Subtitle);
    public sealed record CommercialField(string Name, string Label);
    public sealed record CommercialModuleViewModel(string Title, string Endpoint, IReadOnlyList<CommercialField> Fields);
}
