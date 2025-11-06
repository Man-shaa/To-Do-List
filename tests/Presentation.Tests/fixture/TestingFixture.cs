using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Tests.fixture;

public sealed class TestingFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private ResourceNotificationService? _notificationService;
    private HttpClient? _testingClient;

    public async ValueTask InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();

        builder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler();
        });

        _app = await builder.BuildAsync();

        _notificationService = _app.Services.GetService<ResourceNotificationService>();

        await _app.StartAsync();
    }
    
    public async Task<HttpClient> CreateHttpClient(string applicationName = "Presentation")
    {
        if (_testingClient is not null)
            return _testingClient;

        _testingClient = _app!.CreateHttpClient(applicationName);

        await _notificationService!.WaitForResourceAsync(applicationName, KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

        return _testingClient;
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
            await _app.DisposeAsync().ConfigureAwait(false);
    }
}
