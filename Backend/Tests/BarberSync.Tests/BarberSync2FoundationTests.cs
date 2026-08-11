using BarberSync.Api.Services.Growth;
using BarberSync.Api.Services.Recognition;

namespace BarberSync.Tests;

public sealed class BarberSync2FoundationTests
{
    [Fact]
    public async Task Rule_provider_suggests_barba_but_requires_separate_confirmation()
    {
        var provider = new DevRuleBasedRecognitionProvider();
        var evt = new ServiceRecognitionEvent(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),null,null,DateTimeOffset.UtcNow,
            new HashSet<string>(["chair-inclined","face-towel"]));
        var result = await provider.SuggestAsync(evt,CancellationToken.None);
        Assert.NotNull(result); Assert.Equal("Barba",result.ServiceCategory); Assert.Equal("Pending",result.Status);
    }

    [Fact]
    public async Task Unconfigured_channels_never_report_delivery()
    {
        var message = new NotificationMessage("recipient","subject","body");
        var channels = new INotificationChannel[] { new UnconfiguredWhatsAppProvider(), new UnconfiguredEmailProvider(), new UnconfiguredSmsProvider() };
        foreach (var channel in channels) { var result=await channel.SendAsync(message,CancellationToken.None); Assert.False(result.Delivered); Assert.Equal("Canal não configurado.",result.Message); }
    }
}
