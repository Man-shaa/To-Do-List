using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Tests.fixture;

[CollectionDefinition("TodoTestingCollection")]
public class TodoIntegrationCollection : ICollectionFixture<TestingFixture>;

public sealed class TestingFixture : IAsyncLifetime
{
    private const string PostgresResourceName = "postgres";
    private const string PresentationResourceName = "Presentation";
    private const string DatabaseConnectionName = "todo-db";
    private const int ResourceStartupTimeoutSeconds = 30;
    private HttpClient? _client;
    private DistributedApplication? _app;
    private ResourceNotificationService? _notificationService;
    private string? _connectionString;

    public async ValueTask InitializeAsync()
    {
        await BuildAndStartAppAsync();
        if (_app is null)
            throw new InvalidOperationException("Application has not been initialized");
        _client = CreateHttpClient();
        await WaitForResourcesAsync();
        _connectionString = await _app.GetConnectionStringAsync(DatabaseConnectionName);
        await InitDbContextAsync();
    }

    private async Task BuildAndStartAppAsync()
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
        if (_notificationService is null)
            throw new InvalidOperationException("ResourceNotificationService not available");

        await _app.StartAsync();
    }

    private async Task WaitForResourcesAsync()
    {
        var timeout = TimeSpan.FromSeconds(ResourceStartupTimeoutSeconds);

        await _notificationService!.WaitForResourceAsync(PostgresResourceName, KnownResourceStates.Running)
            .WaitAsync(timeout);

        await _notificationService!.WaitForResourceAsync(PresentationResourceName, KnownResourceStates.Running)
            .WaitAsync(timeout);
    }
    public HttpClient CreateHttpClient()
    {
        if (_client is not null)
            return _client;
        if (_app is null)
            throw new InvalidOperationException("Application has not been initialized");

        _client = _app.CreateHttpClient(PresentationResourceName);

        return _client;
    }

    private async Task InitDbContextAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("Connection string not available");

        var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);

        var dbContext = new TodoDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync();

        await SeedTestTodoAsync(dbContext);
    }

    private static async Task SeedTestTodoAsync(TodoDbContext dbContext)
    {
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
