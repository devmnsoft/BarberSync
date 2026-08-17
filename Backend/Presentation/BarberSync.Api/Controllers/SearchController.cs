using System.Security.Claims;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/search")]
public sealed class SearchController(EnterpriseDataService data, ILogger<SearchController> logger) : ControllerBase
{
    private static readonly SearchSource[] Sources =
    [
        new("Clientes", "clients", "/Admin/Clients?profile={id}", "Client.Read", ["name", "phone", "email", "document"]),
        new("Agendamentos", "appointments", "/Admin/Appointments?appointment={id}", "Appointment.Read", ["clientName", "professionalName", "serviceName", "status"]),
        new("Comandas", "service-orders", "/Admin/ServiceOrders?order={id}", "ServiceOrder.Read", ["number", "code", "clientName", "status"]),
        new("Produtos", "products", "/Admin/Products?product={id}", "Product.Read", ["name", "sku", "barcode", "category"]),
        new("Serviços", "services", "/Admin/Services?service={id}", "Service.Read", ["name", "category", "description"]),
        new("Profissionais", "professionals", "/Admin/Professionals?profile={id}", "Professional.Read", ["name", "email", "phone", "specialty"]),
        new("Pacotes", "packages", "/Admin/Packages?package={id}", "Package.Read", ["name", "description", "status"]),
        new("Assinaturas", "memberships", "/Admin/Memberships?membership={id}", "Membership.Read", ["name", "description", "status"]),
        new("Compras", "purchases", "/Admin/Purchases?purchase={id}", "Purchase.Read", ["number", "invoiceNumber", "supplierName", "status"]),
        new("Fornecedores", "suppliers", "/Admin/Suppliers?supplier={id}", "Supplier.Read", ["name", "tradeName", "document", "email"]),
        new("Financeiro", "financial-entries", "/Admin/Financial?entry={id}", "Finance.Read", ["description", "origin", "category", "status"])
    ];

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = q?.Trim();
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Ok(new { data = new Dictionary<string, object[]>(), traceId = HttpContext.TraceIdentifier });
        if (query.Length > 80)
            return BadRequest(new { message = "A busca deve ter no máximo 80 caracteres.", traceId = HttpContext.TraceIdentifier });

        var results = new Dictionary<string, object[]>();
        try
        {
            foreach (var source in Sources.Where(CanRead))
            {
                var matches = (await data.ListAsync(source.Resource, cancellationToken))
                    .Where(item => source.Fields.Any(field => Value(item, field).Contains(query, StringComparison.CurrentCultureIgnoreCase)))
                    .Take(6)
                    .Select(item => Result(source, item))
                    .ToArray();
                if (matches.Length > 0) results[source.Group] = matches;
            }

            var reports = ReportResults(query);
            if (reports.Length > 0 && CanRead(new("Relatórios", "", "", "Report.Read", []))) results["Relatórios"] = reports;
            return Ok(new { data = results, traceId = HttpContext.TraceIdentifier });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Falha na busca global para o usuário {UserId}.", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Não foi possível concluir a busca.",
                detail: $"Tente novamente. TraceId: {HttpContext.TraceIdentifier}");
        }
    }

    private bool CanRead(SearchSource source)
    {
        if (User.IsInRole("SuperAdmin") || User.IsInRole("Owner") || User.IsInRole("Admin") || User.IsInRole("Manager")) return true;
        return User.FindAll("permissions").Any(claim => claim.Value.Equals(source.Permission, StringComparison.OrdinalIgnoreCase));
    }

    private static object Result(SearchSource source, IReadOnlyDictionary<string, object?> item)
    {
        var id = Value(item, "id");
        var title = source.Fields.Select(field => Value(item, field)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        var detail = source.Fields.Select(field => Value(item, field)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && value != title);
        return new { title = title ?? source.Group, subtitle = detail ?? "Abrir registro", url = source.Url.Replace("{id}", Uri.EscapeDataString(id)) };
    }

    private static string Value(IReadOnlyDictionary<string, object?> item, string key)
        => item.TryGetValue(key, out var value) ? Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty : string.Empty;

    private static object[] ReportResults(string query) => new[]
    {
        "Faturamento", "Formas de pagamento", "Ocupação por profissional", "Comissões", "Pacotes", "Assinaturas",
        "Fluxo de caixa", "Estoque crítico", "Clientes inativos", "Clientes VIP", "Estornos", "Check-ins do Totem"
    }.Where(name => name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
     .Take(6).Select(name => (object)new { title = name, subtitle = "Relatório executivo", url = $"/Admin/Reports?report={Uri.EscapeDataString(name)}" }).ToArray();

    private sealed record SearchSource(string Group, string Resource, string Url, string Permission, string[] Fields);
}
