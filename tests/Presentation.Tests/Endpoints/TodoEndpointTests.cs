using System.Net.Http.Json;
using Domain.Entities;
using Presentation.Tests.fixture;

namespace Presentation.Tests.Endpoints;

[Collection("TodoApiFixtureCollection")]
public sealed class TodoEndpointTests(TodoApiFixture fixture)
    : IClassFixture<TodoApiFixture>, IAsyncLifetime
{
    public ValueTask InitializeAsync() => fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        
        
    [Fact]
    public async Task CreateTodo_ReturnsCreatedTodo()
    {
        await fixture.ResetDatabaseAsync();
        var testTodo = new
        {
            Title = "Test Todo",
            Order = 1,
            Completed = false
        };

        var sut = await fixture.Client.PostAsJsonAsync("/todos",
            testTodo);

        var content = await sut.Content.ReadFromJsonAsync<Todo>();
        Assert.Equal(System.Net.HttpStatusCode.Created,
            sut.StatusCode);
        await Verify(new
        {
            StatusCode = sut.StatusCode,
            Content = content
        });
    }
    
    [Fact]
    public async Task CreateTodo_ReturnBadRequestWhenTitleIsEmpty()
    {
        await fixture.ResetDatabaseAsync();
        var testTodo = new
        {
            Title = "",
            Order = 1,
            Completed = false
        };
    
        var sut = await fixture.Client.PostAsJsonAsync("/todos",
            testTodo);
    
        Assert.Equal(System.Net.HttpStatusCode.BadRequest,
            sut.StatusCode);
        await Verify(sut);
    }
    
    [Fact]
    public async Task GetAllTodos_ReturnsSuccess()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.Client.PostAsJsonAsync("/todos",
            new
            {
                Title = "A",
                Order = 1,
                Completed = false
            });
        await fixture.Client.PostAsJsonAsync("/todos",
            new
            {
                Title = "B",
                Order = 2,
                Completed = false
            });
        
        var sut = await fixture.Client.GetAsync("/todos");
    
        var content = sut.Content.ReadFromJsonAsync<List<Todo>>();
        Assert.Equal(System.Net.HttpStatusCode.OK,
            sut.StatusCode);
        await Verify(new 
        {
            StatusCode = sut.StatusCode,
            Content = content
        });
    }
    
    [Fact]
    public async Task GetTodoById_ReturnsTodoOfIdOne()
    {
        await fixture.ResetDatabaseAsync();
        var testTodo = await fixture.Client.PostAsJsonAsync("/todos",
            new
            {
                Title = "X",
                Order = 1,
                Completed = false
            });
        var todoContent = await testTodo.Content.ReadFromJsonAsync<Todo>();
    
        var sut = await fixture.Client.GetAsync($"/todos/{todoContent!.Id}");
    
        Assert.Equal(System.Net.HttpStatusCode.OK,
            sut.StatusCode);
        await Verify(new 
        {
            StatusCode = sut.StatusCode,
            Content = todoContent
        });
    }
    
    [Fact]
    public async Task GetTodoById_ReturnsNotFoundWhenTodoDoesNotExist()
    {
        await fixture.ResetDatabaseAsync();

        var sut = await fixture.Client.GetAsync("/todos/9999");
    
        Assert.Equal(System.Net.HttpStatusCode.NotFound, sut.StatusCode);
        await Verify(sut);
    }
    
    [Fact]
    public async Task DeleteTodoById_ReturnsSuccessWhenTodoExists()
    {
        await fixture.ResetDatabaseAsync();
        var testTodo = await fixture.Client.PostAsJsonAsync("/todos",
            new
            {
                Title = "X",
                Order = 1,
                Completed = false
            });
        var todoContent = await testTodo.Content.ReadFromJsonAsync<Todo>();

        var sut = await fixture.Client.DeleteAsync($"/todos/{todoContent!.Id}");
    
        Assert.Equal(System.Net.HttpStatusCode.OK, sut.StatusCode);
        await Verify(sut);
        
    }
    [Fact]
    public async Task DeleteTodoById_ReturnsNotFoundWhenTodoDoesNotExists()
    {
        await fixture.ResetDatabaseAsync();

        var sut = await fixture.Client.DeleteAsync("/todos/9999");
    
        Assert.Equal(System.Net.HttpStatusCode.NotFound, sut.StatusCode);
        await Verify(sut);
    }
    
    [Fact]
    public async Task DeleteAllTodos_ReturnsSuccess()
    {
        await fixture.ResetDatabaseAsync();

        var sut = await fixture.Client.DeleteAsync("/todos/");
    
        Assert.Equal(System.Net.HttpStatusCode.OK, sut.StatusCode);
        await Verify(sut);
    }
}
