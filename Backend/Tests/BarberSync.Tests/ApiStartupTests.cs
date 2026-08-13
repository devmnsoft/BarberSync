using BarberSync.Api.Security;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace BarberSync.Tests;

public sealed class ApiStartupTests
{
    [Fact]
    public void Api_ServiceProvider_Should_Not_Register_Parameterized_Permission_Filter()
    {
        var programPath = FindRepositoryFile("Backend", "Presentation", "BarberSync.Api", "Program.cs");
        var program = File.ReadAllText(programPath);

        Assert.DoesNotContain("AddScoped<RequirePermissionFilter>", program, StringComparison.Ordinal);
        Assert.DoesNotContain("AddScoped(typeof(RequirePermissionFilter", program, StringComparison.Ordinal);
        Assert.Contains("AddScoped<ICurrentUserContext, CurrentUserContext>", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Attribute_Should_Pass_Permission_As_TypeFilter_Argument()
    {
        var attribute = new RequirePermissionAttribute("ServiceOrder.Read");

        Assert.Equal(typeof(RequirePermissionFilter), attribute.ImplementationType);
        Assert.Equal("ServiceOrder.Read", Assert.Single(attribute.Arguments!));
    }

    [Theory]
    [InlineData("superadmin")]
    [InlineData("OWNER")]
    [InlineData("Admin")]
    public void Administrative_Roles_Should_Bypass_Permission_Check_Case_Insensitively(string role)
    {
        var context = AuthorizationContext();
        var filter = new RequirePermissionFilter(new StubCurrentUser([role], []), "ServiceOrder.Read");

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void User_Without_Required_Permission_Should_Receive_Forbidden()
    {
        var context = AuthorizationContext();
        var filter = new RequirePermissionFilter(new StubCurrentUser(["Professional"], ["Appointment.Read"]), "ServiceOrder.Read");

        filter.OnAuthorization(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public void Permission_Check_Should_Be_Case_Insensitive()
    {
        var context = AuthorizationContext();
        var filter = new RequirePermissionFilter(new StubCurrentUser([], ["serviceorder.read"]), "ServiceOrder.Read");

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    private static AuthorizationFilterContext AuthorizationContext()
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, []);
    }

    private static string FindRepositoryFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine([directory.FullName, .. parts]);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Repository file not found: {Path.Combine(parts)}");
    }

    private sealed class StubCurrentUser(IEnumerable<string> roles, IEnumerable<string> permissions) : ICurrentUserContext
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid BranchId { get; } = Guid.NewGuid();
        public IReadOnlySet<string> Roles { get; } = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        public IReadOnlySet<string> Permissions { get; } = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
