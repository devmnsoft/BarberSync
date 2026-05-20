namespace BarberSync.Application.Filters;

public record PagedQuery(int Page = 1, int PageSize = 20, string? SortBy = null, string? SortOrder = "asc", string? Search = null);
