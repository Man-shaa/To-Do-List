using Presentation.Tests.Fixtures;
using Presentation.Tests.Common.Constants;

namespace Presentation.Tests.Endpoints;

[Collection(Constants.TodoCollection)]
public sealed class GetAllTodosEndpointTests(CatalogApiFixture fixture)
    : IClassFixture<CatalogApiFixture>, IAsyncLifetime
{
    /// <see cref="ProductEndpoints.GetProductsV1"/>
    [Fact]
    public async Task ProductShouldBeReturned()
    {
        var url = $"{ApiRoutes.Root}/{ApiRoutes.Products.GetProducts}?eans={ProductFixture.Gtin.ToEan()}"
            .WithApiVersion(1);
        
        var sut = await fixture.Client.GetAsync(url);

        await Verify(sut);
    }

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
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
