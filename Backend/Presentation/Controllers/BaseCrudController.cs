using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Presentation.Controllers;

[ApiController]
public abstract class BaseCrudController : ControllerBase
{
    protected static object Paged<T>(IEnumerable<T> items, int total, int page, int pageSize)
        => new { items, total, page, pageSize };
}
