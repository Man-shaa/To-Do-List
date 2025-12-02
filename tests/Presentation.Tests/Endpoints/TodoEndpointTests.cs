using System.Globalization;
using System.Net.Http.Json;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Presentation.Common.Constants;
using Presentation.Tests.fixture;

namespace Presentation.Tests.Endpoints;

[Collection("TodoApiFixtureCollection")]
public sealed class TodoEndpointTests(TodoApiFixture fixture)
    : IClassFixture<TodoApiFixture>, IAsyncLifetime
{
    private readonly string _baseUrl = $"{ApiRoutes.Root}"
        .Replace("{version:apiVersion}", 1.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
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

        var url = $"{_baseUrl}/{ApiRoutes.Todos.Create}";
        var sut = await fixture.Client.PostAsJsonAsync(url,
            testTodo);

        var apiResponse = await sut.Content.ReadFromJsonAsync<Todo>();

        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.FindAsync(apiResponse!.Id);

        await Verify(new
        {
            sut,
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
    
        var url = $"{_baseUrl}/{ApiRoutes.Todos.Create}";
        var sut = await fixture.Client.PostAsJsonAsync(url,
            testTodo);

        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.ToListAsync();

        await Verify(new
        {
            sut,
            FromDb = persisted
        });
    }
    
    [Fact]
    public async Task GetAllTodos_ReturnsSuccess()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var url = $"{_baseUrl}/{ApiRoutes.Todos.GetAll}";
        var sut = await fixture.Client.GetAsync(url);

        await using var dbContext = fixture.CreateScopeDbContext();

        await Verify(sut);
    }
    
    [Fact]
    public async Task GetTodoById_ReturnsTodoOfId666()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();
    
        var url = $"{_baseUrl}/{ApiRoutes.Todos.GetById}"
            .Replace("{todoId}", 666.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var sut = await fixture.Client.GetAsync(url);
    
        await using var dbContext = fixture.CreateScopeDbContext();

        await Verify(sut);
    }

    [Fact]
    public async Task GetTodoById_ReturnsNotFoundWhenTodoDoesNotExist()
    {
        await fixture.ResetDatabaseAsync();

        var url = $"{_baseUrl}/{ApiRoutes.Todos.GetById}"
            .Replace("{todoId}", 9999.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var sut = await fixture.Client.GetAsync(url);

        await using var dbContext = fixture.CreateScopeDbContext();

        await Verify(sut);
    }

    [Fact]
    public async Task UpdateTodoById_ReturnsUpdatedTodoWhenTodoExists()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var getAsyncUrl = $"{_baseUrl}/{ApiRoutes.Todos.GetById}"
            .Replace("{todoId}", 666.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var initialTodo = await fixture.Client.GetAsync(getAsyncUrl);
        var initialTodoContent = await initialTodo.Content.ReadFromJsonAsync<Todo>();
        
        var patchOps = new[]
        {
            new { op = "replace", path = "/order", value = (object)5 },
            new { op = "replace", path = "/title", value = (object)"New Title" },
            new { op = "replace", path = "/isCompleted", value = (object)true },
        };

        var url = $"{_baseUrl}/{ApiRoutes.Todos.UpdateById}"
            .Replace("{todoId}", initialTodoContent!.Id.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var sut = await fixture.Client.PatchAsJsonAsync(url, patchOps);

        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.FindAsync(initialTodoContent.Id);

        await Verify(new 
        {
            sut,
            FromDb = persisted
        });
    }
    
    [Fact]
    public async Task UpdateTodoById_ReturnsValidationFailedWhenInvalidPatchDocument()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var getAsyncUrl = $"{_baseUrl}/{ApiRoutes.Todos.GetById}"
            .Replace("{todoId}", 666.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var initialTodo = await fixture.Client.GetAsync(getAsyncUrl);
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

        var url = $"{_baseUrl}/{ApiRoutes.Todos.UpdateById}"
            .Replace("{todoId}", initialTodoContent!.Id.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var sut = await fixture.Client.PatchAsJsonAsync(url, patchOps);

        await using var dbContext = fixture.CreateScopeDbContext();
        var persisted = await dbContext.Todos.AsNoTracking().SingleAsync(t => t.Id == initialTodoContent.Id);

        await Verify(new
        {
            sut,
            Original = original,
            Persisted = persisted
        });
    }
    
    [Fact]
    public async Task DeleteTodoById_ReturnsSuccessWhenTodoExists()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var url = $"{_baseUrl}/{ApiRoutes.Todos.DeleteById}"
            .Replace("{todoId}", 666.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var sut = await fixture.Client.DeleteAsync(url);
    
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

        var url = $"{_baseUrl}/{ApiRoutes.Todos.DeleteById}"
            .Replace("{todoId}", 9999.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var sut = await fixture.Client.DeleteAsync(url);
    
        await Verify(sut);
    }
    
    [Fact]
    public async Task DeleteAllTodos_ReturnsSuccess()
    {
        await fixture.ResetDatabaseAsync();
        await fixture.SeedInitialTodosAsync();

        var url = $"{_baseUrl}/{ApiRoutes.Todos.DeleteAll}";
        var sut = await fixture.Client.DeleteAsync(url);
    
        await using var dbContext = fixture.CreateScopeDbContext();

        var fromDb = await dbContext.Todos
            .OrderBy(t => t.Id)
            .ToListAsync();
        
        Assert.Empty(fromDb);

        await Verify(sut);
    }
}
