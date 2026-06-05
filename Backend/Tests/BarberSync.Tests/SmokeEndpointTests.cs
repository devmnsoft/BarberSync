using System.Net;
using System.Text.Json;
using BarberSync.AdminWeb.Controllers;
using BarberSync.Api.Controllers;
using BarberSync.KioskWeb.Controllers;
using BarberSync.PublicWeb.Controllers;
using BarberSync.Api.Controllers.Configuration;
using BarberSync.Api.Services.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BarberSync.Tests;

public class SmokeEndpointTests
{
    [Fact]
    public void Api_services_endpoint_returns_200_envelope()
    {
        var result = new ServicesController().GetAll();
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Contains("Serviços", JsonSerializer.Serialize(ok.Value));
    }

    [Fact]
    public void Api_professionals_endpoint_returns_200_envelope()
    {
        var controller = new ProfessionalsController(NullLogger<ProfessionalsController>.Instance);
        var ok = Assert.IsType<OkObjectResult>(controller.Get());
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Contains("Profissionais", JsonSerializer.Serialize(ok.Value));
    }

    [Fact]
    public void Api_kiosk_services_endpoint_returns_200_envelope()
    {
        var controller = new KioskConfigController(new StubConfigurationService(), NullLogger<KioskConfigController>.Instance);
        var action = controller.Services("KIOSK-DEMO-001");
        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Contains("Serviços", JsonSerializer.Serialize(ok.Value));
    }


    [Fact]
    public void Api_kiosk_professionals_endpoint_returns_200_envelope()
    {
        var controller = new KioskConfigController(new StubConfigurationService(), NullLogger<KioskConfigController>.Instance);
        var action = controller.Professionals("demo", "KIOSK-DEMO-001");
        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Contains("Profissionais", JsonSerializer.Serialize(ok.Value));
    }

    [Fact]
    public async Task AdminApi_swagger_json_returns_200_fallback_contract()
    {
        var controller = BuildAdminApiController(HttpStatusCode.ServiceUnavailable);
        var ok = Assert.IsType<OkObjectResult>(await controller.SwaggerJson());
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Contains("openapi", JsonSerializer.Serialize(ok.Value));
    }

    [Fact]
    public async Task AdminApi_fallback_returns_standard_json_envelope()
    {
        var controller = BuildAdminApiController(HttpStatusCode.NotFound);
        var ok = Assert.IsType<OkObjectResult>(await controller.Clients());
        AssertStandardEnvelope(ok.Value, "isDemo");
    }

    [Fact]
    public async Task PublicApi_fallback_returns_standard_json_envelope()
    {
        var controller = BuildPublicApiController(HttpStatusCode.NotFound);
        var ok = Assert.IsType<OkObjectResult>(await controller.Services());
        AssertStandardEnvelope(ok.Value, "isDemo");
    }

    [Fact]
    public async Task KioskApi_fallback_returns_standard_json_envelope()
    {
        var controller = BuildKioskApiController(HttpStatusCode.NotFound);
        var ok = Assert.IsType<OkObjectResult>(await controller.Services("KIOSK-DEMO-001"));
        AssertStandardEnvelope(ok.Value, "data");
    }

    [Fact]
    public void Api_smoke_test_has_health_contract_metadata()
    {
        var controller = new DemoCommerceController();
        var ok = Assert.IsType<OkObjectResult>(controller.ProductsGet());
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Contains("Produtos", JsonSerializer.Serialize(ok.Value));
    }

    private static AdminApiController BuildAdminApiController(HttpStatusCode statusCode) =>
        new(new StubHttpClientFactory(statusCode), BuildConfiguration(), NullLogger<AdminApiController>.Instance);

    private static PublicApiController BuildPublicApiController(HttpStatusCode statusCode) =>
        new(new StubHttpClientFactory(statusCode), BuildConfiguration(), NullLogger<PublicApiController>.Instance);

    private static KioskApiController BuildKioskApiController(HttpStatusCode statusCode) =>
        new(new StubHttpClientFactory(statusCode), BuildConfiguration(), NullLogger<KioskApiController>.Instance);

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiSettings:BaseUrl"] = "http://barbersync-test.local" })
        .Build();

    private static void AssertStandardEnvelope(object? value, string expectedProperty)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.True(doc.RootElement.TryGetProperty("message", out _));
        Assert.True(doc.RootElement.TryGetProperty(expectedProperty, out _));
    }

    private sealed class StubConfigurationService : IConfigurationService
    {
        public PublicBrandingDto GetBranding(string tenantSlug) => new(tenantSlug, "BarberSync Demo", "/img/logo-barbersync.svg", "#111827", "#d4af37");
        public PublicLandingDto GetLanding(string tenantSlug) => new(tenantSlug, "BarberSync", "Demo", true);
        public IReadOnlyList<PublicServiceDto> GetServices(string tenantSlug) => Array.Empty<PublicServiceDto>();
        public IReadOnlyList<PublicProfessionalDto> GetProfessionals(string tenantSlug) => Array.Empty<PublicProfessionalDto>();
        public KioskConfigDto GetKioskByDevice(string deviceCode) => new(deviceCode, "barbersync-demo", "matriz", true, 90);
    }

    private sealed class StubHttpClientFactory(HttpStatusCode statusCode) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new StubHandler(statusCode)) { BaseAddress = new Uri("http://barbersync-test.local") };
    }

    private sealed class StubHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(string.Empty) });
    }
}
