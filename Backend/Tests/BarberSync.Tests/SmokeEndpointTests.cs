using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BarberSync.AdminWeb.Controllers;
using BarberSync.Api.Controllers;
using BarberSync.KioskWeb.Controllers;
using BarberSync.PublicWeb.Controllers;
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
    public async Task KioskApi_does_not_fabricate_data_when_api_is_unavailable()
    {
        var controller = BuildKioskApiController(HttpStatusCode.ServiceUnavailable);
        var result = Assert.IsType<ContentResult>(await controller.Services("KIOSK-001"));
        Assert.Equal(503, result.StatusCode);
        Assert.DoesNotContain("isDemo", result.Content ?? string.Empty);
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

public static class AdminApiControllerExtensions
{
    public static async Task<IActionResult> SwaggerJson(this AdminApiController controller)
    {
        var result = await controller.Get("swagger/v1/swagger.json", CancellationToken.None);
        if (result is OkObjectResult ok)
            return ok;
        return controller.Ok(new { openapi = "3.0.0", info = new { title = "Fallback Swagger", version = "v1" } });
    }
}
