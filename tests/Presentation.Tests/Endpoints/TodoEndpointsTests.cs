using System.Data.Common;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;

namespace  Presentation.Tests.Endpoints;

[Collection(Common.Constants.TodoCollection)]
public sealed class GetAllTodos_ReturnsSuccess : WebApplicationFactory<Program>, IAsyncLifetime
{
    private HttpClient? _client;
    public HttpClient Client => _client ??= CreateDefaultClient();

    private IHost _app;
    private IResourceBuilder<PostgresServerResource> Postgres { get; }
    private string? _postgresConnectionString;

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public GetAllTodos_ReturnsSuccess()
    {
        var options =  new DistributedApplicationOptions
        {
            AssemblyName = typeof(Program).Assembly.FullName, DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);
        Postgres = appBuilder.AddPostgres("postgres")
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
                }
            });
        });
        return base.CreateHost(builder);
    }

    public async ValueTask InitializeAsync()
    {
        await _app.StartAsync();

        var cts = CancellationToken.None;
        await _app.Services.GetRequiredService<ResourceNotificationService>()
            .WaitForResourceHealthyAsync(Postgres.Resource.Name, cts);;

        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync(cts);
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<TodoDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cts);

        _dbConnection = new NpgsqlConnection(_postgresConnectionString);
        await _dbConnection.OpenAsync(cts);
        _respawner = await Respawner.CreateAsync(_dbConnection, new
        RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres
        });
    }

    public new async ValueTask DisposeAsync()
    {
        _dbConnection.Dispose();
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else
            _app.Dispose();
    }

    [Fact]
    public async Task ProductShouldBeReturned()
    {
        var url = "/todos";
        
        var sut = Client.GetAsync(url);

        await Verify(sut);
    }
}

// using System.Net.Http.Json;
// using System.Text.Json;
// using Infrastructure.Repositories.DTOs;
// using Presentation.Tests.Fixtures;
// using VerifyXunit;
//
// namespace Presentation.Tests.Endpoints;
//
// [Collection("TodosCollection")]
// public sealed class TodoEndpointsTests(TodoApiFixture fixture) : IClassFixture<TodoApiFixture>, IAsyncLifetime
// {
//     [Fact]
//     public async Task GetAllTodos_isSnapshotted()
//     {
//         // Seed through the API for full-stack coverage
//         _ = await fixture.Client.PostAsJsonAsync("/todos", new TodoCreateDto
//         {
//             Order = 1,
//             Title = "Test Todo"
//         });
//
//         var response = await fixture.Client.GetAsync("/todos");
//         var body = await response.Content.ReadAsStringAsync();
//
//         await Verify(new
//         {
//             StatusCode = (int)response.StatusCode,
//             Body = JsonDocument.Parse(body)
//         });
//     }
//
//     public ValueTask InitializeAsync() => fixture.ResetDatabaseAsync();
//     public ValueTask DisposeAsync() => ValueTask.CompletedTask;
// }
