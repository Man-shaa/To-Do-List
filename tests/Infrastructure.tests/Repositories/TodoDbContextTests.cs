using Application.Todos.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Presentation.Common.Constants;

namespace Infrastructure.Tests.Repositories;

public sealed class TodoDbContextTests
{
    private static TodoDbContext CreateDbContext()
    {
        var dbName = Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        
        return new TodoDbContext(options);
    }
    private static IOptions<SettingsOptions> CreateOptions(Uri baseUrl)
    {
        return Options.Create(new SettingsOptions
        {
            BaseUrl = baseUrl
        });
    }

    private static TodoRepository CreateRepository()
    {
        var dbContext = CreateDbContext();
        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));

        return new TodoRepository(options, dbContext);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        var repository = CreateRepository();

        var sut = await repository.GetAllAsync(CancellationToken.None);

        await Verify(sut);
    }
    [Fact]
    public async Task GetAllAsync_WhenHasItems_ReturnsListOfTwoTodos()
    {
        var dbContext = CreateDbContext();

        dbContext.Todos.AddRange(
            new Todo(1, "A", new Uri(ApiRoutes.HttpsBaseUrl + "/todos/1"), 1),
            new Todo(2, "B", new Uri(ApiRoutes.HttpsBaseUrl + "/todos/2"), 2)
        );
        await dbContext.SaveChangesAsync();

        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));
        var repository = new TodoRepository(options, dbContext);

        var sut = await repository.GetAllAsync(CancellationToken.None);
        
        await Verify(sut);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsTodo()
    {
        var dbContext = CreateDbContext();

        dbContext.Todos.Add(new Todo(10, "Title todo 10", new Uri(ApiRoutes.HttpsBaseUrl + "/todos/10"), 1));
        await dbContext.SaveChangesAsync();

        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));
        var repository = new TodoRepository(options, dbContext);

        var sut = await repository.GetByIdAsync(10, CancellationToken.None);

        await Verify(sut);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        var repository = CreateRepository();

        var sut = await repository.GetByIdAsync(999, CancellationToken.None);

        await Verify(sut);
    }

    [Fact]
    public async Task CreateAsync_AddsTodoAndPersists()
    {
        var dbContext = CreateDbContext();
        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));
        var repository = new TodoRepository(options, dbContext);

        var dto = new TodoCreateDto
        {
            Title = "New Todo",
            Order = 5
        };

        var sut = await repository.CreateAsync(dto, CancellationToken.None);

        await Verify(sut);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingTodo()
    {
        var dbContext = CreateDbContext();

        var original = new Todo(1, "Old", new Uri(ApiRoutes.HttpsBaseUrl + "/todos/1"), 1);
        dbContext.Todos.Add(original);
        await dbContext.SaveChangesAsync();

        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));
        var repository = new TodoRepository(options, dbContext);

        original.Title = "Updated";
        original.IsCompleted = true;

        var sut = await repository.UpdateAsync(original, CancellationToken.None);

        await Verify(sut);

    }

    [Fact]
    public async Task DeleteByIdAsync_WhenNullTodo_ReturnsFalseAndDoesNotChangeDb()
    {
        var dbContext = CreateDbContext();
        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));
        var repository = new TodoRepository(options, dbContext);

        var sut = await repository.DeleteByIdAsync(null, CancellationToken.None);

        await Verify(new { Result = sut });
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenExistingTodo_RemovesAndReturnsTrue()
    {
        var dbContext = CreateDbContext();
        var todo = new Todo(1, "To delete", new Uri(ApiRoutes.HttpsBaseUrl + "/todos/1"), 1);

        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync();

        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));
        var repository = new TodoRepository(options, dbContext);

        var sut = await repository.DeleteByIdAsync(todo, CancellationToken.None);
        
        await Verify(new { Result = sut });
    }

    [Fact]
    public async Task DeleteAllAsync_RemovesAllTodos_ReturnsEmpty()
    {
        var dbContext = CreateDbContext();

        dbContext.Todos.AddRange(
            new Todo(1, "A", new Uri(ApiRoutes.HttpsBaseUrl + "/todos/1"), 1),
            new Todo(2, "B", new Uri(ApiRoutes.HttpsBaseUrl + "/todos/2"), 2)
        );
        await dbContext.SaveChangesAsync();

        var options = CreateOptions(new Uri(ApiRoutes.HttpsBaseUrl));
        var repository = new TodoRepository(options, dbContext);

        await repository.DeleteAllAsync(CancellationToken.None);

        var sut = await repository.GetAllAsync(CancellationToken.None);
        await Verify(sut);
    }
}
