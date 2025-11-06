
using System.Data.Common;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using Npgsql;
using Respawn;

namespace Presentation.Tests.Fixtures;

[CollectionDefinition(Constants.CatalogCollection)]
public class CatalogIntegrationCollection : ICollectionFixture<CatalogApiFixture>;

public sealed class CatalogApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private HttpClient? _client;
    public HttpClient Client => _client ??= CreateDefaultClient();

    private readonly IHost _app;

    private IResourceBuilder<PostgresServerResource> Postgres { get; }
    private string? _postgresConnectionString;

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public CatalogApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(CatalogApiFixture).Assembly.FullName, DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);
        Postgres = appBuilder.AddPostgres(AspireResourcesName.CatalogDatabase)
            .WithImageTag("latest");
        _app = appBuilder.Build();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { $"ConnectionStrings:{Postgres.Resource.Name}", _postgresConnectionString }
            });

            config.AddInMemoryCollection(FeatureManagementFixture.Build());
        });
        return base.CreateHost(builder);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        _dbConnection.Dispose();
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable) { await asyncDisposable.DisposeAsync(); }
        else { _app.Dispose(); }
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        await DatabaseFixture.SeedAsync(Services);
    }

    public async Task InitializeAsync()
    {
        await _app.StartAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        await _app.Services.GetRequiredService<ResourceNotificationService>()
            .WaitForResourceHealthyAsync(Postgres.Resource.Name, cts.Token);

        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync(cts.Token);

        await DatabaseFixture.CreateDatabaseAsync(Services);

        _dbConnection = new NpgsqlConnection(_postgresConnectionString);
        await _dbConnection.OpenAsync(cts.Token);
        _respawner =
            await Respawner.CreateAsync(_dbConnection, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });
    }
}

//
// using System.Net.Http;
// using Aspire.Hosting;
// using Aspire.Hosting.Testing;
// using Microsoft.Extensions.DependencyInjection;
// using Npgsql;
// using Projects;
// using Respawn;
// using Xunit;
//
// namespace Presentation.Tests.Fixtures;
//
// [CollectionDefinition("TodosCollection")]
// public sealed class TodosIntegrationCollection : ICollectionFixture<TodoApiFixture> { }
//
// public sealed class TodoApiFixture : IAsyncLifetime
// {
//     private DistributedApplication? _app;
//     private NpgsqlConnection? _db;
//     private Respawner? _respawner;
//
//     public HttpClient Client { get; private set; } = default!;
//
//     public async ValueTask InitializeAsync()
//     {
//         var builder = await DistributedApplicationTestingBuilder.CreateAsync<AppHost>();
//
//         builder.Services.ConfigureHttpClientDefaults(c => c.AddStandardResilienceHandler());
//
//         _app = await builder.BuildAsync();
//         await _app.StartAsync();
//
//         // Name must match your Aspire project for the API in AppHost
//         Client = _app.CreateHttpClient("Presentation");
//
//         // Optional: prepare Respawn (resource name must match your Postgres resource in AppHost)
//         try
//         {
//             var connectionString = await _app.GetConnectionStringAsync("postgres");
//             _db = new NpgsqlConnection(connectionString);
//             await _db.OpenAsync();
//             _respawner = await Respawner.CreateAsync(_db, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });
//         }
//         catch
//         {
//             // If the connection string name doesn't match, ResetDatabaseAsync will be a no-op.
//         }
//     }
//
//     public async ValueTask ResetDatabaseAsync()
//     {
//         if (_respawner is not null && _db is not null)
//             await _respawner.ResetAsync(_db);
//     }
//
//     public async ValueTask DisposeAsync()
//     {
//         if (_db is not null)
//             await _db.DisposeAsync();
//
//         if (_app is IAsyncDisposable iad)
//             await iad.DisposeAsync();
//         else
//             _app?.Dispose();
//     }
// }
