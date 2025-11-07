using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Tests.fixture;

public sealed class TestingFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private ResourceNotificationService? _notificationService;
    private HttpClient? _testingClient;
    private string? _connectionString;
    
    public async ValueTask InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>(
            [
                "Testing=true"
            ]);

        builder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler();
        });
        
        _app = await builder.BuildAsync();

        _notificationService = _app.Services.GetService<ResourceNotificationService>();

        await _app.StartAsync();
        await _notificationService!.WaitForResourceAsync("postgres", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        await _notificationService!.WaitForResourceAsync("Presentation", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        _connectionString = await _app.GetConnectionStringAsync("todo-db");
        
        await InitDbContextAsync();
    }

    public HttpClient CreateHttpClient()
    {
        if (_testingClient is not null)
            return _testingClient;

        _testingClient = _app!.CreateHttpClient("Presentation");

        return _testingClient;
    }

    private async Task InitDbContextAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
        _connectionString = await _app!.GetConnectionStringAsync("todo-db");

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("Connection string not available");
        optionsBuilder.UseNpgsql(_connectionString);

        var dbContext = new TodoDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync();

        var todo = new Todo(
            id: 925,
            title: "Test Todo from Integration Test",
            url: new Uri("https://localhost:7214/todos/925"),
            order: 1
        );

        await dbContext.AddAsync(todo, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
            await _app.DisposeAsync();
    }
}
