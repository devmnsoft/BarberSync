using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class ClientsController : Controller { public IActionResult Index() => View("EntityPage"); }
public class ProfessionalsController : Controller { public IActionResult Index() => View("EntityPage"); }
public class ServicesController : Controller { public IActionResult Index() => View("EntityPage"); }
public class AppointmentsController : Controller { public IActionResult Index() => View("EntityPage"); }
public class ServiceOrdersController : Controller { public IActionResult Index() => View("EntityPage"); }
public class CashController : Controller { public IActionResult Index() => View("EntityPage"); }
public class StockController : Controller { public IActionResult Index() => View("EntityPage"); }
public class CopilotController : Controller { public IActionResult Index() => View("EntityPage"); }
