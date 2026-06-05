using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
public class DemoCommerceController : ControllerBase
{
    private static readonly object[] Products =
    [
        new { id = "prd-001", name = "Pomada Matte", category = "Finalizadores", price = 39.90m, stock = 14, minStock = 6, status = "Ativo", isDemo = true },
        new { id = "prd-002", name = "Shampoo Premium", category = "Higiene", price = 49.90m, stock = 8, minStock = 5, status = "Ativo", isDemo = true },
        new { id = "prd-003", name = "Óleo para Barba", category = "Barba", price = 44.90m, stock = 10, minStock = 4, status = "Ativo", isDemo = true }
    ];

    private static readonly object[] ServiceOrders =
    [
        new { id = "so-001", number = "1001", client = "Marcos Vinícius", professional = "Rafael Barber", status = "Open", channel = "Admin", total = 75.00m, isDemo = true },
        new { id = "so-002", number = "1002", client = "Fernanda Costa", professional = "Camila Beauty", status = "Paid", channel = "PublicWeb", total = 125.00m, isDemo = true },
        new { id = "so-003", number = "1003", client = "Visitante Totem", professional = "Lucas Navalha", status = "InService", channel = "Kiosk", total = 45.00m, isDemo = true }
    ];

    [HttpGet("api/products")]
    public IActionResult ProductsGet() => Ok(new { success = true, message = "Produtos carregados em modo demonstração.", data = Products, items = Products, total = Products.Length, isDemo = true });

    [HttpGet("api/service-orders")]
    public IActionResult ServiceOrdersGet() => Ok(new { success = true, message = "Comandas carregadas em modo demonstração.", data = ServiceOrders, items = ServiceOrders, total = ServiceOrders.Length, isDemo = true });
}
