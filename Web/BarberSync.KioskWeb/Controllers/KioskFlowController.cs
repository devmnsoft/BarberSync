using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.KioskWeb.Controllers;

/// <summary>
/// Keeps the short-lived kiosk journey on the server. The browser never becomes
/// the source of truth for client, service, professional or attendance data.
/// </summary>
[ApiController]
[Route("KioskFlow")]
public sealed class KioskFlowController : ControllerBase
{
    private const string StateKey = "KioskFlow.State";

    [HttpGet]
    public IActionResult Get() => Ok(ReadState());

    [HttpPut]
    public IActionResult Update([FromBody] JsonElement patch)
    {
        if (patch.ValueKind != JsonValueKind.Object)
        {
            return BadRequest(new { success = false, message = "Estado do atendimento inválido." });
        }

        var state = ReadState();
        foreach (var property in patch.EnumerateObject())
        {
            state[property.Name] = property.Value.Clone();
        }

        state["updatedAt"] = JsonSerializer.SerializeToElement(DateTimeOffset.UtcNow);
        HttpContext.Session.SetString(StateKey, JsonSerializer.Serialize(state));
        return Ok(state);
    }

    [HttpDelete]
    public IActionResult Reset()
    {
        HttpContext.Session.Remove(StateKey);
        return NoContent();
    }

    private Dictionary<string, JsonElement> ReadState()
    {
        var json = HttpContext.Session.GetString(StateKey);
        if (string.IsNullOrWhiteSpace(json)) return new(StringComparer.OrdinalIgnoreCase);

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                ?? new(StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            HttpContext.Session.Remove(StateKey);
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
