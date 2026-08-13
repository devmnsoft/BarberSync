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
    [HttpGet("LeadToCash")] public IActionResult LeadToCash() => Render("LeadToCash");
    [HttpGet("SaasControlCenter")] public IActionResult SaasControlCenter() => Render("SaasControlCenter");
    [HttpGet("Operations")] public IActionResult Operations() => Render("Operations");
    [HttpGet("FullServiceFlow")] public IActionResult FullServiceFlow() => Render("FullServiceFlow");
    [HttpGet("CommercialFlow")] public IActionResult CommercialFlow() => Render("CommercialFlow");
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
    [HttpGet("PlatformSettings")] public IActionResult PlatformSettings() => Render("PlatformSettings");
    [HttpGet("Users")] public IActionResult Users() => Render("Users");
    [HttpGet("Branches")] public IActionResult Branches() => Render("Branches");
    [HttpGet("Audit")] public IActionResult Audit() => Render("Audit");
    [HttpGet("Notifications")] public IActionResult Notifications() => Render("Notifications");
    [HttpGet("Mobile")] public IActionResult Mobile() => RedirectToAction(nameof(PublicSite));
    [HttpGet("Manual")] public IActionResult Manual() => RedirectToAction(nameof(Help));
    [HttpGet("Help")] public IActionResult Help() => Render("Help");
    [HttpGet("Onboarding")] public IActionResult Onboarding() => Render("Onboarding");
    [HttpGet("Subscription")] public IActionResult Subscription() => Render("Subscription");
    [HttpGet("ClientOnboarding")] public IActionResult ClientOnboarding() => Render("ClientOnboarding");
    [HttpGet("AddOns")] public IActionResult AddOns() => Render("AddOns");
    [HttpGet("Automations")] public IActionResult Automations() => Render("Automations");
    [HttpGet("Assistant")] public IActionResult Assistant() => Render("Assistant");
    [HttpGet("Reputation")] public IActionResult Reputation() => RedirectToAction(nameof(Reviews));
    [HttpGet("Integrations")] public IActionResult Integrations() => Render("Integrations");
    [HttpGet("KnowledgeBase")] public IActionResult KnowledgeBase() => Render("KnowledgeBase");
    [HttpGet("Diagnostics")] public IActionResult Diagnostics() => DevelopmentOnly("Diagnostics");

    private IActionResult DevelopmentOnly(string module)
        => environment.IsDevelopment() && User.IsInRole("SuperAdmin") ? Render(module) : NotFound();

    private IActionResult Render(string module) => View(module, BuildViewModel(module));

    private static AdminModuleViewModel BuildViewModel(string module)
        => new(module, $"BarberSync • {module}", "Agenda, caixa, estoque, totem e inteligência em um só lugar.");

    public sealed record AdminModuleViewModel(string Module, string Title, string Subtitle);
}
