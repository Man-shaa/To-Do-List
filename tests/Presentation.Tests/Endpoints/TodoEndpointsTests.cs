using System.Net.Http.Json;
using Domain.Entities;
using Presentation.Tests.fixture;

namespace Presentation.Tests.Endpoints;

[CollectionDefinition("TodoTestingCollection", DisableParallelization = true)]
public class TodoIntegrationCollection : ICollectionFixture<TestingFixture>;

[Collection("TodoTestingCollection")]
public sealed class TodoEndpointsTests(TestingFixture fixture) : IClassFixture<TestingFixture>
{
    [Fact]
    public async Task CreateTodo_ReturnsCreatedTodo()
    {
        var client = fixture.CreateHttpClient();
        var testTodo = new { Title = "Test Todo", Order = 1, Completed = false };

        var sut = await client.PostAsJsonAsync("/todos", testTodo);

        var content = await sut.Content.ReadFromJsonAsync<Todo>();
        Assert.Equal(System.Net.HttpStatusCode.Created, sut.StatusCode);
        await Verify(new
        {
            StatusCode = sut.StatusCode,
            Content = content
        });
    }
    
    [Fact]
    public async Task CreateTodo_ReturnBadRequestWhenTitleIsEmpty()
    {
        var client = fixture.CreateHttpClient();
        var testTodo = new { Title = "", Order = 1, Completed = false };

        var sut = await client.PostAsJsonAsync("/todos", testTodo);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, sut.StatusCode);
        await Verify(sut);
    }
    
    [Fact]
    public async Task GetAllTodos_ReturnsSuccess()
    {
        var client = fixture.CreateHttpClient();

        await client.PostAsJsonAsync("/todos", new { Title = "A", Order = 1, Completed = false });
        await client.PostAsJsonAsync("/todos", new { Title = "B", Order = 2, Completed = false });
        
        var sut = await client.GetAsync("/todos");

        var content = sut.Content.ReadFromJsonAsync<List<Todo>>();
        Assert.Equal(System.Net.HttpStatusCode.OK, sut.StatusCode);
        await Verify(new 
        {
            StatusCode = sut.StatusCode,
            Content = content
        });
    }
    
    [Fact]
    public async Task GetTodoById_ReturnsTodoOfIdOne()
    {
        var client = fixture.CreateHttpClient();

        var created = await (await client.PostAsJsonAsync("/todos", new { Title = "X", Order = 1, Completed = false }))
            .Content.ReadFromJsonAsync<Todo>();

        var sut = await client.GetAsync($"/todos/{created!.Id}");

        var content = sut.Content.ReadFromJsonAsync<Todo>();
        Assert.Equal(System.Net.HttpStatusCode.OK, sut.StatusCode);
        await Verify(new 
        {
            StatusCode = sut.StatusCode,
            Content = content
        });
    }
    
    [Fact]
    public async Task GetTodoById_ReturnsNotFoundWhenTodoDoesNotExist()
    {
        var client = fixture.CreateHttpClient();

        var sut = await client.GetAsync("/todos/9999");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, sut.StatusCode);
        await Verify(sut);
    }

    [Fact]
    public async Task DeleteTodoById_ReturnsSuccessWhenTodoExists()
    {
        var client = fixture.CreateHttpClient();

        var created = await (await client.PostAsJsonAsync("/todos", new { Title = "X", Order = 1, Completed = false }))
            .Content.ReadFromJsonAsync<Todo>();
        var sut = await client.DeleteAsync($"/todos/{created!.Id}");

        Assert.Equal(System.Net.HttpStatusCode.OK, sut.StatusCode);
        await Verify(sut);
        
    }
    [Fact]
    public async Task DeleteTodoById_ReturnsNotFoundWhenTodoDoesNotExists()
    {
        var client = fixture.CreateHttpClient();

        var sut = await client.DeleteAsync("/todos/9999");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, sut.StatusCode);
        await Verify(sut);
    }
    
    [Fact]
    public async Task DeleteAllTodos_ReturnsSuccess()
    {
        var client = fixture.CreateHttpClient();
        
        var sut = await client.DeleteAsync("/todos/");
    
        Assert.Equal(System.Net.HttpStatusCode.OK, sut.StatusCode);
        await Verify(sut);
    }
}
