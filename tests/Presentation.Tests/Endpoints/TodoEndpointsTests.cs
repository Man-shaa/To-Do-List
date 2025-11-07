using System.Net.Http.Json;
using Domain.Entities;
using Presentation.Tests.fixture;

namespace Presentation.Tests.Endpoints;

[Collection("TodoTestingCollection")]
public sealed class TodoEndpointsTests(TestingFixture fixture) : IClassFixture<TestingFixture>
{
    // Test for POST /todos
    [Fact]
    public async Task CreateTodo_ReturnsCreatedTodo()
    {
        var client = fixture.CreateHttpClient();
        var testTodo = new { Title = "Test Todo", Order = 1, Completed = false };

        var sut = await client.PostAsJsonAsync("/todos", testTodo);

        await Verify(new
        {
            StatusCode = sut.StatusCode,
            Content = await sut.Content.ReadFromJsonAsync<Todo>()
        });
    }
    
    [Fact]
    public async Task GetAllTodos_ReturnsSuccess()
    {
        var client = fixture.CreateHttpClient();

        var sut = await client.GetAsync("/todos");

        await Verify(new 
        {
            StatusCode = sut.StatusCode,
            Content = sut.Content.ReadFromJsonAsync<List<Todo>>()
        });
    }
}
