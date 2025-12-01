using System.Data.Common;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Presentation.Tests.fixture;


[CollectionDefinition("TodoApiFixtureCollection")]
public class TodoIntegrationCollection : ICollectionFixture<TodoApiFixture>;


public sealed class TodoApiFixture :
    WebApplicationFactory<Program>,
    IAsyncLifetime
{
    private HttpClient? _client;
    public HttpClient Client => _client ??= CreateDefaultClient();
    
    private IResourceBuilder<PostgresServerResource> Postgres { get; }
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    private string? _postgresConnectionString;

    private readonly IHost _app;
    
    public TodoApiFixture()
    {
        var options = new DistributedApplicationOptions()
        {
            AssemblyName = typeof(TodoApiFixture).Assembly.FullName,
            DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);

        Postgres = appBuilder.AddPostgres(AspireConfiguration.AspireResourcesName.TodoDatabase)
            .WithImageTag("latest");

        _app = appBuilder.Build();
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {
                    $"ConnectionStrings:{Postgres.Resource.Name}", _postgresConnectionString
                },
            });
        });
        return base.CreateHost(builder);
    }

    public async ValueTask InitializeAsync()
    {
        await _app.StartAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        await _app.Services.GetRequiredService<ResourceNotificationService>()
            .WaitForResourceHealthyAsync(Postgres.Resource.Name, cts.Token);

        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync(cts.Token);
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        // await dbContext.Database.EnsureCreatedAsync(cts.Token);
        await dbContext.Database.MigrateAsync(cancellationToken: cts.Token);

        _dbConnection = new NpgsqlConnection(_postgresConnectionString);
        await _dbConnection.OpenAsync(cts.Token);
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new Table[]
            {
                "__EFMigrationsHistory"
            },
            WithReseed = true
        });
    }
    
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    public async ValueTask ResetDatabaseAsync() => await _respawner.ResetAsync(_dbConnection);
}
