using Aspire.Hosting.Testing;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Presentation.Tests.Endpoints;

public sealed class TodoEndpointsTests
    // : IClassFixture<TodoApiFixture>, IAsyncLifetime
{
    [Fact]
    public async Task GetAllTodos_returnsSuccess()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();
        
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        // builder.AddNpgsqlDbContext<TodoDbContext>("todo-db",
        //     null,
        //     options =>
        //     {
        //         options.UseNpgsql(npgsqlOptionsAction =>
        //             npgsqlOptionsAction.ConfigureDataSource(dataSourceBuilder =>
        //                 {
        //                     dataSourceBuilder.EnableDynamicJson();
        //                     if (builder.Environment.IsDevelopment())
        //                         dataSourceBuilder.ConnectionStringBuilder.IncludeErrorDetail = true;
        //                 }
        //             ));
        //         if (!builder.Environment.IsDevelopment())
        //             return;
        //
        //         options.EnableDetailedErrors();
        //         options.EnableSensitiveDataLogging();
        //     });
        //
        // builder.EnrichNpgsqlDbContext<TodoDbContext>();

        await using var app = await builder.BuildAsync();

        await app.StartAsync();
        // var dbcontext = app.Services.GetRequiredService<TodoDbContext>();
        // await dbcontext.Database.MigrateAsync();
        // dbcontext.Todos.Add(new Domain.Entities.Todo(1, "Test Todo", new Uri("https://localhost/1"), 1));
        // await dbcontext.SaveChangesAsync();

        var httpClient = app.CreateHttpClient("Presentation");
        const string getTodosUrl = "/todos";

        var sut = await httpClient.GetAsync(getTodosUrl);

        await Verify(sut);
    }

    // public ValueTask InitializeAsync() => fixture.ResetDatabaseAsync();
    //
    // public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
