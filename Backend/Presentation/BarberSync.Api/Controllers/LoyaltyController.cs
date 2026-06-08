using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/loyalty")]
public sealed class LoyaltyController(EnterpriseDataService data, ILogger<LoyaltyController> logger) : EnterpriseCrudController(data, logger, "loyalty")
{
    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);

    [HttpGet("accounts")]
    public Task<IActionResult> Accounts(CancellationToken cancellationToken) => List(cancellationToken);

    [HttpGet("summary")]
    public Task<IActionResult> Summary(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.LoyaltySummaryAsync(cancellationToken), "Fidelidade carregada com dados reais.")));

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);

    [HttpPost]
    public Task<IActionResult> CreateAccount([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<IActionResult> UpdateAccount(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);

    [HttpPost("accrue")]
    public Task<IActionResult> Accrue([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("loyalty_transactions", payload, cancellationToken), "Cashback/fidelidade registrado com sucesso.")));
}
