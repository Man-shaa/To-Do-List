using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Argon;
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
        var apiResponse = await sut.Content.ReadFromJsonAsync<Todo>();

        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.SingleAsync(t => t.Id == apiResponse!.Id);

        await Verify(new
        {
            sut.StatusCode,
            FromApi = apiResponse,
            FromDb = persisted
        });
    }
    
    [Fact]
    public async Task CreateTodo_ReturnBadRequestWhenTitleIsEmpty()
    {
        var testTodo = new
        {
            Title = "",
            Order = 1,
        };
    
        var sut = await fixture.Client.PostAsJsonAsync("/todos",
            testTodo);
        var apiResponse = sut.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    
        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.ToListAsync();

        await Verify(new
        {
            sut.StatusCode,
            Content = new
            {
                FromApi = apiResponse,
                FromDb = persisted
            }
        });
    }
    
    [Fact]
    public async Task GetAllTodos_ReturnsSuccess()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var sut = await fixture.Client.GetAsync("/todos");
        var fromApi = sut.Content.ReadFromJsonAsync<List<Todo>>();

        await using var dbContext = fixture.CreateScopeDbContext();

        var persisted = await dbContext.Todos
            .OrderBy(t => t.Id)
            .ToListAsync();
        
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
    public async Task GetTodoById_ReturnsTodoOfId666()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();
    
        var sut = await fixture.Client.GetAsync($"/todos/666");
        var apiResponse = sut.Content.ReadFromJsonAsync<Todo>();
    
        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.SingleOrDefaultAsync(t => t.Id == 666);

        await Verify(new 
        {
            sut.StatusCode,
            Content = new
            {
                FromApi = apiResponse,
                FromDb = persisted
            }
        });
    }
    
    [Fact]
    public async Task GetTodoById_ReturnsNotFoundWhenTodoDoesNotExist()
    {
        await fixture.ResetDatabaseAsync();

        var sut = await fixture.Client.GetAsync("/todos/9999");
        var apiResponse = sut.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    
        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.AnyAsync(t => t.Id == 9999);
        
        await Verify(new
        {
            sut,
            IsTodoInDb = persisted
        });
    }

    [Fact]
    public async Task UpdateTodoById_ReturnsUpdatedTodoWhenTodoExists()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var initialTodo = await fixture.Client.GetAsync("/todos/666");
        var initialTodoContent = await initialTodo.Content.ReadFromJsonAsync<Todo>();
        
        var patchOps = new[]
        {
            new { op = "replace", path = "/order", value = (object)5 },
            new { op = "replace", path = "/title", value = (object)"New Title" },
            new { op = "replace", path = "/isCompleted", value = (object)true },
        };
        
        var sut = await fixture.Client.PatchAsJsonAsync($"/todos/{initialTodoContent!.Id}", patchOps);
        var apiResponse = await sut.Content.ReadFromJsonAsync<Todo>();

        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.SingleAsync(t => t.Id == initialTodoContent.Id);

        await Verify(new 
        {
            sut.StatusCode,
            Content = new
            {
                FromApi = apiResponse,
                FromDb = persisted
            }
        });
    }
    
    [Fact]
    public async Task UpdateTodoById_ReturnsValidationFailedWhenInvalidPatchDocument()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var initialTodo = await fixture.Client.GetAsync("/todos/666");
        var initialTodoContent = await initialTodo.Content.ReadFromJsonAsync<Todo>();

        await using var initialDb = fixture.CreateScopeDbContext();
        var original = await initialDb.Todos.AsNoTracking().SingleAsync(t => t.Id == initialTodoContent!.Id);

        var patchOps = new[]
        {
            new { op = "replace", path = "/order", value = (object)536 },
            new { op = "replae", path = "/order", value = (object)5 },
            new { op = "replace", path = "/title", value = (object)null! },
            new { op = "replace", path = "/isCopleted", value = (object)true }
        };

        var sut = await fixture.Client.PatchAsJsonAsync($"/todos/{initialTodoContent!.Id}", patchOps);

        var validationErrors = await sut.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.AsNoTracking().SingleAsync(t => t.Id == initialTodoContent.Id);

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
        await fixture.SeedInitialTodosAsync();

        var sut = await fixture.Client.DeleteAsync($"/todos/666");
    
        await using var dbContext = fixture.CreateScopeDbContext();

        var exists = await dbContext.Todos.AnyAsync(t => t.Id == 666);
        
        Assert.False(exists);

        await Verify(new
        {
            sut,
            IsTodoInDb = exists
        });
    }

    [Fact]
    public async Task DeleteTodoById_ReturnsNotFoundWhenTodoDoesNotExists()
    {
        await fixture.ResetDatabaseAsync();

        var sut = await fixture.Client.DeleteAsync("/todos/9999");
    
        await Verify(sut);
    }
    
    [Fact]
    public async Task DeleteAllTodos_ReturnsSuccess()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var sut = await fixture.Client.DeleteAsync("/todos/");
    
        await using var dbContext = fixture.CreateScopeDbContext();

        var fromDb = await dbContext.Todos
            .OrderBy(t => t.Id)
            .ToListAsync();
        
        Assert.Empty(fromDb);

        await Verify(sut);
    }
}
