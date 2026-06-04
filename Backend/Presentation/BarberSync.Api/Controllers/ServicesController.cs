using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private static readonly List<ServiceCatalogItem> Services =
    [
        new("srv-001", "Corte Masculino", "Barbearia", "Corte moderno com acabamento profissional.", 45.00m, 40, true, true, true, "Ativo"),
        new("srv-002", "Barba Tradicional", "Barbearia", "Barba alinhada com toalha quente e navalha.", 35.00m, 30, true, true, true, "Ativo"),
        new("srv-003", "Corte + Barba", "Combo", "Experiência completa de corte e barba.", 75.00m, 70, true, true, true, "Ativo"),
        new("srv-004", "Sobrancelha", "Estética", "Design e alinhamento de sobrancelha.", 25.00m, 20, true, true, true, "Ativo"),
        new("srv-005", "Hidratação Capilar", "Tratamento", "Tratamento capilar profissional.", 60.00m, 45, true, true, true, "Ativo"),
        new("srv-006", "Manicure", "Beleza", "Cuidado completo para unhas.", 40.00m, 50, true, true, true, "Ativo")
    ];

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc", [FromQuery] string? search = null)
    {
        var query = Services.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(service => service.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || service.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var items = query.Skip(Math.Max(page - 1, 0) * pageSize).Take(pageSize).ToArray();
        return Ok(new { success = true, message = "Serviços carregados em modo demonstração.", page, pageSize, sortBy, sortOrder, search, items, data = items, total = query.Count(), isDemo = true });
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var service = Services.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        return service is null ? NotFound(new { success = false, message = "Serviço não encontrado." }) : Ok(new { success = true, data = service });
    }

    [HttpPost]
    public IActionResult Create([FromBody] ServiceCatalogItem request)
    {
        var service = request with { Id = string.IsNullOrWhiteSpace(request.Id) ? $"srv-{Guid.NewGuid():N}" : request.Id };
        Services.Add(service);
        return Ok(new { success = true, message = "Serviço criado em modo demonstração.", data = service });
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] ServiceCatalogItem request)
    {
        var index = Services.FindIndex(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        var service = request with { Id = id };
        if (index >= 0) Services[index] = service; else Services.Add(service);
        return Ok(new { success = true, message = "Serviço atualizado em modo demonstração.", data = service });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        Services.RemoveAll(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        return Ok(new { success = true, message = "Serviço removido em modo demonstração.", data = new { id } });
    }

    public sealed record ServiceCatalogItem(string Id, string Name, string Category, string Description, decimal Price, int DurationMinutes, bool VisibleOnPublicWeb, bool VisibleOnKiosk, bool VisibleOnMobile, string Status);
}
