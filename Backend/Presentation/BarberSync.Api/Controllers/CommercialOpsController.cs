using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api")]
public class CommercialOpsController : ControllerBase
{
    [HttpGet("service-orders")]
    public IActionResult GetServiceOrders()
        => Ok(new[]
        {
            new { id = Guid.NewGuid(), number = "CMD-2026-001", client = "João Silva", professional = "Marina", items = 3, total = 145.00m, status = "Open" },
            new { id = Guid.NewGuid(), number = "CMD-2026-002", client = "Carlos Lima", professional = "Rafael", items = 2, total = 95.00m, status = "Paid" }
        });

    [HttpGet("products")]
    public IActionResult GetProducts()
        => Ok(new[]
        {
            new { id = Guid.NewGuid(), name = "Pomada Modeladora", category = "Finalização", stock = 8, minStock = 10, salePrice = 49.90m, status = "Critical" },
            new { id = Guid.NewGuid(), name = "Shampoo Anticaspa", category = "Higiene", stock = 24, minStock = 8, salePrice = 39.90m, status = "Active" }
        });

    [HttpGet("stock/critical")]
    public IActionResult GetCriticalStock()
        => Ok(new[]
        {
            new { id = Guid.NewGuid(), product = "Pomada Modeladora", current = 8, minimum = 10, gap = 2 },
            new { id = Guid.NewGuid(), product = "Navalha Profissional", current = 4, minimum = 12, gap = 8 }
        });
}
