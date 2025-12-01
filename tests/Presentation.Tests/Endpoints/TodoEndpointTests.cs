using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core.Serialization;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        };

        var sut = await fixture.Client.PostAsJsonAsync("/todos",
            testTodo);
        var content = await sut.Content.ReadFromJsonAsync<Todo>();

        Assert.Equal(System.Net.HttpStatusCode.Created,
            sut.StatusCode);

        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var persisted = await dbContext.Todos.SingleOrDefaultAsync(t => t.Id == content!.Id);

        Assert.NotNull(persisted);
        Assert.Equal(testTodo.Title, persisted.Title);
        Assert.Equal(testTodo.Order, persisted.Order);

        await Verify(new
        {
            sut.StatusCode,
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
        };
    
        var sut = await fixture.Client.PostAsJsonAsync("/todos",
            testTodo);
    
        Assert.Equal(System.Net.HttpStatusCode.BadRequest,
            sut.StatusCode);
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var persisted = await dbContext.Todos.ToListAsync();

        Assert.Empty(persisted);
        
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
            });

        await fixture.Client.PostAsJsonAsync("/todos",
            new
            {
                Title = "B",
                Order = 2,
            });
        
        var sut = await fixture.Client.GetAsync("/todos");
        var fromApi = sut.Content.ReadFromJsonAsync<List<Todo>>();

        Assert.Equal(System.Net.HttpStatusCode.OK,
            sut.StatusCode);
        
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var fromDb = await dbContext.Todos
            .OrderBy(t => t.Id)
            .ToListAsync();
        
        await Verify(new 
        {
            sut.StatusCode,
            Content = new
            {
                FromApi = fromApi,
                FromDb = fromDb
            }
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
            });
        var todoContent = await testTodo.Content.ReadFromJsonAsync<Todo>();
    
        var sut = await fixture.Client.GetAsync($"/todos/{todoContent!.Id}");
    
        Assert.Equal(System.Net.HttpStatusCode.OK,
            sut.StatusCode);
        
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var persisted = await dbContext.Todos.SingleOrDefaultAsync(t => t.Id == todoContent.Id);

        await Verify(new 
        {
            sut.StatusCode,
            Content = new
            {
                FromApi = todoContent,
                FromDb = persisted
            }
        });
    }
    
    [Fact]
    public async Task GetTodoById_ReturnsNotFoundWhenTodoDoesNotExist()
    {
        await fixture.ResetDatabaseAsync();

        var sut = await fixture.Client.GetAsync("/todos/9999");
    
        Assert.Equal(System.Net.HttpStatusCode.NotFound, sut.StatusCode);
        
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var exists = await dbContext.Todos.AnyAsync(t => t.Id == 9999);
        
        Assert.False(exists);

        await Verify(sut);
    }

    [Fact]
    public async Task UpdateTodoById_ReturnsSuccessWhenTodoExists()
    {
        await fixture.ResetDatabaseAsync();
        var testTodo = await fixture.Client.PostAsJsonAsync("/todos",
            new
            {
                Title = "X",
                Order = 1,
            });
        
        var created = await testTodo.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(testTodo);
        Assert.Equal(System.Net.HttpStatusCode.Created, testTodo.StatusCode);

        var patchOps = new[]
        {
            new { op = "replace", path = "/order", value = (object)5 },
            new { op = "replace", path = "/title", value = (object)"New Title" },
            new { op = "replace", path = "/isCompleted", value = (object)true },
        };
        
        var sut = await fixture.Client.PatchAsJsonAsync($"/todos/{created!.Id}", patchOps);
        var fromApi = await sut.Content.ReadFromJsonAsync<Todo>();

        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var persisted = await dbContext.Todos.SingleAsync(t => t.Id == created.Id);

        await Verify(new 
        {
            sut.StatusCode,
            Content = new
            {
                FromApi = fromApi,
                FromDb = persisted
            }
        });
    }
    
    [Fact]
    public async Task UpdateTodoById_ReturnsValidationFailedWhenInvalidPatchDocument()
    {
        await fixture.ResetDatabaseAsync();
        var testTodo = await fixture.Client.PostAsJsonAsync("/todos",
            new
            {
                Title = "X",
                Order = 1,
            });

        var created = await testTodo.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(testTodo);
        Assert.Equal(System.Net.HttpStatusCode.Created, testTodo.StatusCode);

        using var initialScope = fixture.CreateScope();
        var initialDb = initialScope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var original = await initialDb.Todos.AsNoTracking().SingleAsync(t => t.Id == created!.Id);

        var patchOps = new[]
        {
            new { op = "replace", path = "/order", value = (object)536 },
            new { op = "replae", path = "/order", value = (object)5 },
            new { op = "replace", path = "/title", value = (object)null! },
            new { op = "replace", path = "/isCopleted", value = (object)true }
        };

        var sut = await fixture.Client.PatchAsJsonAsync($"/todos/{created!.Id}", patchOps);

        var validationErrors = await sut.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var persisted = await dbContext.Todos.AsNoTracking().SingleAsync(t => t.Id == created.Id);

        await Verify(new
        {
            sut.StatusCode,
            Validation = validationErrors,
            Original = original,
            Persisted = persisted
        });
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
            });
        var todoContent = await testTodo.Content.ReadFromJsonAsync<Todo>();

        var sut = await fixture.Client.DeleteAsync($"/todos/{todoContent!.Id}");
    
        Assert.Equal(System.Net.HttpStatusCode.OK, sut.StatusCode);
        
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var exists = await dbContext.Todos.AnyAsync(t => t.Id == todoContent.Id);
        
        Assert.False(exists);

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

        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var fromDb = await dbContext.Todos
            .OrderBy(t => t.Id)
            .ToListAsync();
        
        Assert.Empty(fromDb);

        await Verify(sut);
    }
}
