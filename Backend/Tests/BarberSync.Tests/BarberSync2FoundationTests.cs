using BarberSync.Api.Services.Growth;
using BarberSync.Api.Services.Recognition;
using BarberSync.Application.Abstractions;
using System;
using System.Data.Common;

namespace BarberSync.Tests;

public sealed class BarberSync2FoundationTests
{
    [Fact]
    public void Recognition_contracts_keep_suggestions_pending_until_human_confirmation()
    {
        var serviceId = Guid.NewGuid();
        var evt = new RecognitionEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null, null,
            DateTimeOffset.UtcNow, ["chair-inclined", "face-towel"]);
        var rule = new RecognitionRule(Guid.NewGuid(), "Barba", serviceId, ["chair-inclined", "face-towel"], .85m, 300, true);
        var suggestion = new RecognitionSuggestion(Guid.NewGuid(), evt.Id, rule.ServiceId, null, rule.MinimumConfidence,
            rule.Name, "Pending", DateTimeOffset.UtcNow);
        var decision = new RecognitionDecision(suggestion.Id, serviceId, null, null, null, false, null);

        Assert.NotEmpty(evt.Signals);
        Assert.True(rule.Active);
        Assert.Equal("Pending", suggestion.Status);
        Assert.False(decision.CreatePreOrder);
        Assert.Null(decision.ServiceOrderId);
    }

    [Fact]
    public async Task Unconfigured_ai_provider_is_safe_by_default()
    {
        IAiProvider provider = new UnconfiguredAiProvider();

        Assert.False(provider.IsConfigured);
        Assert.False(await provider.TestAsync(CancellationToken.None));
        Assert.Equal(nameof(UnconfiguredAiProvider), provider.Name);
    }

    [Fact]
    public async Task Recognition_service_does_not_persist_or_fabricate_a_suggestion_without_a_provider()
    {
        var service = new ServiceRecognitionService(new ConnectionFactoryThatMustNotBeUsed(), new UnconfiguredAiProvider());
        var item = new RecognitionEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null, null,
            DateTimeOffset.UtcNow, ["chair-inclined"]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RecordEventAsync(item, CancellationToken.None));

        Assert.Contains("não configurado", exception.Message);
        Assert.Contains("nenhum evento operacional", exception.Message);
    }

    [Fact]
    public async Task Unconfigured_channels_never_report_delivery()
    {
        var message = new NotificationMessage("recipient","subject","body");
        var channels = new INotificationChannel[] { new UnconfiguredWhatsAppProvider(), new UnconfiguredEmailProvider(), new UnconfiguredSmsProvider() };
        foreach (var channel in channels) { var result=await channel.SendAsync(message,CancellationToken.None); Assert.False(result.Delivered); Assert.Equal("Canal não configurado.",result.Message); }
    }

    private sealed class ConnectionFactoryThatMustNotBeUsed : IDbConnectionFactory
    {
        public Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default) =>
            throw new Xunit.Sdk.XunitException("A conexão não deve ser aberta sem um provider configurado.");
    }
}
