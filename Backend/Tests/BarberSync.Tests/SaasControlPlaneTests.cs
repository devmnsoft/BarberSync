using System.Security.Claims;
using BarberSync.AdminWeb.Services.Navigation;
using BarberSync.Api.Middleware;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using BarberSync.Domain.Saas;
using BarberSync.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace BarberSync.Tests;

public sealed class SaasControlPlaneTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("04.252.011/0001-10", "04252011000110")]
    public void Brazilian_document_accepts_valid_cpf_and_cnpj(string input, string normalized)
    {
        Assert.True(BrazilianDocument.IsValid(input));
        Assert.Equal(normalized, BrazilianDocument.Normalize(input));
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("00.000.000/0000-00")]
    [InlineData("529.982.247-24")]
    public void Brazilian_document_rejects_repeated_or_invalid_digits(string input) =>
        Assert.False(BrazilianDocument.IsValid(input));

    [Fact]
    public void Tenant_menu_is_intersection_of_entitled_modules_and_permissions()
    {
        var user = Principal(
            ("modules", SaasModuleKeys.Scheduling),
            ("modules", SaasModuleKeys.Finance360),
            ("permissions", "Scheduling.Read"));

        var menu = new ModuleNavigationService().Build(user, "/Scheduling");
        var links = menu.SelectMany(group => group.Items).Select(item => item.Href).ToArray();

        Assert.Contains("/Scheduling", links);
        Assert.DoesNotContain("/Finance360", links);
        Assert.DoesNotContain("/Platform", links);
    }

    [Fact]
    public async Task Direct_module_url_is_forbidden_and_uses_claim_scope()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var entitlement = new CapturingEntitlement(false, ModuleAccessReasonCodes.ModuleNotEntitled);
        var nextCalled = false;
        var middleware = new ModuleRouteEntitlementMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/finance360/dashboard";
        context.Request.QueryString = new QueryString($"?tenantId={otherTenantId}");
        context.User = Principal(("sub", userId.ToString()), ("tenant_id", tenantId.ToString()), ("branch_id", branchId.ToString()));

        await middleware.InvokeAsync(context, entitlement);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal(tenantId, entitlement.LastRequest!.TenantId);
        Assert.Equal(branchId, entitlement.LastRequest.BranchId);
        Assert.Equal(SaasModuleKeys.Finance360, entitlement.LastRequest.ModuleKey);
    }

    [Fact]
    public async Task Entitled_module_url_reaches_controller_pipeline()
    {
        var entitlement = new CapturingEntitlement(true, ModuleAccessReasonCodes.Allowed);
        var nextCalled = false;
        var middleware = new ModuleRouteEntitlementMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/scheduling";
        context.User = Principal(("sub", Guid.NewGuid().ToString()), ("tenant_id", Guid.NewGuid().ToString()), ("branch_id", Guid.NewGuid().ToString()));

        await middleware.InvokeAsync(context, entitlement);

        Assert.True(nextCalled);
    }

    private static ClaimsPrincipal Principal(params (string Type, string Value)[] claims) =>
        new(new ClaimsIdentity(claims.Select(claim => new Claim(claim.Type, claim.Value)), "test", "name", "roles"));

    private sealed class CapturingEntitlement(bool allowed, string reasonCode) : IModuleEntitlementService
    {
        public ModuleAccessRequest? LastRequest { get; private set; }

        public Task<ModuleAccessDecision> DecideAsync(ModuleAccessRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new ModuleAccessDecision(allowed, request.ModuleKey, reasonCode, reasonCode, request.TenantId, request.UserId, "Active", allowed ? "Active" : null));
        }

        public Task<IReadOnlyDictionary<string, ModuleAccessDecision>> GetEffectiveAccessAsync(Guid userId, Guid tenantId, Guid branchId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, ModuleAccessDecision>>(new Dictionary<string, ModuleAccessDecision>());

        public void Invalidate(Guid tenantId, Guid? userId = null, Guid? branchId = null) { }
    }
}
