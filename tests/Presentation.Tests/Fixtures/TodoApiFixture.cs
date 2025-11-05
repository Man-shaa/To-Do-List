using System.Data.Common;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;

namespace Presentation.Tests.Fixtures;

public class TodoIntegrationCollection : ICollectionFixture<TodoApiFixture>;

public sealed class TodoApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost _app;
    private HttpClient? _client;
    public HttpClient Client => _client ??= CreateDefaultClient();

    private IResourceBuilder<PostgresServerResource> Postgres { get; }
    private string? _postgresConnectionString;

    private DbConnection _dbConnection = null!;
    private Respawner _respawner = default!;

    
    public TodoApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(TodoApiFixture).Assembly.FullName,
            DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);
        Postgres = appBuilder.AddPostgres("postgres");
        _app = appBuilder.Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _app.StartAsync();

        var cts = CancellationToken.None;
    
        await _app.Services.GetRequiredService<ResourceNotificationService>()
            .WaitForResourceHealthyAsync(Postgres.Resource.Name, cts);

        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync(cts);

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cts);
        //await dbContext.Database.MigrateAsync(cancellationToken: cts.Token);

        _dbConnection = new NpgsqlConnection(_postgresConnectionString);
        await _dbConnection.OpenAsync(cts);

    }
    
    public async ValueTask ResetDatabaseAsync() =>
        await _respawner.ResetAsync(_dbConnection);
}
