using System.Net.Http.Json;
using Domain.Entities;
using Presentation.Tests.fixture;

namespace Presentation.Tests.Endpoints;

[Collection("TodoTestingCollection")]
public sealed class TodoEndpointsTests(TestingFixture fixture) : IClassFixture<TestingFixture>
{
    [Fact]
    public async Task GetAllTodos_ReturnsSuccess()
    {
        var client = fixture.CreateHttpClient();
        var sut = await client.GetAsync("/todos");

        var content = await sut.Content.ReadFromJsonAsync<List<Todo>>();

        await Verify(new 
        {
            StatusCode = sut.StatusCode,
            Content = content
        });
    }
}
